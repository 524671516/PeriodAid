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
    public class MonthlyDeliveryModel: DbContext
    {
        public MonthlyDeliveryModel()
            : base("name=MonthlyDeliveryConnection")
        {
        }
        public virtual DbSet<MD_Customer> MD_Customer { get; set; }
        public virtual DbSet<MD_Order> MD_Order { get; set; }
        public virtual DbSet<MD_SubOrder> MD_SubOrder { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<CRM_Customer>().HasMany(m => m.CRM_Contract).WithRequired(m => m.CRM_Customer).HasForeignKey(m => m.customer_id).WillCascadeOnDelete(false);
        }
    }

    [Table("MD_Customer")]
    public partial class MD_Customer
    {
        public int Id { get; set; }
        
    }

    [Table("MD_Order")]
    public partial class MD_Order
    {
        public int Id { get; set; }

    }

    [Table("MD_SubOrder")]
    public partial class MD_SubOrder
    {
        public int Id { get; set; }

    }
}
