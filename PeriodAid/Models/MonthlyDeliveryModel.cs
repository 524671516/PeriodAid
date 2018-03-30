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
        public virtual DbSet<MD_OrderDetail> MD_OrderDetail { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MD_Customer>().HasMany(m => m.MD_Order).WithRequired(m => m.MD_Customer).HasForeignKey(m => m.customer_id).WillCascadeOnDelete(false);
            modelBuilder.Entity<MD_Order>().HasMany(m => m.MD_OrderDetail).WithRequired(m => m.MD_Order).HasForeignKey(m => m.order_id).WillCascadeOnDelete(false);
        }
    }

    [Table("MD_Customer")]
    public partial class MD_Customer
    {
        public int Id { get; set; }
        [StringLength(64)]
        public string customer_name { get; set; }
        [StringLength(32)]
        public string customer_tel { get; set; }
        [StringLength(32)]
        public string customer_area { get; set; }
        [StringLength(128)]
        public string customer_address { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MD_Order> MD_Order { get; set; }

    }

    [Table("MD_Order")]
    public partial class MD_Order
    {
        public int Id { get; set; }
        [StringLength(128)]
        public string order_code { get; set; }

        public int quantity { get; set; }

        public double total_amount { get; set; }

        public DateTime receiver_date { get; set; }

        [StringLength(32)]
        public string receiver_area { get; set; }
        [StringLength(128)]
        public string receiver_address { get; set; }

        public int receiver_status { get; set; }

        public int order_status { get; set; }
        [StringLength(128)]
        public string express_information { get; set; }
        [StringLength(128)]
        public string remark { get; set; }

        public int customer_id { get; set; }

        public int parentOrder_id { get; set; }

        public int times { get; set; }

        public virtual MD_Customer MD_Customer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MD_OrderDetail> MD_OrderDetail { get; set; }
    }

    [Table("MD_OrderDetail")]
    public partial class MD_OrderDetail
    {
        public int Id { get; set; }
        
        [StringLength(64)]
        public string product_code { get; set; }
        [StringLength(64)]
        public string product_name { get; set; }

        public int total_quantity { get; set; }

        public int order_id { get; set; }

        public virtual MD_Order MD_Order { get; set; }

    }
    
}
