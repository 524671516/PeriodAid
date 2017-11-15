namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ShopStorageModel : DbContext
    {
        //您的上下文已配置为从您的应用程序的配置文件(App.config 或 Web.config)
        //使用“ShopStorageModel”连接字符串。默认情况下，此连接字符串针对您的 LocalDb 实例上的
        //“PeriodAid.Models.ShopStorageModel”数据库。
        // 
        //如果您想要针对其他数据库和/或数据库提供程序，请在应用程序配置文件中修改“ShopStorageModel”
        //连接字符串。
        public ShopStorageModel()
            : base("name=ShopStorageConnection")
        {
        }

        //为您要在模型中包含的每种实体类型都添加 DbSet。有关配置和使用 Code First  模型
        //的详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=390109。

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        public virtual DbSet<SS_Plattform> SS_Plattform { get; set; }
        public virtual DbSet<SS_Product> SS_Product { get; set; }
        public virtual DbSet<SS_Storage> SS_Storage { get; set; }
        public virtual DbSet<SS_SalesRecord> SS_SalesRecord { get; set; }
        public virtual DbSet<SS_UploadRecord> SS_UploadRecord { get; set; }
        public virtual DbSet<SS_Event> SS_Event { get; set; }
        public virtual DbSet<SS_TrafficPlattform> SS_TrafficPlattform { get; set; }
        public virtual DbSet<SS_TrafficSource> SS_TrafficSource { get; set; }
        public virtual DbSet<SS_TrafficData> SS_TrafficData { get; set; }
        public virtual DbSet<SS_UploadTraffic> SS_UploadTraffic { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SS_Plattform>().HasMany(m => m.SS_Product).WithRequired(m => m.SS_Plattform).HasForeignKey(m => m.Plattform_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_Product>().HasMany(m => m.SS_SalesRecord).WithRequired(m => m.SS_Product).HasForeignKey(m => m.Product_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_Storage>().HasMany(m => m.SS_SalesRecord).WithRequired(m => m.SS_Storage).HasForeignKey(m => m.Storage_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_Plattform>().HasMany(m => m.SS_Storage).WithRequired(m => m.SS_Plattform).HasForeignKey(m => m.Plattform_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_Plattform>().HasMany(m => m.SS_UploadRecord).WithRequired(m => m.SS_Plattform).HasForeignKey(m => m.Plattform_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_Product>().HasMany(m => m.SS_Event).WithRequired(m => m.SS_Product).HasForeignKey(m => m.Product_Id).WillCascadeOnDelete(true);
            // 新建
            modelBuilder.Entity<SS_TrafficPlattform>().HasMany(m => m.AttendTrafficSource).WithMany(e => e.AttendTrafficPlattform).Map(m => { m.MapLeftKey("TrafficPlattformId"); m.MapRightKey("TrafficSourceId"); m.ToTable("AttendPlattform_Source"); });
            modelBuilder.Entity<SS_TrafficSource>().HasMany(m => m.SS_TrafficData).WithRequired(m => m.SS_TrafficSource).HasForeignKey(m => m.TrafficSource_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_Product>().HasMany(m => m.SS_TrafficData).WithRequired(m => m.SS_Product).HasForeignKey(m => m.Product_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_Plattform>().HasMany(m => m.SS_TrafficPlattform).WithRequired(m => m.SS_Plattform).HasForeignKey(m => m.Plattform_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_TrafficPlattform>().HasMany(m => m.SS_TrafficData).WithRequired(m => m.SS_TrafficPlattform).HasForeignKey(m => m.TrafficPlattform_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_TrafficPlattform>().HasMany(m => m.SS_UploadTraffic).WithRequired(m => m.SS_TrafficPlattform).HasForeignKey(m => m.TrafficPlattform_Id).WillCascadeOnDelete(false);

        }
    }
    /// <summary>
    /// 平台表
    /// </summary>
    [Table("SS_Plattform")]
    public partial class SS_Plattform
    {
        public int Id { get; set; }
        [StringLength(16)]
        public string Plattform_Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_Product> SS_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_Storage> SS_Storage { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_UploadRecord> SS_UploadRecord { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_TrafficPlattform> SS_TrafficPlattform { get; set; }

    }
    /// <summary>
    /// 产品表
    /// </summary>
    [Table("SS_Product")]
    public partial class SS_Product
    {
        public int Id { get; set; }
        [StringLength(16)]
        public string System_Code { get; set; }
        [StringLength(16)]
        public string Item_Code { get; set; }
        [StringLength(16)]
        public string Item_Name { get; set; }

        // 产品类型
        public int Product_Type { get; set; }
        
        // 箱规
        public int Carton_Spec { get; set; }

        // 采购价
        public decimal Purchase_Price { get; set; }

        public DateTime Inventory_Date { get; set; }

        public int Plattform_Id { get; set; }

        public virtual SS_Plattform SS_Plattform { get; set; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_Storage> SS_Storage { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_SalesRecord> SS_SalesRecord { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_Event> SS_Event { get; set; }

        public int Bar_Code { get; internal set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_TrafficData> SS_TrafficData { get; set; }
    }
    /// <summary>
    /// 仓库表
    /// </summary>
    [Table("SS_Storage")]
    public partial class SS_Storage
    {
        public int Id { get; set; }
        [StringLength(16)]
        public string Storage_Name { get; set; }

        public int Plattform_Id { get; set; }

        public virtual SS_Plattform SS_Plattform { get; set; }

        // 常用仓库

        public int Storage_Type { get; set; }

        [StringLength(32)]
        public string Storage_Code { get; set; }

        [StringLength(16)]
        public string Sales_Header { get; set; }

        [StringLength(16)]
        public string Inventory_Header { get; set; }

        // 仓库顺序
        public int Index { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_SalesRecord> SS_SalesRecord { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_Product> SS_Product { get; set; }
    }
    [Table("SS_SalesRecord")]
    public partial class SS_SalesRecord
    {
        public int Id { get; set; }

        public DateTime SalesRecord_Date { get; set; }

        public int Sales_Count { get; set; }

        public int Storage_Count { get; set; }

        public int Product_Id { get; set; }

        public virtual SS_Product SS_Product { get; set; }

        public int Storage_Id { get; set; }

        public decimal Pay_Money { get; set; }

        public decimal SubAccount_Price { get; set; }

        public virtual SS_Storage SS_Storage { get; set; }
        
    }

    [Table("SS_UploadRecord")]
    public partial class SS_UploadRecord
    {
        public int Id { get; set; }

        public DateTime SalesRecord_Date { get; set; }

        public int Plattform_Id { get; set; }

        public virtual SS_Plattform SS_Plattform { get; set; }

        public DateTime Upload_Date { get; set; }
    }

    /// <summary>
    /// 活动表
    /// </summary>
    [Table("SS_Event")]
    public partial class SS_Event
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string EventName { get; set; }

        public int Product_Id { get; set; }

        public virtual SS_Product SS_Product { get; set; }

        public DateTime EventDate { get; set; }
    }
    
    // 新建
    [Table("SS_TrafficPlattform")]
    public partial class SS_TrafficPlattform
    {
        public int Id { get; set; }

        [StringLength(10)]
        public string TrafficPlattform_Name { get; set; }

        public int Plattform_Id { get; set; }

        public virtual SS_Plattform SS_Plattform { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_TrafficSource> AttendTrafficSource { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_TrafficSource> SS_TrafficSource { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_TrafficData> SS_TrafficData { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_UploadTraffic> SS_UploadTraffic { get; set; }

    }

    [Table("SS_TrafficSource")]
    public partial class SS_TrafficSource
    {
        public int Id { get; set; }

        [StringLength(16)]
        public string TrafficSource_Name { get; set; }

        public int Source_Type { get; set; }

        public int TrafficPlattform_Id { get; set; }

        public virtual SS_TrafficPlattform SS_TrafficPlattform { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_TrafficPlattform> AttendTrafficPlattform { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_TrafficData> SS_TrafficData { get; set; }
        
    }

    [Table("SS_TrafficData")]
    public partial class SS_TrafficData
    {
        public int Id { get; set; }

        public DateTime Update { get; set; }

        public int Product_Flow { get; set; }

        public int Product_Visitor { get; set; }

        public int Product_Customer { get; set; }

        public double Product_VisitTimes { get; set; }

        public int Order_Count { get; set; }

        public virtual SS_TrafficSource SS_TrafficSource { get; set; }

        public int TrafficSource_Id { get; set; }

        public virtual SS_Product SS_Product { get; set; }

        public int Product_Id { get; set; }

        public int TrafficPlattform_Id { get; set; }

        public virtual SS_TrafficPlattform SS_TrafficPlattform { get; set; }
    }

    public partial class TrafficData
    {
        public DateTime T_date { get; set; }
        public int Product_Id { get; set; }
        public string Product_Name { get; set; }
        public string Date_Source { get; set; }
        public int Product_Flow { get; set; }
        public int Product_Visitor { get; set; }
        public int Product_Customer { get; set; }
        public string Product_Times { get; set; }
        public int Order_Count { get; set; }
        public string Convert_Ratio { get; set; }
    }

    [Table("SS_UploadTraffic")]
    public partial class SS_UploadTraffic
    {
        public int Id { get; set; }

        public DateTime Traffic_Date { get; set; }

        public int TrafficPlattform_Id { get; set; }

        public virtual SS_TrafficPlattform SS_TrafficPlattform { get; set; }

        public DateTime Upload_Date { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}


}