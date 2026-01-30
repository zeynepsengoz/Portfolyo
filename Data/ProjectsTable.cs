using System;
using System.Collections.Generic;

namespace Portfolyo.Data
{
    public partial class ProjectsTable
    {
        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public string? Title { get; set; }
        public string? Image { get; set; }

        public string? Description { get; set; }
        public string? GithubUrl { get; set; }

        public int? CategoryId { get; set; }
        public virtual CategoryTable? Category { get; set; }
    }
}
