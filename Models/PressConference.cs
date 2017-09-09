using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PeriodAid.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class PressConference : DbContext
    {
        public PressConference() : base("name=SqzEventConnection")
        {
        }

        public virtual DbSet<PC_Order> PC_Order { get; set; }

        public virtual DbSet<PC_OrderProduct> PC_OrderProduct { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PC_Order>()
                .HasMany(e => e.PC_OrderProduct)
                .WithRequired(e => e.PC_Order)
                .HasForeignKey(e => e.OrderId)
                .WillCascadeOnDelete(true);
        }
    }
    public partial class PC_Order
    {
        public int Id { get; set; }

        // 用户名
        public string UserName { get; set; }

        // 订单金额
        public int OrderAmount { get; set; }

        // 状态
        public int Status { get; set; }

        // 创建时间
        public DateTime ApplicationTime { get; set; }

        // 支付金额
        public int PurchaseAmount { get; set; }

        // 支付时间
        public DateTime PurchaseTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PC_OrderProduct> PC_OrderProduct { get; set; }
    }

    public partial class PC_OrderProduct
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public string ProductName { get; set; }

        public string ProductCode { get; set; }

        public int SinglePrice { get; set; }

        public int Quantity { get; set; }

        public int TotalPrice { get; set; }

        public virtual PC_Order PC_Order { get; set; }
    }
}