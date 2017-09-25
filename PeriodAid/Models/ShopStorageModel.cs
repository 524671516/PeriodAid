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

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SS_Plattform>().HasMany(m => m.SS_Product).WithRequired(m => m.SS_Plattform).HasForeignKey(m => m.Plattform_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_Product>().HasMany(m => m.SS_SalesRecord).WithRequired(m => m.SS_Product).HasForeignKey(m => m.Product_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_Storage>().HasMany(m => m.SS_SalesRecord).WithRequired(m => m.SS_Storage).HasForeignKey(m => m.Storage_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SS_Plattform>().HasMany(m => m.SS_Storage).WithRequired(m => m.SS_Plattform).HasForeignKey(m => m.Plattform_Id).WillCascadeOnDelete(false);
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

        public DateTime Inventory_Date { get; set; }

        public int Plattform_Id { get; set; }

        public virtual SS_Plattform SS_Plattform { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_Storage> SS_Storage { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_SalesRecord> SS_SalesRecord { get; set; }
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

        [StringLength(16)]
        public string Sales_Header { get; set; }
        [StringLength(16)]
        public string Inventory_Header { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_SalesRecord> SS_SalesRecord { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SS_Product> SS_Product { get; set; }
    }
    
    public partial class SS_SalesRecord
    {
        public int Id { get; set; }

        public DateTime SalesRecord_Date { get; set; }

        public int Sales_Count { get; set; }

        public int Storage_Count { get; set; }

        public int Product_Id { get; set; }

        public virtual SS_Product SS_Product { get; set; }

        public int Storage_Id { get; set; }

        public virtual SS_Storage SS_Storage { get; set; }
    }
    // ViewModel
    public class Product_SummaryViewModel
    {
        public SS_Product Product { get; set; }

        public int Sales_Sum { get; set; }

        public int Inventory_Sum { get; set; }
    }
    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}