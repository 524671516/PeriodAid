namespace PeriodAid.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VM_Department
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VM_Department()
        {
            VM_Employee = new HashSet<VM_Employee>();
        }

        public int Id { get; set; }

        [StringLength(32)]
        public string Department_Name { get; set; }

        [StringLength(16)]
        public string Department_Type { get; set; }

        public int Department_Status { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_Employee> VM_Employee { get; set; }
    }
}
