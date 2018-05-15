namespace PeriodAid.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class VM_Contact
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string Contact_Name { get; set; }

        [StringLength(16)]
        public string Departmental_Positions { get; set; }

        [StringLength(16)]
        public string Contact_Mobile { get; set; }

        [StringLength(16)]
        public string Contact_WeChat { get; set; }

        public int Company_Id { get; set; }

        public virtual VM_Company VM_Company { get; set; }
    }
}
