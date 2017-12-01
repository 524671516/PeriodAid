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
        public virtual DbSet<SF_Seller> SF_Seller { get; set; }
        public virtual DbSet<SF_Customer> SF_Customer { get; set; }
        public virtual DbSet<SF_Product> SF_Product { get; set; }
        public virtual DbSet<SF_OfferSheet> SF_OfferSheet { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SF_Seller>().HasMany(m => m.SF_Customer).WithRequired(m => m.SF_Seller).HasForeignKey(m => m.Seller_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SF_Customer>().HasMany(m => m.SF_Product).WithRequired(m => m.SF_Customer).HasForeignKey(m => m.Customer_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SF_Customer>().HasMany(m => m.SF_OfferSheet).WithRequired(m => m.SF_Customer).HasForeignKey(m => m.Customer_Id).WillCascadeOnDelete(false);

        }
    }
    /// <summary>
    /// 业务员
    /// </summary>
    [Table("SF_Seller")]
    public partial class SF_Seller
    {
        public int Id { get; set; }

        [StringLength(16)]
        public string Seller_Name { get; set; }

        public int Seller_Mobile { get; set; }

        public int Seller_Type { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SF_Customer> SF_Customer { get; set; }
    }
    /// <summary>
    /// 客户信息
    /// </summary>
    [Table("SF_Customer")]
    public partial class SF_Customer
    {
        public int Id { get; set; }

        [StringLength(16)]
        public string Customer_Name { get; set; }

        public int Customer_Mobile { get; set; }

        public int Customer_Type { get; set; }

        public int Seller_Id { get; set; }

        public virtual SF_Seller SF_Seller { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SF_Product> SF_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SF_OfferSheet> SF_OfferSheet { get; set; }
    }
    /// <summary>
    /// 产品信息
    /// </summary>
    [Table("SF_Product")]
    public partial class SF_Product
    {
        public int Id { get; set; }

        public int Item_Code { get; set; }

        [StringLength(16)]
        public string Item_Name { get; set; }

        public int System_Code { get; set; }

        public int Customer_Id { get; set; }

        public virtual SF_Customer SF_Customer { get; set; }

        public int Carton_Spec { get; set; }

        public int Purchase_Price { get; set; }
    }
    /// <summary>
    /// 订货单产品信息
    /// </summary>
    [Table ("SF_OfferSheet")]
    public partial class SF_OfferSheet
    {
        public int Id { get; set; }

        public int Quoted_Price { get; set; }

        public int Product_Id { get; set; }

        public int Customer_Id { get; set; }

        public virtual SF_Customer SF_Customer { get; set; }

        public int Order_Count { get; set; }

        public string Remark { get; set; }
    }
}