namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public class SalesProcessModel : DbContext
    {
        public SalesProcessModel()
            : base("name=SalesProcessConnection")
        {
        }
        public virtual DbSet<SP_Product> SP_Product { get; set; }
        public virtual DbSet<SP_ProductType> SP_ProductType { get; set; }
        public virtual DbSet<SP_Department> SP_Department { get; set; }
        public virtual DbSet<SP_Seller> SP_Seller { get; set; }
        public virtual DbSet<SP_Client> SP_Client { get; set; }
        public virtual DbSet<SP_Contact> SP_Contact { get; set; }
        public virtual DbSet<SP_SalesSystem> SP_SalesSystem { get; set; }
        public virtual DbSet<SP_QuotePrice> SP_QuotePrice { get; set; }
        public virtual DbSet<SP_Order> SP_Order { get; set; }
        public virtual DbSet<SP_OrderPrice> SP_OrderPrice { get; set; }
        public virtual DbSet<SP_FinanceInfo> SP_FinanceInfo { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SP_Seller>().HasMany(m => m.SP_Client).WithRequired(m => m.SP_Seller).HasForeignKey(m => m.Seller_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Client>().HasMany(m => m.SP_Contact).WithRequired(m => m.SP_Client).HasForeignKey(m => m.Client_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Product>().HasMany(m => m.SP_QuotePrice).WithRequired(m => m.SP_Product).HasForeignKey(m => m.Product_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Client>().HasMany(m => m.SP_FinanceInfo).WithRequired(m => m.SP_Client).HasForeignKey(m => m.Client_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_ProductType>().HasMany(m => m.SP_Product).WithRequired(m => m.SP_ProductType).HasForeignKey(m => m.ProductType_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Client>().HasMany(m => m.SP_SalesSystem).WithRequired(m => m.SP_Client).HasForeignKey(m => m.Client_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_SalesSystem>().HasMany(m => m.SP_QuotePrice).WithRequired(m => m.SP_SalesSystem).HasForeignKey(m => m.SalesSystem_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Contact>().HasMany(m => m.SP_Order).WithRequired(m => m.SP_Contact).HasForeignKey(m => m.Contact_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Order>().HasMany(m => m.SP_OrderPrice).WithRequired(m => m.SP_Order).HasForeignKey(m => m.Order_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Product>().HasMany(m => m.SP_OrderPrice).WithRequired(m => m.SP_Product).HasForeignKey(m => m.Product_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Department>().HasMany(m => m.SP_Seller).WithRequired(m => m.SP_Department).HasForeignKey(m => m.Department_Id).WillCascadeOnDelete(false);
        }
    }

    /// <summary>
    /// 产品信息
    /// </summary>
    [Table("SP_Product")]
    public partial class SP_Product
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string Item_Code { get; set; }

        public string System_Code { get; set; }

        [StringLength(16)]
        public string Item_Name { get; set; }

        public string Brand_Name { get; set; }

        public string Item_ShortName { get; set; }

        public string Supplier_Name { get; set; }

        public string Bar_Code { get; set; }

        public int Product_Weight { get; set; }

        public int Carton_Spec { get; set; }
        
        public decimal Purchase_Price { get; set; }
        
        public decimal Supply_Price { get; set; }

        public int Product_Status { get; set; }

        public int ProductType_Id { get; set; }

        [StringLength(256)]
        public string Product_Img { get; set; }

        public virtual SP_ProductType SP_ProductType { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_QuotePrice> SP_QuotePrice { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_OrderPrice> SP_OrderPrice { get; set; }

    }
    /// <summary>
    /// 商品类别
    /// </summary>
    [Table("SP_ProductType")]
    public partial class SP_ProductType
    {
        public int Id { get; set; }

        [StringLength(16)]
        public string Type_Name { get; set; }

        public int ProductType_Status { get; set; }

        public int Priority { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Product> SP_Product { get; set; }
    }
    /// <summary>
    /// 部门
    /// </summary>
    [Table("SP_Department")]
    public partial class SP_Department
    {
        public int Id { get; set; }

        public string Department_Name { get; set; }

        public int Status { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Seller> SP_Seller { get; set; }
    }
    /// <summary>
    /// 业务员
    /// </summary>
    [Table("SP_Seller")]
    public partial class SP_Seller
    {
        public int Id { get; set; }

        [StringLength(16)]
        public string Seller_Name { get; set; }

        [Required, StringLength(32)]
        public string User_Name { get; set; }

        [StringLength(16), RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        public string Seller_Mobile { get; set; } // 手机号码

        public int Seller_Type { get; set; }

        public int Seller_Status { get; set; }

        public string Manager_Id { get; set; }

        public int Department_Id { get; set; }

        public virtual SP_Department SP_Department { get; set; }
        
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Client> SP_Client { get; set; }
        
    }
    /// <summary>
    /// 经销商信息
    /// </summary>
    [Table("SP_Client")]
    public partial class SP_Client
    {
        public int Id { get; set; }

        [StringLength(16)]
        public string Client_Name { get; set; }
        
        public string Client_Area { get; set; }

        public string Client_Address { get; set; }

        public int Client_Type { get; set; }

        public int Client_Status { get; set; }

        public int Seller_Id { get; set; }

        public virtual SP_Seller SP_Seller { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Contact> SP_Contact { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_FinanceInfo> SP_FinanceInfo { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_SalesSystem> SP_SalesSystem { get; set; }

    }
    /// <summary>
    /// 销售系统
    /// </summary>
    [Table("SP_SalesSystem")]
    public partial class SP_SalesSystem
    {
        public int Id { get; set; }
         
        public string System_Name { get; set; }
        
        public string System_Address { get; set; }

        public string System_Phone { get; set; }

        public int System_Status { get; set; }

        public int Client_Id { get; set; }

        public virtual SP_Client SP_Client { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_QuotePrice> SP_QuotePrice { get; set; }
    }
    /// <summary>
    /// 对接人人信息
    /// </summary>
    [Table("SP_Contact")]
    public partial class SP_Contact
    {
        public int Id { get; set; }

        public string Contact_Name { get; set; }

        [StringLength(16), RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        public string Contact_Mobile { get; set; }

        public string Contact_Job { get; set; }

        public int Contact_Status { get; set; }

        public int Client_Id { get; set; }

        public virtual SP_Client SP_Client { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Order> SP_Order { get; set; }
    }
    
    /// <summary>
    /// 报价商品
    /// </summary>
    [Table("SP_QuotePrice")]
    public partial class SP_QuotePrice
    {
        public int Id { get; set; }

        public decimal Quote_Price { get; set; }

        public DateTime Quoted_Date { get; set; }

        public int Quoted_Status { get; set; }

        public int Product_Id { get; set; }

        public virtual SP_Product SP_Product { get; set; }

        public int SalesSystem_Id { get; set; }

        public virtual SP_SalesSystem SP_SalesSystem { get; set; }

    }
    /// <summary>
    /// 订货单信息
    /// </summary>
    [Table("SP_Order")]
    public partial class SP_Order
    {
        public int Id { get; set; }

        public string Order_Number { get; set; }

        public string Order_Address { get; set; }

        public DateTime Order_Date { get; set; }

        public int Order_Status { get; set; }

        public int Order_Type { get; set; }

        public int Contact_Id { get; set; }

        public string Order_Remark { get; set; }

        public string Signed_Number { get; set; }

        public decimal Cancellation_Fee { get; set; }

        public virtual SP_Contact SP_Contact { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_OrderPrice> SP_OrderPrice { get; set; }
    }
    /// <summary>
    /// 订货详情
    /// </summary>
    [Table("SP_OrderPrice")]
    public partial class SP_OrderPrice
    {
        public int Id { get; set; }

        public decimal Order_Price { get; set; }

        public int Order_Count { get; set; }

        public int OrderPrice_Status { get; set; }

        public int Order_Id { get; set; }

        public string OrderPrice_Remark { get; set; }

        public virtual SP_Order SP_Order { get; set; }

        public int Product_Id { get; set; }

        public virtual SP_Product SP_Product { get; set; }
    }
    /// <summary>
    /// 财务信息
    /// </summary>
    [Table("SP_FinanceInfo")]
    public partial class SP_FinanceInfo
    {
        public int Id { get; set; }

        public int Client_Id { get; set; }

        public virtual SP_Client SP_Client { get; set; }
    }
    
    public class OrderPriceSum
    {
        public int SumCount { get; set; }

        public decimal SumPrice { get; set; }
    }
    
    public static class SellerType
    {
        /// <summary>
        /// 业务员
        /// </summary>
        public static int SELLER = 0;
        /// <summary>
        /// 产品部
        /// </summary>
        public static int PRODUCTDEPARTMENT = 1;
        /// <summary>
        ///  财务部
        /// </summary>
        public static int FINANCIALDEPARTMENT = 2;
        /// <summary>
        ///  业务主管
        /// </summary>
        public static int SELLERADMIN = 3;
        /// <summary>
        ///  管理员
        /// </summary>
        public static int ADMINISTARTOR = 4;

    }
    

}