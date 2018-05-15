namespace PeriodAid.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VM_VisitRecord
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VM_VisitRecord()
        {
            VM_Comment = new HashSet<VM_Comment>();
            VM_Employee1 = new HashSet<VM_Employee>();
        }

        public int Id { get; set; }

        public DateTime? Visit_Time { get; set; }

        [StringLength(128)]
        public string Intentional_Products { get; set; }

        public int Smoothly { get; set; }

        public int Reply_Status { get; set; }

        public int Cooperation_Intention { get; set; }

        public decimal Intentional_Funds { get; set; }

        public string NoIntention_Reason { get; set; }

        public DateTime? ExpectedDelivery_Time { get; set; }

        public decimal ExpectedDelivery_Funds { get; set; }

        [StringLength(128)]
        public string Core_Problem { get; set; }

        [StringLength(128)]
        public string Support { get; set; }

        [StringLength(128)]
        public string Reply_Detail { get; set; }

        public int CustomerReply_Status { get; set; }

        public DateTime? CustomerReply_Time { get; set; }

        [StringLength(128)]
        public string CustomerReply_Detail { get; set; }

        public int NextVisit_Type { get; set; }

        public DateTime? NextVisit_Time { get; set; }

        public string NextVisit_Detail { get; set; }

        public int status { get; set; }

        public int Employee_Id { get; set; }

        public int Company_Id { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_Comment> VM_Comment { get; set; }

        public virtual VM_Company VM_Company { get; set; }

        public virtual VM_Employee VM_Employee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_Employee> VM_Employee1 { get; set; }
    }
}
