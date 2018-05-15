namespace PeriodAid.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VM_Company
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VM_Company()
        {
            VM_Contact = new HashSet<VM_Contact>();
            VM_VisitRecord = new HashSet<VM_VisitRecord>();
        }

        public int Id { get; set; }

        [StringLength(32)]
        public string Company_Name { get; set; }

        [StringLength(16)]
        public string Company_Type { get; set; }

        [StringLength(32)]
        public string Company_Source { get; set; }

        [StringLength(128)]
        public string Company_Address { get; set; }

        [StringLength(128)]
        public string Source_Name { get; set; }

        [StringLength(16)]
        public string Region { get; set; }

        [StringLength(16)]
        public string Source_Type { get; set; }

        public int Dedicated_Warehouse { get; set; }

        public decimal Sales_Amount { get; set; }

        [StringLength(128)]
        public string Agent_FamousBrand { get; set; }

        public int Special_Source { get; set; }

        [StringLength(16)]
        public string SpecialSource_Detail { get; set; }

        public string Company_Phone { get; set; }

        public int Company_Status { get; set; }

        public DateTime? Update_Time { get; set; }

        public int UpdateEmployee_Id { get; set; }

        public int Employee_Id { get; set; }

        public virtual VM_Employee VM_Employee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_Contact> VM_Contact { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_VisitRecord> VM_VisitRecord { get; set; }
    }
}
