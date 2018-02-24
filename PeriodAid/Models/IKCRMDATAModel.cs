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
    }
    [Table("CRM_Product")]
    public partial class CRM_Product
    {
        public int Id { get; set; }

        public string Item_Code { get; set; }

        public string System_Code { get; set; }

        public string Item_Name { get; set; }
    }
    [Table("CRM_Contract")]
    public partial class CRM_Contract
    {
        public int Id { get; set; }

        public string user_id { get; set; }

        public string contract_id { get; set; }

        public string customer_id { get; set; }

        public string customer_name { get; set; }

        public string status { get; set; }

        public string special_terms { get; set; }

        public DateTime? sign_date { get; set; }

        public DateTime? updated_at { get; set; }
    }
    [Table("CRM_ContractDetail")]
    public partial class CRM_ContractDetail
    {
        public int Id { get; set; }

        public int product_no { get; set; }

        public string product_name { get; set; }

        public int product_count { get; set; }

        public decimal product_price { get; set; }
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
}