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
        public virtual DbSet<SP_Seller> SP_Seller { get; set; }
        public virtual DbSet<SP_Client> SP_Client { get; set; }
        public virtual DbSet<SP_Product> SP_Product { get; set; }
        public virtual DbSet<SP_OfferSheet> SP_OfferSheet { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
          
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

        [StringLength(16), RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        public string Client_Mobile { get; set; } // 手机号码

        public int Client_Type { get; set; }

        public string Client_Address { get; set; }
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
        
    }
    /// <summary>
    /// 订货单产品信息
    /// </summary>
    [Table ("SP_OfferSheet")]
    public partial class SP_OfferSheet
    {
        public int Id { get; set; }

        public decimal Quoted_Price { get; set; }

        public int Product_Id { get; set; }

        public int Order_Count { get; set; }

        public string Remark { get; set; }
    }
}