using System;
using System.Collections.Generic;

namespace Portfolyo.Data
{
    public partial class AboutMeTable
    {
        public int AboutId { get; set; }
        public string? NameSurname { get; set; }
        public string? JobTitle { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }

        public string? ShortDescription { get; set; }




        public virtual AboutMe2Table? AboutMe2Table { get; set; }


    }
}

