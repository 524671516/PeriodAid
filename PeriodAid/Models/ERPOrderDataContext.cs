﻿namespace PeriodAid.Models
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
        public virtual DbSet<tags> tags { get; set; }
        public virtual DbSet<items> items { get; set; }
        public virtual DbSet<skus> skus { get; set; }
        public virtual DbSet<combine_items> combine_items { get; set; }
        public virtual DbSet<generic_data> generic_data { get; set; }
        public virtual DbSet<product_details> product_details { get; set; }
        public virtual DbSet<product_generic_data> product_generic_data { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<orders>()
                .HasMany(e => e.deliverys)
                .WithRequired(e => e.orders)
                .HasForeignKey(e => e.orderid)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<orders>()
                .HasMany(e => e.details)
                .WithRequired(e => e.orders)
                .HasForeignKey(e => e.orderid)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<orders>()
                .HasMany(e => e.invoices)
                .WithRequired(e => e.orders)
                .HasForeignKey(e => e.orderid)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<orders>()
                .HasMany(e => e.payments)
                .WithRequired(e => e.orders)
                .HasForeignKey(e => e.orderid)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<vips>()
                .HasMany(e => e.receive_infos)
                .WithRequired(e => e.vips)
                .HasForeignKey(e => e.vipid)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<items>()
                .HasMany(e => e.skus)
                .WithRequired(e => e.items)
                .HasForeignKey(e => e.itemsid)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<items>()
                .HasMany(e => e.combine_items)
                .WithRequired(e => e.items)
                .HasForeignKey(e => e.itemsid)
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

        public bool? refund { get; set; }

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
        
        public int type { get; set; }

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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tags> tags { get; set; }
    }
    public partial class receive_infos
    {
        public int id { get; set; }

        public int vipid { get; set; }

        [StringLength(64)]
        public string receiver { get; set; }

        [StringLength(32)]
        public string phone { get; set; }

        [StringLength(256)]
        public string address { get; set; }

        [StringLength(64)]
        public string area { get; set; }

        [StringLength(32)]
        public string tel { get; set; }

        public virtual vips vips { get; set; }
    }
    public partial class items
    {
        public long id { get; set; }

        public DateTime? create_date { get; set; }

        public DateTime? modify_date { get; set; }

        [StringLength(32)]
        public string code { get; set; }

        [StringLength(128)]
        public string name { get; set; }

        [StringLength(256)]
        public string note { get; set; }

        public decimal? weight { get; set; }

        public bool combine { get; set; }

        [StringLength(128)]
        public string simple_name { get; set; }

        [StringLength(32)]
        public string category_code { get; set; }

        [StringLength(64)]
        public string category_name { get; set; }

        [StringLength(32)]
        public string supplier_code { get; set; }

        [StringLength(32)]
        public string item_unit_code { get; set; }

        public decimal? package_point { get; set; }

        public decimal? sales_point { get; set; }

        public decimal? sales_price { get; set; }

        public decimal? purchase_price { get; set; }

        public decimal? agent_price { get; set; }

        public decimal? cost_price { get; set; }

        [StringLength(32)]
        public string stock_status_code { get; set; }

        [StringLength(256)]
        public string pic_url { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<skus> skus { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<combine_items> combine_items { get; set; }
    }
    public partial class skus
    {
        public long id { get; set; }

        public long itemsid { get; set; }

        [StringLength(32)]
        public string code { get; set; }

        [StringLength(128)]
        public string name { get; set; }

        [StringLength(256)]
        public string note { get; set; }
        
        public decimal? weight { get; set; }

        public decimal? package_point { get; set; }

        public decimal? sales_point { get; set; }

        public decimal? sales_price { get; set; }

        public decimal? purchase_price { get; set; }

        public decimal? agent_price { get; set; }

        public decimal? cost_price { get; set; }

        [StringLength(32)]
        public string stock_status_code { get; set; }

        [StringLength(32)]
        public string bar_code { get; set; }

        public virtual items items { get; set; }
    }
    public partial class combine_items
    {
        public long id { get; set; }

        public long itemsid { get; set; }

        public DateTime? create_date { get; set; }

        public DateTime? modify_date { get; set; }

        public decimal? qty { get; set; }

        public decimal? percent { get; set; }

        [StringLength(32)]
        public string item_code { get; set; }

        [StringLength(128)]
        public string item_name { get; set; }

        [StringLength(128)]
        public string simple_name { get; set; }

        [StringLength(32)]
        public string item_sku_code { get; set; }

        [StringLength(128)]
        public string item_sku_name { get; set; }

        public decimal? sales_price { get; set; }

        public virtual items items { get; set; }
    }

    public partial class generic_data
    {
        public int id { get; set; }

        public DateTime date { get; set; }

        [StringLength(64)]
        public string storename { get; set; }

        public decimal sales_amount { get; set; }

        public int? uv { get; set; }

        public int? pv { get; set; }

        public decimal? convertion { get; set; }

        public decimal? guest_avg_price { get; set; }

        public int? order_count { get; set; }

        public decimal? storage_amount { get; set; }

        public decimal? delivery_amount { get; set; }

        public decimal? advertisement_fee { get; set; }

        public decimal? roi { get; set; }

        public bool invoice { get; set; }

        public bool balance { get; set; }
    }

    public partial class product_details
    {
        public int id { get; set; }

        public DateTime date { get; set; }

        [StringLength(64)]
        public string storename { get; set; }
        
        [StringLength(32)]
        public string item_code { get; set; }

        [StringLength(64)]
        public string simple_name { get; set; }

        public int sales_count { get; set; }

        public decimal sales_amount { get; set; }
    }

    public partial class product_generic_data
    {
        public int id { get; set; }

        public DateTime date { get; set; }

        [StringLength(64)]
        public string storename { get; set; }

        [StringLength(32)]
        public string item_code { get; set; }

        [StringLength(64)]
        public string simple_name { get; set; }

        public int? uv { get; set; }

        public int? pv { get; set; }

        public int? order_count { get; set; }

        public decimal? order_amount { get; set; }

        public int? product_unit { get; set; }

        public decimal? convertion { get; set; }
    }

    public partial class tags
    {
        public int id { get; set; }

        [StringLength(16)]
        public string name { get; set; }

        public int totalcount { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<vips> vips { get; set; }
    }

    
}

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}