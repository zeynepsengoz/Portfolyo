using System;
using System.Collections.Generic;

namespace Portfolyo.Data
{
    public partial class TestimonialTable
    {
        public int TestimonialId { get; set; }
        public string? CustomerName { get; set; }
        public string? JobTitle { get; set; }
        public string? Comment { get; set; }
        public string? ImagePath { get; set; }
        public int? OrderNo { get; set; }
    }
}
