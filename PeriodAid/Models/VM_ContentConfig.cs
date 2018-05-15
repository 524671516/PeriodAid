namespace PeriodAid.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VM_ContentConfig
    {
        public int Id { get; set; }

        [StringLength(16)]
        public string Content_Name { get; set; }

        [StringLength(128)]
        public string Content_Detail { get; set; }
    }
}
