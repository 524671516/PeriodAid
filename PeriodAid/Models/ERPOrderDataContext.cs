namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    public class ERPOrderDataContext : DbContext
    {
        //您的上下文已配置为从您的应用程序的配置文件(App.config 或 Web.config)
        //使用“ERPOrderDataContext”连接字符串。默认情况下，此连接字符串针对您的 LocalDb 实例上的
        //“PeriodAid.Models.ERPOrderDataContext”数据库。
        // 
        //如果您想要针对其他数据库和/或数据库提供程序，请在应用程序配置文件中修改“ERPOrderDataContext”
        //连接字符串。
        public ERPOrderDataContext()
            : base("name=ERPOrderDataContext")
        {
        }

        //为您要在模型中包含的每种实体类型都添加 DbSet。有关配置和使用 Code First  模型
        //的详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=390109。

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        public virtual DbSet<deliverys> deliverys { get; set; }
        public virtual DbSet<details> details { get; set; }
        public virtual DbSet<invoices> invoices { get; set; }
        public virtual DbSet<orders> orders { get; set; }
        public virtual DbSet<payments> payments { get; set; }
        public virtual DbSet<taskstatus> taskstatus { get; set; }
        public virtual DbSet<vips> vips { get; set; }
        public virtual DbSet<receive_infos> receive_infos { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<orders>()
                .HasMany(e => e.deliverys)
                .WithRequired(e => e.orders)
                .HasForeignKey(e => e.orderid);

            modelBuilder.Entity<orders>()
                .HasMany(e => e.details)
                .WithRequired(e => e.orders)
                .HasForeignKey(e => e.orderid);

            modelBuilder.Entity<orders>()
                .HasMany(e => e.invoices)
                .WithRequired(e => e.orders)
                .HasForeignKey(e => e.orderid);

            modelBuilder.Entity<orders>()
                .HasMany(e => e.payments)
                .WithRequired(e => e.orders)
                .HasForeignKey(e => e.orderid);

            modelBuilder.Entity<vips>()
                .HasMany(e => e.receive_infos)
                .WithRequired(e => e.vips)
                .HasForeignKey(e => e.vipid)
                .WillCascadeOnDelete(true);
        }

    }
    public partial class orders
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public orders()
        {
            deliverys = new HashSet<deliverys>();
            details = new HashSet<details>();
            invoices = new HashSet<invoices>();
            payments = new HashSet<payments>();
        }

        public int id { get; set; }

        [StringLength(64)]
        public string code { get; set; }

        public decimal? qty { get; set; }

        public decimal? amount { get; set; }

        public decimal? payment { get; set; }

        public bool? approve { get; set; }

        public bool? cod { get; set; }

        [StringLength(128)]
        public string platform_code { get; set; }

        public DateTime? createtime { get; set; }

        public DateTime? dealtime { get; set; }

        public DateTime? paytime { get; set; }

        [StringLength(64)]
        public string shop_name { get; set; }

        [StringLength(64)]
        public string shop_code { get; set; }

        [StringLength(64)]
        public string warehouse_name { get; set; }

        [StringLength(64)]
        public string warehouse_code { get; set; }

        [StringLength(64)]
        public string express_name { get; set; }

        [StringLength(64)]
        public string express_code { get; set; }

        [StringLength(128)]
        public string vip_name { get; set; }

        [StringLength(128)]
        public string vip_code { get; set; }

        [StringLength(128)]
        public string receiver_name { get; set; }

        [StringLength(64)]
        public string receiver_phone { get; set; }

        [StringLength(64)]
        public string receiver_mobile { get; set; }

        [StringLength(64)]
        public string receiver_zip { get; set; }

        [StringLength(256)]
        public string receiver_address { get; set; }

        [StringLength(128)]
        public string receiver_area { get; set; }

        public string buyer_memo { get; set; }

        public string seller_memo { get; set; }

        public string seller_memo_late { get; set; }

        public decimal? post_fee { get; set; }

        public decimal? cod_fee { get; set; }

        public decimal? discount_fee { get; set; }

        public decimal? post_cost { get; set; }

        public decimal? weight_origin { get; set; }

        public decimal? payment_amount { get; set; }

        public int? delivery_state { get; set; }

        [StringLength(64)]
        public string order_type_name { get; set; }

        [StringLength(64)]
        public string platform_flag { get; set; }

        [StringLength(64)]
        public string business_man { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<deliverys> deliverys { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<details> details { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<invoices> invoices { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<payments> payments { get; set; }
    }
    public partial class payments
    {
        public int id { get; set; }

        public int orderid { get; set; }

        public decimal? payment { get; set; }

        [StringLength(64)]
        public string pay_type_name { get; set; }

        public DateTime? paytime { get; set; }

        public virtual orders orders { get; set; }
    }
    public partial class details
    {
        public int id { get; set; }

        public int orderid { get; set; }

        [StringLength(128)]
        public string oid { get; set; }

        public decimal? qty { get; set; }

        public decimal? price { get; set; }

        public decimal? amount { get; set; }

        public decimal? refund { get; set; }

        [StringLength(64)]
        public string item_code { get; set; }

        [StringLength(64)]
        public string item_name { get; set; }

        [StringLength(64)]
        public string item_simple_name { get; set; }

        [StringLength(64)]
        public string sku_name { get; set; }

        [StringLength(64)]
        public string sku_code { get; set; }

        public decimal? post_fee { get; set; }

        public decimal? discount_fee { get; set; }

        public decimal? amount_after { get; set; }

        [StringLength(128)]
        public string platform_item_name { get; set; }

        [StringLength(64)]
        public string platform_sku_name { get; set; }

        [StringLength(64)]
        public string note { get; set; }

        public virtual orders orders { get; set; }
    }
    public partial class invoices
    {
        public int id { get; set; }

        public int orderid { get; set; }

        [StringLength(64)]
        public string invoice_type_name { get; set; }

        [StringLength(64)]
        public string invoice_title { get; set; }

        [StringLength(256)]
        public string invoice_content { get; set; }

        public decimal? invoice_amount { get; set; }

        public virtual orders orders { get; set; }
    }
    public partial class deliverys
    {
        public int id { get; set; }

        public int orderid { get; set; }

        public bool? delivery { get; set; }

        [StringLength(64)]
        public string code { get; set; }

        public bool? printExpress { get; set; }

        public bool? printDeliveryList { get; set; }

        public bool? scan { get; set; }

        public bool? weight { get; set; }

        [StringLength(64)]
        public string warehouse_name { get; set; }

        [StringLength(64)]
        public string warehouse_code { get; set; }

        [StringLength(64)]
        public string express_name { get; set; }

        [StringLength(64)]
        public string express_code { get; set; }

        [StringLength(64)]
        public string mail_no { get; set; }

        public virtual orders orders { get; set; }
    }
    public partial class taskstatus
    {
        public int id { get; set; }
        [StringLength(64)]
        public string name { get; set; }

        public int status { get; set; }

        public DateTime create_time { get; set; }

        public DateTime? finish_time { get; set; }

        public int totalcount { get; set; }

        public int currentcount { get; set; }

        public string message { get; set; }
    }
    public partial class vips
    {
        public int id { get; set; }

        public DateTime created { get; set; }

        [StringLength(64)]
        public string code { get; set; }

        [StringLength(64)]
        public string name { get; set; }

        public decimal? sex { get; set; }

        [StringLength(32)]
        public string qq { get; set; }

        [StringLength(32)]
        public string ww { get; set; }

        [StringLength(64)]
        public string email { get; set; }

        public DateTime? birthday { get; set; }

        public bool agent { get; set; }

        [StringLength(32)]
        public string shop_name { get; set; }

        [StringLength(32)]
        public string level { get; set; }

        [StringLength(32)]
        public string source { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<receive_infos> receive_infos { get; set; }
    }
    public partial class receive_infos
    {
        public int id { get; set; }

        public int vipid { get; set; }

        [StringLength(64)]
        public string receiver { get; set; }

        [StringLength(32)]
        public string phone { get; set; }

        [StringLength(128)]
        public string address { get; set; }

        [StringLength(64)]
        public string area { get; set; }

        [StringLength(32)]
        public string tel { get; set; }

        public virtual vips vips { get; set; }
    }
}

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}