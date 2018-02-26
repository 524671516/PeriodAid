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
        public virtual DbSet<CRM_Product> CRM_Product { get; set; }
        public virtual DbSet<CRM_Contract> CRM_Contract { get; set; }
        public virtual DbSet<CRM_ContractDetail> CRM_ContractDetail { get; set; }
        public virtual DbSet<CRM_User_Token> CRM_User_Token { get; set; }
        public virtual DbSet<CRM_ExceptionLogs> CRM_ExceptionLogs { get; set; }
        public virtual DbSet<CRM_Customer> CRM_Customer { get; set; }
        public virtual DbSet<CRM_Contact> CRM_Contact { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CRM_Product>().HasMany(m => m.CRM_ContractDetail).WithRequired(m => m.CRM_Product).HasForeignKey(m => m.product_id).WillCascadeOnDelete(false);
            modelBuilder.Entity<CRM_Customer>().HasMany(m => m.CRM_Contract).WithRequired(m => m.CRM_Customer).HasForeignKey(m => m.customer_id).WillCascadeOnDelete(false);
            modelBuilder.Entity<CRM_Customer>().HasMany(m => m.CRM_Contact).WithRequired(m => m.CRM_Customer).HasForeignKey(m => m.customer_id).WillCascadeOnDelete(false);
        }
    }
    [Table("CRM_Product")]
    public partial class CRM_Product
    {
        public int Id { get; set; }

        [StringLength(64)]
        public string Item_Code { get; set; }

        [StringLength(64)]
        public string System_Code { get; set; }

        [StringLength(64)]
        public string Item_Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_ContractDetail> CRM_ContractDetail { get; set; }
    }
    
    [Table("CRM_User_Token")]
    public partial class CRM_User_Token
    {
        public int Id { get; set; }

        [StringLength(256)]
        public string user_token { get; set; }

        public DateTime download_at { get; set; }
    }

    public partial class orders_Result
    {
        public bool success { get; set; }//响应成功/响应失败
        public string errorCode { get; set; }//错误代码
        public string subErrorCode { get; set; }//子错误代码
        public string errorDesc { get; set; }//错误描述
        public string subErrorDesc { get; set; }//子错误描述
        public string requestMethod { get; set; }//请求接口方法
        public List<orders> orders { get; set; }
        public int total;
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

        public int user_id { get; set; }

        public string user_name { get; set; }

        public int customer_id { get; set; }

        public virtual CRM_Customer CRM_Customer { get; set; }

        public string title { get; set; }

        public decimal total_amount { get; set; }

        public string status { get; set; }

        public DateTime? updated_at { get; set; }
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
        //public CRM_ContractDetail_Customer customer { get; set; }
        
        public List<CRM_ContractDetail_CustomerProductList> product_assets_for_new_record { get; set; }
        
    }
    
    public partial class CRM_ContractDetail_Customer
    {
        public CRM_ContractDetail_CustomerAddress address { get; set; }
    }

    public partial class CRM_ContractDetail_CustomerAddress
    {
        public string full_address { get; set; }

        public string tel { get; set; }
    }

    public partial class CRM_ContractDetail_CustomerProductList
    {
        public int product_id { get; set; }

        public string product_no { get; set; }

        public int quantity { get; set; }

        public decimal recommended_unit_price { get; set; }

        public decimal standard_unit_price { get; set; }
    }
    

    [Table("CRM_ContractDetail")]
    public partial class CRM_ContractDetail
    {
        public int Id { get; set; }

        public int contract_id { get; set; }

        public int product_id { get; set; }

        public int product_quantity { get; set; }

        public decimal standard_unit_price { get; set; }

        public virtual CRM_Product CRM_Product { get; set; }
    }

    [Table("CRM_Customer")]
    public partial class CRM_Customer
    {
        public int Id { get; set; }

        public int customer_id { get; set; }

        public string customer_name { get; set; }

        public string customer_address { get; set; }

        public string customer_tel { get; set; }

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
        public int addressable_id { get; set; }

        public string detail_address { get; set; }

        public string region_info { get; set; }

        public string tel { get; set; }
    }

    public partial class CRM_CustomerContacts
    {
        public CRM_CustomerContactsAddress address { get; set; }
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

        public string contact_name { get; set; }

        public string contact_address { get; set; }

        public string contact_tel { get; set; }

        public int customer_id { get; set; }

        public virtual CRM_Customer CRM_Customer { get; set; }
    }

    public class result_Data
    {
        public string code { get; set; }

        public CRM_User_Token data { get; set; }
    }
    public static class UserInfo
    {
        /// 账户名
        public static string login = "15921503329";
        ///账户密码
        public static string password = "mengyu24";
        /// 类型
        public static string device = "dingtalk";
        ///CRM订单状态：待同步
        public static int status_unsend = 3779515;
        ///CRM订单状态：已同步待发货
        public static int status_undelivered = 3780205;
        ///CRM订单状态：已发货
        public static int status_delivered = 3779516;


    }
}