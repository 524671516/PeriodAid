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
        public virtual DbSet<MD_Order> MD_Order { get; set; }
        public virtual DbSet<MD_Product> MD_Product { get; set; }
        public virtual DbSet<MD_Record> MD_Record { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MD_Product>().HasMany(m => m.MD_Order).WithRequired(m => m.MD_Product).HasForeignKey(m => m.product_id).WillCascadeOnDelete(false);
        }
    }
    
    [Table("MD_Order")]
    public partial class MD_Order
    {
        public int Id { get; set; }
        [StringLength(128)]
        public string order_code { get; set; }
        // 数量
        public int qty { get; set; }
        // 总金额
        public decimal amount { get; set; }

        public decimal payment { get; set; }

        public decimal discount_fee { get; set; }

        public decimal payment_amount { get; set; }

        public DateTime? receiver_date { get; set; }
        [StringLength(32)]
        public string receiver_area { get; set; }
        [StringLength(128)]
        public string receiver_address { get; set; }
        [StringLength(32)]
        public string receiver_tel { get; set; }
        [StringLength(128)]
        public string receiver_name { get; set; }

        [StringLength(128)]
        public string express_information { get; set; }
        [StringLength(128)]
        public string remark { get; set; }
        [StringLength(256)]
        public string vip_code { get; set; }
        
        public int parentOrder_id { get; set; }

        public int receiver_times { get; set; }
        // 邮寄状态
        public int delivery_state { get; set; }
        //  0 分批 1 合并
        public int order_status { get; set; }
        // 0 未推送 1已推送
        public int upload_status { get; set; }

        public int product_id { get; set; }

        public virtual MD_Product MD_Product { get; set; }
    }

    [Table("MD_Product")]
    public partial class MD_Product
    {
        public int Id { get; set; }
        
        [StringLength(64)]
        public string product_code { get; set; }
        [StringLength(64)]
        public string product_name { get; set; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MD_Order> MD_Order { get; set; }

    }

    [Table("MD_Record")]
    public partial class MD_Record
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string record_type { get; set; }
        
        public string record_detail { get; set; }

        public DateTime? record_date { get; set; }
    }

    public static class OrderInfo
    {
    }


}
