using System;
using System.Collections.Generic;

namespace Portfolyo.Data
{
    public partial class AboutMe2Table
    {
        public int DetailId { get; set; }
        public int? AboutId { get; set; }
        public string? DetailType { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public virtual AboutMeTable Detail { get; set; } = null!;
    }
}
