namespace PeriodAid.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VM_Employee
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VM_Employee()
        {
            VM_Comment = new HashSet<VM_Comment>();
            VM_Company = new HashSet<VM_Company>();
            VM_VisitRecord = new HashSet<VM_VisitRecord>();
            VM_VisitRecord1 = new HashSet<VM_VisitRecord>();
        }

        public int Id { get; set; }

        [StringLength(32)]
        public string Employee_Name { get; set; }

        [StringLength(16)]
        public string Employee_Type { get; set; }

        public int Employee_Status { get; set; }

        public int Department_Id { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_Comment> VM_Comment { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_Company> VM_Company { get; set; }

        public virtual VM_Department VM_Department { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_VisitRecord> VM_VisitRecord { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_VisitRecord> VM_VisitRecord1 { get; set; }
    }
}
