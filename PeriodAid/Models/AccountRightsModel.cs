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
        public virtual DbSet<AR_Users> AR_Users { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<MD_Product>().HasMany(m => m.MD_Order).WithRequired(m => m.MD_Product).HasForeignKey(m => m.product_id).WillCascadeOnDelete(false);
        }
    }

    [Table("AR_Users")]
    public partial class AR_Users
    {
        public int Id { get; set; }
        [StringLength(64)]
        public string NickName { get; set; }
        [StringLength(128)]
        public string Email { get; set; }
        // 0 正常 -1 离职
        public int Status { get; set; }
        // 0 普通 1 主管 2 超管
        public int Type { get; set; }
    }
    
}
