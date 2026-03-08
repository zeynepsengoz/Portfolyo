namespace Portfolyo.Data
{
    public class ProjectImageTable
    {
        public int ProjectImageId { get; set; }
        public int ProjectId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        public virtual ProjectsTable? Project { get; set; }
    }
}
