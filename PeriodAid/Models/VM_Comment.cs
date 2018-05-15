namespace PeriodAid.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VM_Comment
    {
        public int Id { get; set; }

        public int VisitRecord_Id { get; set; }

        [StringLength(1024)]
        public string Comment_Detail { get; set; }

        public int Employee_Id { get; set; }

        public virtual VM_Employee VM_Employee { get; set; }

        public virtual VM_VisitRecord VM_VisitRecord { get; set; }
    }
}
