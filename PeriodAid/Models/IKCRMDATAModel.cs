using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public class IKCRMDATAModel : DbContext
    {
        public IKCRMDATAModel()
            : base("name=IKCRMDATAConnection")
        {
        }
        //public virtual DbSet<CRM_Product> CRM_Product { get; set; }
        public virtual DbSet<CRM_Contract> CRM_Contract { get; set; }
        public virtual DbSet<CRM_ContractDetail> CRM_ContractDetail { get; set; }
        public virtual DbSet<CRM_User_Token> CRM_User_Token { get; set; }
        public virtual DbSet<CRM_ExceptionLogs> CRM_ExceptionLogs { get; set; }
        public virtual DbSet<CRM_Customer> CRM_Customer { get; set; }
        public virtual DbSet<CRM_Contact> CRM_Contact { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CRM_Customer>().HasMany(m => m.CRM_Contract).WithRequired(m => m.CRM_Customer).HasForeignKey(m => m.customer_id).WillCascadeOnDelete(false);
            modelBuilder.Entity<CRM_Customer>().HasMany(m => m.CRM_Contact).WithRequired(m => m.CRM_Customer).HasForeignKey(m => m.customer_id).WillCascadeOnDelete(false);
            modelBuilder.Entity<CRM_Contract>().HasMany(m => m.CRM_ContractDetail).WithRequired(m => m.CRM_Contract).HasForeignKey(m => m.contract_id).WillCascadeOnDelete(false);

        }
    }
    //[Table("CRM_Product")]
    //public partial class CRM_Product
    //{
    //    public int Id { get; set; }
        
    //    [StringLength(64)]
    //    public string system_Code { get; set; }

    //    [StringLength(64)]
    //    public string item_Name { get; set; }
        
    //    public int product_id { get; set; }
    //    // 规格
    //    public string product_standard { get; set; }
    //    // 如箱数
    //    public string enter_box { get; set; }
    //    // 单价
    //    public decimal unit_price { get; set; }
    //    // 毛利
    //    public string gross_profit { get; set; }
    //    // 零售价
    //    public decimal retail_price { get; set; }
    //    // 单盒条形码
    //    public string product_barcode { get; set; }
    //    // QS/SC号
    //    public string QS_SC_number { get; set; }
    //    // 保质期
    //    public string expiration_date { get; set; }
    //    // 单包装尺寸
    //    public string product_size { get; set; }
    //    // 外箱条码
    //    public string outside_barcode { get; set; }
    //    // 外箱包装尺寸
    //    public string outside_size { get; set; }
    //    // 净重
    //    public decimal net_weight { get; set; }
    //    // 毛重
    //    public decimal gross_weight { get; set; }
    //}

    //public partial class CRM_Product_ReturnData
    //{
    //    public string code { get; set; }

    //    public CRM_ProductData data { get; set; }
    //}

    //public partial class CRM_ProductData
    //{
    //    public int per_page { get; set; }

    //    public int total_count { get; set; }

    //    public List<CRM_ProductCategories> product_categories { get; set; }

    //    public List<CRM_ProductList> products { get; set; }
    //}

    //public partial class CRM_ProductCategories
    //{
    //    public string name { get; set; }
    //}

    //public partial class CRM_ProductList
    //{
    //    public int id { get; set; }

    //    public string product_no { get; set; }

    //    public string name { get; set; }
    //    // 规格
    //    public string text_asset_ccc3d6 { get; set; }
    //    // 如箱数
    //    public string text_asset_ed9843 { get; set; }
    //    // 单价
    //    public decimal unit_cost { get; set; }
    //    // 毛利
    //    public string gross_margin { get; set; }
    //    // 零售价
    //    public decimal standard_unit_price { get; set; }
    //    // 单盒条形码
    //    public string text_asset_a632bd { get; set; }
    //    // QS/SC号
    //    public string text_asset_3a7a67 { get; set; }
    //    // 保质期
    //    public string text_asset_1c439d { get; set; }
    //    // 单包装尺寸
    //    public string text_asset_2e3eb8 { get; set; }
    //    // 外箱条形码
    //    public string text_asset_d00266 { get; set; }
    //    // 外箱包装尺寸
    //    public string text_asset_ce85c8 { get; set; }
    //    // 净重
    //    public string numeric_asset_db2cc5 { get; set; }
    //    // 毛重
    //    public string numeric_asset_4958a4 { get; set; }
    //    // 分类
    //    public CRM_ProductCategory product_category { get; set; }
        
    //}

    //public partial class CRM_ProductCategory
    //{
    //    public string name { get; set; }
    //}
    
    [Table("CRM_User_Token")]
    public partial class CRM_User_Token
    {
        public int Id { get; set; }

        [StringLength(256)]
        public string user_token { get; set; }

        public DateTime download_at { get; set; }
    }
    
    public partial class CRM_Contract_ReturnData
    {
        public string code { get; set; }

        public CRM_Contract_Data data { get; set; }
    }
    
    public partial class CRM_Contract_Data
    {
        public List<CRM_Contract> contracts { get; set; }
    }

    [Table("CRM_Contract")]
    public class CRM_Contract
    {
        public int id { get; set; }
        
        public int contract_id { get; set; }

        [StringLength(64)]
        public string platform_code { get; set; }
        
        public int user_id { get; set; }

        [StringLength(64)]
        public string user_name { get; set; }
        
        public int customer_id { get; set; }

        public virtual CRM_Customer CRM_Customer { get; set; }

        [StringLength(128)]
        public string title { get; set; }

        public decimal total_amount { get; set; }

        public string status { get; set; }

        public DateTime? updated_at { get; set; }

        [StringLength(64)]
        public string express_name { get; set; }

        [StringLength(64)]
        public string express_code { get; set; }

        public string express_state { get; set; }

        [StringLength(128)]
        public string express_number { get; set; }

        [StringLength(128)]
        public string shop_code { get; set; }

        [StringLength(128)]
        public string warehouse_code { get; set; }

        [StringLength(128)]
        public string vip_code { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_ContractDetail> CRM_ContractDetail { get; set; }
        // 收货人
        public string consignee_name { get; set; }
        // 收货地址
        public string consignee_address { get; set; }
        // 电话
        public string consignee_tel { get; set; }
        //地址检测(0通过，-1出错)
        public int address_status { get; set; }
    }
    
    [Table("CRM_ExceptionLogs")]
    public partial class CRM_ExceptionLogs
    {
        public int Id { get; set; }

        public string type{ get; set; }

        public string exception { get; set; }

        public DateTime exception_at { get; set; }
    }

    public class CRM_ContractDetail_ReturnData
    {
        public string code { get; set; }

        public CRM_ContractDetail_Data data { get; set; }
    }

    public partial class CRM_ContractDetail_Data
    {
        public List<CRM_ContractDetail_CustomerProductList> product_assets_for_new_record { get; set; }
        
        public int id { get; set; }

        public string text_asset_1e30f4 { get; set; }
        // 收件人
        public string text_asset_73f972 { get; set; }
        // 收货地址
        public string text_asset_eb802b { get; set; }
        // 电话
        public string text_asset_da4211 { get; set; }
    }
    
    public partial class CRM_ContractDetail_CustomerProductList
    {
        public int product_id { get; set; }

        public string product_no { get; set; }

        public int quantity { get; set; }

        public decimal recommended_unit_price { get; set; }

        public string name { get; set; }
    }
    

    [Table("CRM_ContractDetail")]
    public partial class CRM_ContractDetail
    {
        public int Id { get; set; }
        
        public int contract_id { get; set; }

        public virtual CRM_Contract CRM_Contract { get; set; }

        public int product_id { get; set; }

        public string product_name { get; set; }

        public string product_code { get; set; }

        public int quantity { get; set; }

        public decimal unit_price { get; set; }
        
    }

    [Table("CRM_Customer")]
    public partial class CRM_Customer
    {
        public int Id { get; set; }
        
        public int customer_id { get; set; }

        [StringLength(64)]
        public string customer_name { get; set; }

        [StringLength(256)]
        public string customer_address { get; set; }

        [StringLength(64)]
        public string customer_tel { get; set; }

        public string province { get; set; }

        public string city { get; set; }

        public string district { get; set; }

        public string zip { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_Contract> CRM_Contract { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_Contact> CRM_Contact { get; set; }
    }

    public partial class CRM_Customer_ReturnData
    {
        public string code { get; set; }

        public CRM_Customer_Data data { get; set; }
    }

    public partial class CRM_Customer_Data
    {
        public List<CRM_CustomerDetail> customers { get; set; }

        public int total_count { get; set; }
    }

    public partial class CRM_CustomerDetail
    {
        public int id { get; set; }

        public string name { get; set; }

        public CRM_CustomerAddress address { get; set; }

        public List<CRM_CustomerContacts> contacts { get; set; }
    }

    public partial class CRM_CustomerAddress
    {
        public string detail_address { get; set; }

        public string region_info { get; set; }

        public string tel { get; set; }
    }

    public partial class CRM_CustomerContacts
    {
        public CRM_CustomerContactsAddress address { get; set; }

        public string name { get; set; }
    }
    
    public partial class CRM_CustomerContactsAddress
    {
        public int addressable_id { get; set; }

        public string region_info { get; set; }

        public string detail_address { get; set; }

        public string tel { get; set; }
    }

    [Table("CRM_Contact")]
    public partial class CRM_Contact
    {
        public int Id { get; set; }
        
        public int contact_id { get; set; }

        [StringLength(64)]
        public string contact_name { get; set; }

        [StringLength(256)]
        public string contact_address { get; set; }

        [StringLength(64)]
        public string contact_tel { get; set; }

        public int customer_id { get; set; }

        public virtual CRM_Customer CRM_Customer { get; set; }

        public string province { get; set; }

        public string city { get; set; }

        public string district { get; set; }

        public string zip { get; set; }
    }

    public class result_Data
    {
        public string code { get; set; }

        public CRM_User_Token data { get; set; }
    }

    public static class UserInfo
    {
        /// 账户名
        public static string login = "18817958576";
        //public static string login = "15921503329";
        ///账户密码
        public static string password = "mengyu24";
        /// 类型
        public static string device = "dingtalk";
        ///CRM订单状态：未开始
        public static string status_unsend = "3531567";
        ///CRM订单状态：已同步待发货
        public static string status_undelivered = "3531568";
        ///CRM订单状态：已发货
        public static string status_delivered = "3764330";


    }

    public partial class orders_Result
    {
        public bool success { get; set; }//响应成功/响应失败
        public string errorCode { get; set; }//错误代码
        public string subErrorCode { get; set; }//子错误代码
        public string errorDesc { get; set; }//错误描述
        public string subErrorDesc { get; set; }//子错误描述
        public string requestMethod { get; set; }//请求接口方法
        public List<orders_List> orders { get; set; }
        public int total;
    }

    public partial class orders_List
    {
        public List<deliverys_List> deliverys { get; set; }

        public string platform_code { get; set; }
    }

    public partial class deliverys_List
    {
        public string express_code { get; set; }

        public string express_name { get; set; }

        public string mail_no { get; set; }

    }
    
}