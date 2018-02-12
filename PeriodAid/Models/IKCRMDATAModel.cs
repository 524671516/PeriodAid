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

        public DateTime? created_at { get; set; }

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
}