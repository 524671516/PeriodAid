using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public class AccountRightsModel : DbContext
    {
        public AccountRightsModel()
            : base("name=AccountRightsConnection")
        {
        }
        public virtual DbSet<AR_Roles> AR_Roles { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //public virtual DbSet<CRM_Contract> CRM_Contract { get; set; }
        }
    }
    [Table("AR_Roles")]
    public partial class AR_Roles
    {
        public int Id { get; set; }

        public string Key { get; set; }
        [StringLength(32)]
        public string Value { get; set; }

    }
}
