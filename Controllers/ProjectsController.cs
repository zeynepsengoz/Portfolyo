using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Portfolyo.Data;
using Portfolyo.Services;
using PortfolyoDbContext;
using System.Text;

namespace Portfolyo.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly portfolyodbContext _portfolyodbContext;
        private readonly IProjectImageStorageService _imageStorageService;

        public ProjectsController(
            portfolyodbContext portfolyodbcontext,
            IProjectImageStorageService imageStorageService)
        {
            _portfolyodbContext = portfolyodbcontext;
            _imageStorageService = imageStorageService;
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
        public async Task<IActionResult> ProjectCreate(
            ProjectsTable projectsTable,
            IFormFile? previewImageFile,
            List<IFormFile>? detailImageFiles)
        {
            projectsTable.GithubUrl = NormalizeOptionalUrl(projectsTable.GithubUrl);

            if (!ModelState.IsValid)
            {
                PopulateCategorySelectList();
                return View(projectsTable);
            }

            if (previewImageFile != null)
            {
                projectsTable.Image = await _imageStorageService.SaveUploadedImageAsync(previewImageFile, "project-previews");
            }

            projectsTable.DisplayOrder = GetNextDisplayOrder();

            _portfolyodbContext.ProjectsTables.Add(projectsTable);
            await _portfolyodbContext.SaveChangesAsync();

            if (detailImageFiles != null && detailImageFiles.Count > 0)
            {
                var sort = 1;
                foreach (var imageFile in detailImageFiles)
                {
                    var imagePath = await _imageStorageService.SaveUploadedImageAsync(imageFile, "project-gallery");
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

                await _portfolyodbContext.SaveChangesAsync();
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
        public async Task<IActionResult> ProjectUpdate(
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
                var newPreviewPath = await _imageStorageService.SaveUploadedImageAsync(previewImageFile, "project-previews");
                if (!string.IsNullOrWhiteSpace(newPreviewPath))
                {
                    await _imageStorageService.DeleteUploadedImageAsync(existingProject.Image);
                    existingProject.Image = newPreviewPath;
                }
            }

            if (removeImageIds != null && removeImageIds.Count > 0)
            {
                var imagesToRemove = existingProject.ProjectImages
                    .Where(x => removeImageIds.Contains(x.ProjectImageId))
                    .ToList();

                foreach (var image in imagesToRemove)
                {
                    await _imageStorageService.DeleteUploadedImageAsync(image.ImagePath);
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
                    var imagePath = await _imageStorageService.SaveUploadedImageAsync(imageFile, "project-gallery");
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

            await _portfolyodbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ProjectDelete(int id)
        {
            var project = _portfolyodbContext.ProjectsTables
                .Include(x => x.ProjectImages)
                .FirstOrDefault(x => x.ProjectId == id);

            if (project == null)
            {
                return RedirectToAction("Index");
            }

            await _imageStorageService.DeleteUploadedImageAsync(project.Image);
            foreach (var image in project.ProjectImages)
            {
                await _imageStorageService.DeleteUploadedImageAsync(image.ImagePath);
            }

            _portfolyodbContext.ProjectsTables.Remove(project);
            await _portfolyodbContext.SaveChangesAsync();
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
            EnsureDefaultProjectCategories();

            ViewBag.Category = _portfolyodbContext.CategoryTables
                .Select(x => new SelectListItem
                {
                    Text = x.CategoryName,
                    Value = x.CategoryId.ToString()
                })
                .ToList();
        }

        private void EnsureDefaultProjectCategories()
        {
            var defaults = new[]
            {
                "Web",
                "Oyun",
                "Blender",
                "Pixel Art",
                "Character Design"
            };

            var existingNormalized = _portfolyodbContext.CategoryTables
                .Select(x => x.CategoryName)
                .AsEnumerable()
                .Select(NormalizeCategoryKey)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var hasChanges = false;
            foreach (var category in defaults)
            {
                if (existingNormalized.Contains(NormalizeCategoryKey(category)))
                {
                    continue;
                }

                _portfolyodbContext.CategoryTables.Add(new CategoryTable
                {
                    CategoryName = category
                });
                hasChanges = true;
            }

            if (hasChanges)
            {
                _portfolyodbContext.SaveChanges();
            }
        }

        private static string NormalizeCategoryKey(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized = value.Trim().ToLowerInvariant()
                .Replace("ı", "i")
                .Replace("ş", "s")
                .Replace("ğ", "g")
                .Replace("ü", "u")
                .Replace("ö", "o")
                .Replace("ç", "c");

            var sb = new StringBuilder(normalized.Length);
            foreach (var ch in normalized)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                }
            }

            return sb.ToString();
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

    }
}

