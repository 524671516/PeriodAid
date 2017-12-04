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
        public virtual DbSet<SP_TrafficPlattform> SP_TrafficPlattform { get; set; }
        public virtual DbSet<SP_Seller> SP_Seller { get; set; }
        public virtual DbSet<SP_Customer> SP_Customer { get; set; }
        public virtual DbSet<SP_Product> SP_Product { get; set; }
        public virtual DbSet<SP_OfferSheet> SP_OfferSheet { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SP_Plattform>().HasMany(m => m.SP_Product).WithRequired(m => m.SP_Plattform).HasForeignKey(m => m.Plattform_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Plattform>().HasMany(m => m.SP_TrafficPlattform).WithRequired(m => m.SP_Plattform).HasForeignKey(m => m.Plattform_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Plattform>().HasMany(m => m.SP_Seller).WithRequired(m => m.SP_Plattform).HasForeignKey(m => m.Plattform_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_TrafficPlattform>().HasMany(m => m.SP_Customer).WithRequired(m => m.SP_TrafficPlattform).HasForeignKey(m => m.TrafficPlattform_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Seller>().HasMany(m => m.SP_Customer).WithRequired(m => m.SP_Seller).HasForeignKey(m => m.Seller_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Customer>().HasMany(m => m.SP_Product).WithRequired(m => m.SP_Customer).HasForeignKey(m => m.Customer_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Customer>().HasMany(m => m.SP_OfferSheet).WithRequired(m => m.SP_Customer).HasForeignKey(m => m.Customer_Id).WillCascadeOnDelete(false);

        }
    }
    /// <summary>
    /// 平台
    /// </summary>
    [Table("SP_Plattform")]
    public partial class SP_Plattform
    {
        public int Id { get; set; }

        [StringLength(16)]
        public string Plattform_Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Product> SP_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_TrafficPlattform> SP_TrafficPlattform { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Seller> SP_Seller { get; set; }
    }
    /// <summary>
    /// 小平台
    /// </summary>
    [Table("SP_TrafficPlattform")]
    public partial class SP_TrafficPlattform
    {
        public int Id { get; set; }

        [StringLength(10)]
        public string TrafficPlattform_Name { get; set; }

        public int Plattform_Id { get; set; }

        public virtual SP_Plattform SP_Plattform { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Customer> SP_Customer { get; set; }
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
        
        public int Plattform_Id { get; set; }

        public virtual SP_Plattform SP_Plattform { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Customer> SP_Customer { get; set; }
    }
    /// <summary>
    /// 客户信息
    /// </summary>
    [Table("SP_Customer")]
    public partial class SP_Customer
    {
        public int Id { get; set; }

        [StringLength(16)]
        public string Customer_Name { get; set; }

        [StringLength(16), RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        public string Customer_Mobile { get; set; } // 手机号码

        public int Customer_Type { get; set; }

        public string Cusromer_Address { get; set; }

        public int Seller_Id { get; set; }

        public virtual SP_Seller SP_Seller { get; set; }

        public int TrafficPlattform_Id { get; set; }

        public virtual SP_TrafficPlattform SP_TrafficPlattform { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_Product> SP_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SP_OfferSheet> SP_OfferSheet { get; set; }
    }
    /// <summary>
    /// 产品信息
    /// </summary>
    [Table("SP_Product")]
    public partial class SP_Product
    {
        public int Id { get; set; }

        public int Item_Code { get; set; }

        [StringLength(16)]
        public string Item_Name { get; set; }

        public int System_Code { get; set; }

        public int Customer_Id { get; set; }

        public virtual SP_Customer SP_Customer { get; set; }

        public int Carton_Spec { get; set; }

        public int Purchase_Price { get; set; }

        public int Plattform_Id { get; set; }

        public virtual SP_Plattform SP_Plattform { get; set; }
    }
    /// <summary>
    /// 订货单产品信息
    /// </summary>
    [Table ("SP_OfferSheet")]
    public partial class SP_OfferSheet
    {
        public int Id { get; set; }

        public int Quoted_Price { get; set; }

        public int Product_Id { get; set; }

        public int Customer_Id { get; set; }

        public virtual SP_Customer SP_Customer { get; set; }

        public int Order_Count { get; set; }

        public string Remark { get; set; }
    }
}