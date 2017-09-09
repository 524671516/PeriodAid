namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Spatial;

    public partial class AppPay : DbContext
    {
        public AppPay()
            : base("name=AppPayConnection")
        {
        }

        public virtual DbSet<WxPayOrder> WxPayMentOrder { get; set; }
        public virtual DbSet<WxRedPackOrder> WxRedPackOrder { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           
        }
    }

    [Table("WxPaymentOrder")]
    public partial class WxPaymentOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Trade_No { get; set; }

        public int Trade_Status { get; set; }

        [Required]
        [StringLength(128)]
        public string Open_Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Mch_Id { get; set; }

        [StringLength(32)]
        public string Device_Info { get; set; }

        [Required]
        [StringLength(32)]
        public string Body { get; set; }

        public string Detail { get; set; }

        [StringLength(128)]
        public string Attach { get; set; }

        [StringLength(16)]
        public string Bank_Type { get; set; }

        [StringLength(16)]
        public string Fee_Type { get; set; }

        public int Total_Fee { get; set; }

        public DateTime Time_Start { get; set; }

        public DateTime? Time_Expire { get; set; }

        [StringLength(32)]
        public string Goods_Tag { get; set; }

        [Required]
        [StringLength(16)]
        public string Trade_Type { get; set; }

        [StringLength(32)]
        public string Product_Id { get; set; }

        [StringLength(32)]
        public string Error_Msg { get; set; }

        [StringLength(128)]
        public string Error_Msg_Des { get; set; }

        [StringLength(64)]
        public string Prepay_Id { get; set; }
    }

    [Table("WxRedPackOrder")]
    public partial class WxRedPackOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string mch_billno { get; set; }

        [Required]
        [StringLength(32)]
        public string send_name { get; set; }

        [Required]
        [StringLength(32)]
        public string re_openid { get; set; }

        public int total_amount { get; set; }

        public int total_num { get; set; }

        [StringLength(128)]
        public string wishing { get; set; }

        [StringLength(32)]
        public string act_name { get; set; }

        [StringLength(256)]
        public string remark { get; set; }

        [StringLength(32)]
        public string detail_id { get; set; }

        [Required]
        [StringLength(16)]
        public string status { get; set; }

        [StringLength(32)]
        public string reason { get; set; }
        
        [StringLength(32)]
        public string hb_type { get; set; }

        [StringLength(32)]
        public string send_type { get; set; }

        [StringLength(128)]
        public string err_code_desc { get; set; }

        public DateTime? send_time { get; set; }

        public DateTime? refund_time { get; set; }

        public int? refund_amount { get; set; }

        public DateTime? rcv_time { get; set; }
    }
}
