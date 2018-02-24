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
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
           
        }
    }
    [Table("CRM_Product")]
    public partial class CRM_Product
    {
        public int Id { get; set; }

        public string Item_Code { get; set; }

        public string System_Code { get; set; }

        public string Item_Name { get; set; }
    }
    
    [Table("CRM_ContractDetail")]
    public partial class CRM_ContractDetail
    {
        public int Id { get; set; }

        public string contacts_name { get; set; }

        public string contacts_tel { get; set; }

        public string contacts_address { get; set; }

        public int customer_address { get; set; }

        public int contract_id { get; set; }

        public int product_id { get; set; }

        public int product_quantity { get; set; }
    }
    [Table("CRM_User_Token")]
    public partial class CRM_User_Token
    {
        public int Id { get; set; }

        public string user_token { get; set; }

        public DateTime download_at { get; set; }
    }

    public class orders_Result
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

    public class CRM_Contract_ReturnData
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

        public int user_id { get; set; }

        public string user_name { get; set; }

        public int customer_id { get; set; }

        public string customer_name { get; set; }

        public string title { get; set; }

        public decimal total_amount { get; set; }

        public string status { get; set; }

        public DateTime? updated_at { get; set; }
    }
}