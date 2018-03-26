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
        public virtual DbSet<CRM_User> CRM_User { get; set; }
        public virtual DbSet<CRM_Department> CRM_Department { get; set; }
        public virtual DbSet<CRM_Role> CRM_Role { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CRM_Customer>().HasMany(m => m.CRM_Contract).WithRequired(m => m.CRM_Customer).HasForeignKey(m => m.customer_id).WillCascadeOnDelete(false);
            modelBuilder.Entity<CRM_Customer>().HasMany(m => m.CRM_Contact).WithRequired(m => m.CRM_Customer).HasForeignKey(m => m.customer_id).WillCascadeOnDelete(false);
            modelBuilder.Entity<CRM_Contract>().HasMany(m => m.CRM_ContractDetail).WithRequired(m => m.CRM_Contract).HasForeignKey(m => m.contract_id).WillCascadeOnDelete(false);
            modelBuilder.Entity<CRM_Department>().HasMany(m => m.CRM_User).WithRequired(m => m.CRM_Department).HasForeignKey(m => m.department_id).WillCascadeOnDelete(false);
            modelBuilder.Entity<CRM_Role>().HasMany(m => m.CRM_User).WithRequired(m => m.CRM_Role).HasForeignKey(m => m.role_id).WillCascadeOnDelete(false);
            //modelBuilder.Entity<CRM_User>().HasMany(m => m.CRM_Contract).WithRequired(m => m.CRM_User).HasForeignKey(m => m.user_id).WillCascadeOnDelete(false);
        }
    }

    [Table("CRM_User_Token")]
    public partial class CRM_User_Token
    {
        public int Id { get; set; }

        public string Key { get; set; }
        [StringLength(256)]
        public string Value { get; set; }

        public DateTime download_at { get; set; }

    }
    [Table("CRM_Department")]
    public partial class CRM_Department
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string name { get; set; }

        public int level { get; set; }

        public int? parent_id { get; set; }

        public Boolean can_use { get; set; }

        public int system_code { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_User> CRM_User { get; set; }
    }
    [Table("CRM_Role")]
    public partial class CRM_Role
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string name { get; set; }

        [StringLength(64)]
        public string entity_grant_scope { get; set; }

        public int system_code { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_User> CRM_User { get; set; }
    }
    [Table("CRM_User")]
    public partial class CRM_User
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string email { get; set; }

        public DateTime? created_at { get; set; }

        [StringLength(32)]
        public string name { get; set; }

        [StringLength(16), RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        public string phone { get; set; }

        public int role_id { get; set; }
        
        public int department_id { get; set; }

        public int superior_id { get; set; }

        public int system_code { get; set; }

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        //public virtual ICollection<CRM_Contract> CRM_Contract { get; set; }

        public virtual CRM_Department CRM_Department { get; set; }

        public virtual CRM_Role CRM_Role { get; set; }
        

    }

    [Table("CRM_Contract")]
    public class CRM_Contract
    {
        public int id { get; set; }

        public int contract_id { get; set; }

        [StringLength(64)]
        public string platform_code { get; set; }

        public int user_id { get; set; }

        public string user_name { get; set; }

        public int customer_id { get; set; }

        public virtual CRM_Customer CRM_Customer { get; set; }

        [StringLength(128)]
        public string contract_title { get; set; }

        public double total_amount { get; set; }

        public double unreceived_amount { get; set; }

        [StringLength(32)]
        public string contract_status { get; set; }
        [StringLength(256)]
        public string contract_remark { get; set; }
        // 类型
        [StringLength(64)]
        public string contract_type { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? edit_time { get; set; }
        [StringLength(256)]
        public string express_information { get; set; }
        [StringLength(64)]
        public string express_code { get; set; }
        [StringLength(32)]
        public string express_status { get; set; }
        [StringLength(256)]
        public string express_remark { get; set; }
        [StringLength(128)]
        public string shop_code { get; set; }
        [StringLength(128)]
        public string warehouse_code { get; set; }
        [StringLength(128)]
        public string vip_code { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_ContractDetail> CRM_ContractDetail { get; set; }
        // 收货人
        [StringLength(64)]
        public string receiver_name { get; set; }
        // 收货地址
        [StringLength(256)]
        public string receiver_address { get; set; }
        // 电话
        [StringLength(64)]
        public string receiver_tel { get; set; }
        [StringLength(64)]
        public string receiver_province { get; set; }
        [StringLength(64)]
        public string receiver_city { get; set; }
        [StringLength(128)]
        public string receiver_district { get; set; }
        //地址检测(1通过，0出错)
        public int address_status { get; set; }
        //订单回款类型(1付清,0未付清)
        public int received_payments_status { get; set; }

        public int employee_id { get; set; }
        [StringLength(64)]
        public string employee_name { get; set; }

        //public virtual CRM_User CRM_User { get; set; }
    }

    [Table("CRM_ExceptionLogs")]
    public partial class CRM_ExceptionLogs
    {
        public int Id { get; set; }

        public string type { get; set; }

        public string exception { get; set; }

        public DateTime exception_at { get; set; }
    }
    [Table("CRM_ContractDetail")]
    public partial class CRM_ContractDetail
    {
        public int Id { get; set; }

        public int contract_id { get; set; }

        public virtual CRM_Contract CRM_Contract { get; set; }

        public int product_id { get; set; }
        [StringLength(64)]
        public string product_name { get; set; }
        [StringLength(128)]
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
        [StringLength(32)]
        public string customer_abbreviation { get; set; }
        [StringLength(256)]
        public string customer_address { get; set; }
        [StringLength(64)]
        public string customer_tel { get; set; }
        //[StringLength(64)]
        //public string province { get; set; }
        //[StringLength(64)]
        //public string city { get; set; }
        //[StringLength(128)]
        //public string district { get; set; }
        //[StringLength(32)]
        //public string zip { get; set; }
        // 0 正常 -1删除
        public int status { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_Contract> CRM_Contract { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CRM_Contact> CRM_Contact { get; set; }
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
        // 0 存在 -1删除
        public int status { get; set; }

        public virtual CRM_Customer CRM_Customer { get; set; }
        //[StringLength(64)]
        //public string province { get; set; }
        //[StringLength(64)]
        //public string city { get; set; }
        //[StringLength(128)]
        //public string district { get; set; }
        //[StringLength(32)]
        //public string zip { get; set; }
    }
    public partial class CRM_Contract_ReturnData
    {
        public string code { get; set; }

        public CRM_Contract_Data data { get; set; }
    }

    public partial class CRM_Contract_Data
    {
        public List<CRM_ContractResult> contracts { get; set; }
    }

    public partial class CRM_ContractResult
    {
        public string approve_status { get; set; }

        public int id { get; set; }

        public int user_id { get; set; }

        public string user_name { get; set; }

        public int customer_id { get; set; }

        public string title { get; set; }

        public decimal total_amount { get; set; }

        public decimal unreceived_amount { get; set; }

        public string status { get; set; }

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
        //回款金额
        public decimal received_payments_amount { get; set; }
        //回款类型
        public string text_asset_c33e2b { get; set; }
        // 物流备注
        public string text_asset_7fd81a { get; set; }
        // 备注
        public string special_terms { get; set; }
        // 店铺
        public string text_asset_615f62_display { get; set; }
        // 订单类型
        public string category_mapped { get; set; }
        // 支付类型
        public string payment_type_mapped { get; set; }

        public List<CRM_Department> options { get; set; }

        public List<users> users { get; set; }

        public DateTime created_at { get; set; }
        // 创建者
        //public List<contractsCreator> creator { get; set; }
    }

    public partial class users
    {
        public int Id { get; set; }

        public string email { get; set; }

        public DateTime? created_at { get; set; }

        public string name { get; set; }

        public string phone { get; set; }

        public int role_id { get; set; }

        public int department_id { get; set; }

        public int superior_id { get; set; }

        public role_json role_json { get; set; }
    }

    public partial class role_json
    {
        public int Id { get; set; }

        public string name { get; set; }

        public string entity_grant_scope { get; set; }
    }
    
    public partial class CRM_ContractDetail_CustomerProductList
    {
        public int product_id { get; set; }

        public string product_no { get; set; }

        public int quantity { get; set; }

        public decimal recommended_unit_price { get; set; }

        public string name { get; set; }
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

        public string wechat { get; set; }
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

        public string phone { get; set; }
    }

    public class result_Data
    {
        public string code { get; set; }

        public User_tokenResult data { get; set; }
    }

    public class User_tokenResult
    {
        public string user_token;
    }

    public static class UserInfo
    {
        /// 账户名
        public static string login = "13636314852";
        ///账户密码
        public static string password = "Shouquanzhai2017";
        /// 类型
        public static string device = "dingtalk";
        ///CRM订单状态：待提交
        public static string status_unsend = "3531567";
        ///CRM订单状态：已同步待发货
        public static string status_undelivered = "3531568";
        ///CRM订单状态：已发货
        public static string status_delivered = "3764330";
        ///CRM订单状态：成功结束
        public static string status_success = "3531569";
        ///CRM订单状态：意外终止
        public static string status_end = "3531570";
        ///CRM订单状态：部分发货
        public static string status_part = "3890335";
        ///CRM订单状态：待财审
        public static string status_pendingApproval = "3907271";
        // 审核状态：已通过
        public static string approved_status = "approved";
        //订单回款类型：先款后货
        public static string received_payments = "sel_99ef";
        //订单回款类型：先货后款
        public static string unreceived_payments = "sel_1792";
        //订单回款类型：无需回款
        public static string nonEssential_payments = "sel_3235";
        // 数量
        public static int Count = 100;
        // 直接提交订单
        public static int received_payments_status = 1;
        // 已删除
        public static string delete = "-1";
        //权限
        //超管
        public static int SuperAdmin = 3;
        //主管
        public static int Manager = 1;
        //内勤
        public static int Assistant = 5;
        //财务
        public static int Finance = 2;
        //销售
        public static int Seller = 4;
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

        public int delivery_state { get; set; }
        
    }

    public partial class deliverys_List
    {
        public string express_code { get; set; }

        public string express_name { get; set; }

        public string mail_no { get; set; }

    }

    public partial class deliverys_Result
    {
        public bool success { get; set; }//响应成功/响应失败
        public string errorCode { get; set; }//错误代码
        public string subErrorCode { get; set; }//子错误代码
        public string errorDesc { get; set; }//错误描述
        public string subErrorDesc { get; set; }//子错误描述
        public string requestMethod { get; set; }//请求接口方法
        public List<deliverys_List> deliverys { get; set; }
        public int total;
    }

    public partial class deliverys_List
    {
        public string seller_memo { get; set; }
    }
}