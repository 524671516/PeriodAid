using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
namespace PeriodAid.Models
{
    public partial class Promotion : DbContext
    {
        public Promotion()
            : base("name=PromotionConnection")
        {
        }

        public virtual DbSet<Promotion_TJH> Promotion_TJH { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
        
    }
    public partial class Promotion_TJH
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string openId { get; set; }

        [Required]
        [StringLength(10)]
        public string name { get; set; }

        [Required]
        [StringLength(32)]
        public string mobile { get; set; }

        [StringLength(32)]
        public string branch { get; set; }

        public int status { get; set; }

        [StringLength(128)]
        public string mch_billno { get; set; }

        [StringLength(32)]
        public string mch_result { get; set; }

        public DateTime? submit_time { get; set; }
    }

    // ÀÕπÎ√€±Ì
    public partial class P_Presents
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string openId { get; set; }

        public int status { get; set; }

        public string source_ReceiverName { get; set; }

        public string source_ReceiverMobile { get; set; }

        public string source_ReceiverAddress { get; set; }

        public string target_ReceiverName { get; set; }

        public string target_ReceiverMobile { get; set; }

        public string target_ReceiverState { get; set; }

        public string target_ReceiverCity { get; set; }

        public string target_ReceiverDistrict { get; set; }

        public string target_ReceiverAddress { get; set; }

        public string express_name { get; set; }

        public string mail_no { get; set; }
    }
}
