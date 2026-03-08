using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portfolyo.Data;
using PortfolyoDbContext;

namespace Portfolyo.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly portfolyodbContext _portfolyodbContext;
        private readonly IWebHostEnvironment _environment;

        public ProjectsController(portfolyodbContext portfolyodbcontext, IWebHostEnvironment environment)
        {
            _portfolyodbContext = portfolyodbcontext;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var value = _portfolyodbContext.ProjectsTables
                .Include(x => x.Category)
                .Include(x => x.ProjectImages)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ProjectId)
                .ToList();

            return View(value);
        }

        [HttpGet]
        public IActionResult ProjectCreate()
        {
            PopulateCategorySelectList();
            return View();
        }

        [HttpPost]
        public IActionResult ProjectCreate(
            ProjectsTable projectsTable,
            IFormFile? previewImageFile,
            List<IFormFile>? detailImageFiles)
        {
            projectsTable.GithubUrl = NormalizeOptionalUrl(projectsTable.GithubUrl);

            if (previewImageFile != null)
            {
                projectsTable.Image = SaveUploadedImage(previewImageFile, "project-previews");
            }

            if (!ModelState.IsValid)
            {
                PopulateCategorySelectList();
                return View(projectsTable);
            }

            projectsTable.DisplayOrder = GetNextDisplayOrder();

            _portfolyodbContext.ProjectsTables.Add(projectsTable);
            _portfolyodbContext.SaveChanges();

            if (detailImageFiles != null && detailImageFiles.Count > 0)
            {
                var sort = 1;
                foreach (var imageFile in detailImageFiles)
                {
                    var imagePath = SaveUploadedImage(imageFile, "project-gallery");
                    if (string.IsNullOrWhiteSpace(imagePath))
                    {
                        continue;
                    }

                    _portfolyodbContext.ProjectImageTables.Add(new ProjectImageTable
                    {
                        ProjectId = projectsTable.ProjectId,
                        ImagePath = imagePath,
                        SortOrder = sort++
                    });
                }

                _portfolyodbContext.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ProjectUpdate(int id)
        {
            var project = _portfolyodbContext.ProjectsTables
                .Include(x => x.ProjectImages)
                .FirstOrDefault(x => x.ProjectId == id);

            if (project == null)
            {
                return NotFound();
            }

            PopulateCategorySelectList();
            return View(project);
        }

        [HttpPost]
        public IActionResult ProjectUpdate(
            ProjectsTable projectsTable,
            IFormFile? previewImageFile,
            List<IFormFile>? detailImageFiles,
            List<int>? removeImageIds)
        {
            projectsTable.GithubUrl = NormalizeOptionalUrl(projectsTable.GithubUrl);

            if (!ModelState.IsValid)
            {
                var invalidProject = _portfolyodbContext.ProjectsTables
                    .Include(x => x.ProjectImages)
                    .FirstOrDefault(x => x.ProjectId == projectsTable.ProjectId) ?? projectsTable;

                PopulateCategorySelectList();
                return View(invalidProject);
            }

            var existingProject = _portfolyodbContext.ProjectsTables
                .Include(x => x.ProjectImages)
                .FirstOrDefault(x => x.ProjectId == projectsTable.ProjectId);

            if (existingProject == null)
            {
                return NotFound();
            }

            existingProject.ProjectName = projectsTable.ProjectName;
            existingProject.Title = projectsTable.Title;
            existingProject.Description = projectsTable.Description;
            existingProject.GithubUrl = projectsTable.GithubUrl;
            existingProject.CategoryId = projectsTable.CategoryId;

            if (previewImageFile != null)
            {
                DeleteUploadedImage(existingProject.Image);
                existingProject.Image = SaveUploadedImage(previewImageFile, "project-previews");
            }

            if (removeImageIds != null && removeImageIds.Count > 0)
            {
                var imagesToRemove = existingProject.ProjectImages
                    .Where(x => removeImageIds.Contains(x.ProjectImageId))
                    .ToList();

                foreach (var image in imagesToRemove)
                {
                    DeleteUploadedImage(image.ImagePath);
                    _portfolyodbContext.ProjectImageTables.Remove(image);
                }
            }

            if (detailImageFiles != null && detailImageFiles.Count > 0)
            {
                var currentMaxOrder = existingProject.ProjectImages.Any()
                    ? existingProject.ProjectImages.Max(x => x.SortOrder)
                    : 0;

                foreach (var imageFile in detailImageFiles)
                {
                    var imagePath = SaveUploadedImage(imageFile, "project-gallery");
                    if (string.IsNullOrWhiteSpace(imagePath))
                    {
                        continue;
                    }

                    _portfolyodbContext.ProjectImageTables.Add(new ProjectImageTable
                    {
                        ProjectId = existingProject.ProjectId,
                        ImagePath = imagePath,
                        SortOrder = ++currentMaxOrder
                    });
                }
            }

            _portfolyodbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ProjectDelete(int id)
        {
            var project = _portfolyodbContext.ProjectsTables
                .Include(x => x.ProjectImages)
                .FirstOrDefault(x => x.ProjectId == id);

            if (project == null)
            {
                return RedirectToAction("Index");
            }

            DeleteUploadedImage(project.Image);
            foreach (var image in project.ProjectImages)
            {
                DeleteUploadedImage(image.ImagePath);
            }

            _portfolyodbContext.ProjectsTables.Remove(project);
            _portfolyodbContext.SaveChanges();
            NormalizeProjectOrder();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MoveUp(int id)
        {
            NormalizeProjectOrder();

            var projects = _portfolyodbContext.ProjectsTables
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ProjectId)
                .ToList();

            var index = projects.FindIndex(x => x.ProjectId == id);
            if (index <= 0)
            {
                return RedirectToAction("Index");
            }

            var current = projects[index];
            var previous = projects[index - 1];
            (current.DisplayOrder, previous.DisplayOrder) = (previous.DisplayOrder, current.DisplayOrder);
            _portfolyodbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MoveDown(int id)
        {
            NormalizeProjectOrder();

            var projects = _portfolyodbContext.ProjectsTables
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ProjectId)
                .ToList();

            var index = projects.FindIndex(x => x.ProjectId == id);
            if (index < 0 || index >= projects.Count - 1)
            {
                return RedirectToAction("Index");
            }

            var current = projects[index];
            var next = projects[index + 1];
            (current.DisplayOrder, next.DisplayOrder) = (next.DisplayOrder, current.DisplayOrder);
            _portfolyodbContext.SaveChanges();

            return RedirectToAction("Index");
        }

        private void PopulateCategorySelectList()
        {
            ViewBag.Category = _portfolyodbContext.CategoryTables
                .Select(x => new SelectListItem
                {
                    Text = x.CategoryName,
                    Value = x.CategoryId.ToString()
                })
                .ToList();
        }

        private int GetNextDisplayOrder()
        {
            return _portfolyodbContext.ProjectsTables.Any()
                ? _portfolyodbContext.ProjectsTables.Max(x => x.DisplayOrder) + 1
                : 1;
        }

        private void NormalizeProjectOrder()
        {
            var projects = _portfolyodbContext.ProjectsTables
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.ProjectId)
                .ToList();

            for (var i = 0; i < projects.Count; i++)
            {
                projects[i].DisplayOrder = i + 1;
            }

            _portfolyodbContext.SaveChanges();
        }

        private static string? NormalizeOptionalUrl(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        private string? SaveUploadedImage(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

            if (!allowedExtensions.Contains(extension))
            {
                return null;
            }

            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", folderName);
            Directory.CreateDirectory(uploadsRoot);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsRoot, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            file.CopyTo(stream);

            return $"/uploads/{folderName}/{fileName}";
        }

        private void DeleteUploadedImage(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            if (!relativePath.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var trimmed = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_environment.WebRootPath, trimmed);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
