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
        public virtual DbSet<SP_Plattform> SP_Plattform { get; set; }
        public virtual DbSet<SP_Product> SP_Product { get; set; }
        public virtual DbSet<SP_Seller> SP_Seller { get; set; }
        public virtual DbSet<SP_Client> SP_Client { get; set; }
        public virtual DbSet<SP_Contact> SP_Contact { get; set; }
        public virtual DbSet<SP_Quoted> SP_Quoted { get; set; }
        public virtual DbSet<SP_Order> SP_Order { get; set; }
        public virtual DbSet<SP_FinanceInfo> SP_FinanceInfo { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SP_Plattform>().HasMany(m => m.SP_Client).WithRequired(m => m.SP_Plattform).HasForeignKey(m => m.Plattform_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Seller>().HasMany(m => m.SP_Client).WithRequired(m => m.SP_Seller).HasForeignKey(m => m.Seller_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Client>().HasMany(m => m.SP_Contact).WithRequired(m => m.SP_Client).HasForeignKey(m => m.Client_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Product>().HasMany(m => m.SP_Quoted).WithRequired(m => m.SP_Product).HasForeignKey(m => m.Product_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Client>().HasMany(m => m.SP_Quoted).WithRequired(m => m.SP_Client).HasForeignKey(m => m.Client_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Client>().HasMany(m => m.SP_FinanceInfo).WithRequired(m => m.SP_Client).HasForeignKey(m => m.Client_Id).WillCascadeOnDelete(false);
        }
    }
    /// <summary>
    /// 平台
    /// </summary>
    [Table("SP_Plattform")]
    public partial class SP_Plattform
    {
        public int Id { get; set; }

        [StringLength(10)]
        public string Plattform_Name { get; set; }

        public int Plattform_Type { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Client> SP_Client { get; set; }
    }
    /// <summary>
    /// 产品信息
    /// </summary>
    [Table("SP_Product")]
    public partial class SP_Product
    {
        public int Id { get; set; }

        public string Item_Code { get; set; }

        [StringLength(16)]
        public string Item_Name { get; set; }

        public string System_Code { get; set; }

        public int Carton_Spec { get; set; }

        public decimal Purchase_Price { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Quoted> SP_Quoted { get; set; }

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

        [StringLength(16), RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        public string Seller_Mobile { get; set; } // 手机号码

        public int Seller_Type { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Client> SP_Client { get; set; }
    }
    /// <summary>
    /// 客户信息
    /// </summary>
    [Table("SP_Client")]
    public partial class SP_Client
    {
        public int Id { get; set; }

        [StringLength(16)]
        public string Client_Name { get; set; }
        
        public int Client_Type { get; set; }
        
        public int Plattform_Id { get; set; }

        public virtual SP_Plattform SP_Plattform { get; set; }

        public int Seller_Id { get; set; }

        public virtual SP_Seller SP_Seller { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Contact> SP_Contact { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Quoted> SP_Quoted { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_FinanceInfo> SP_FinanceInfo { get; set; }
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

        public string Contact_Address { get; set; }

        public int Client_Id { get; set; }

        public virtual SP_Client SP_Client { get; set; }
    }
    /// <summary>
    /// 报价信息
    /// </summary>
    [Table ("SP_Quoted")]
    public partial class SP_Quoted
    {
        public int Id { get; set; }

        public decimal Quoted_Price { get; set; }

        public DateTime Quoted_Date { get; set; }

        public string Remark { get; set; }

        public int Product_Id { get; set; }

        public virtual SP_Product SP_Product { get; set; }

        public int Client_Id { get; set; }

        public virtual SP_Client SP_Client { get; set; }
    }
    /// <summary>
    /// 订货单信息
    /// </summary>
    [Table("SP_Order")]
    public partial class SP_Order
    {
        public int Id { get; set; }

        public string Order_Number { get; set; }

        public DateTime Order_Date { get; set; }
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
}