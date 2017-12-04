﻿namespace PeriodAid.Models
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
        public virtual DbSet<SP_Seller> SP_Seller { get; set; }
        public virtual DbSet<SP_Customer> SP_Customer { get; set; }
        public virtual DbSet<SP_Product> SP_Product { get; set; }
        public virtual DbSet<SP_OfferSheet> SP_OfferSheet { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SP_Seller>().HasMany(m => m.SP_Customer).WithRequired(m => m.SP_Seller).HasForeignKey(m => m.Seller_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Customer>().HasMany(m => m.SP_Product).WithRequired(m => m.SP_Customer).HasForeignKey(m => m.Customer_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<SP_Customer>().HasMany(m => m.SP_OfferSheet).WithRequired(m => m.SP_Customer).HasForeignKey(m => m.Customer_Id).WillCascadeOnDelete(false);

        }
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

        public int Seller_Mobile { get; set; }

        public int Seller_Type { get; set; }

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

        public int Customer_Mobile { get; set; }

        public int Customer_Type { get; set; }

        public int Seller_Id { get; set; }

        public virtual SP_Seller SP_Seller { get; set; }

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