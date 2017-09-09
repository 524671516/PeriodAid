using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Collections.Generic;
namespace PeriodAid.Models
{
    public partial class Promotion : DbContext
    {
        public Promotion()
            : base("name=PromotionConnection")
        {
        }

        public virtual DbSet<Promotion_TJH> Promotion_TJH { get; set; }
        public virtual DbSet<P_Presents> P_Presents { get; set; }
        public virtual DbSet<UNI_Group> UNI_Group { get; set; }
        public virtual DbSet<UNI_MchBill> UNI_MchBill { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UNI_Group>()
                .HasMany(e => e.UNI_MchBill)
                .WithRequired(e => e.UNI_Group)
                .HasForeignKey(e => e.GroupId);
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

        [Required]
        [StringLength(32)]
        public string source_ReceiverName { get; set; }

        [Required]
        [StringLength(32)]
        public string source_ReceiverMobile { get; set; }

        [Required]
        [StringLength(256)]
        public string source_ReceiverAddress { get; set; }
        
        [StringLength(32)]
        public string target_ReceiverName { get; set; }

        [StringLength(32)]
        public string target_ReceiverMobile { get; set; }

        [StringLength(32)]
        public string target_ReceiverState { get; set; }

        [StringLength(32)]
        public string target_ReceiverCity { get; set; }

        [StringLength(32)]
        public string target_ReceiverDistrict { get; set; }

        [StringLength(256)]
        public string target_ReceiverAddress { get; set; }

        [StringLength(16)]
        public string target_ReceiverZip { get; set; }

        [StringLength(32)]
        public string express_name { get; set; }

        [StringLength(32)]
        public string mail_no { get; set; }

        public DateTime create_time { get; set; }

        [StringLength(128)]
        public string plattform_code { get; set; }
    }

    public partial class UNI_Group
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string GroupCode { get; set; }

        [Required]
        [StringLength(32)]
        public string GroupName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UNI_MchBill> UNI_MchBill { get; set; }
    }

    public partial class UNI_MchBill
    {
        public int Id { get; set; }

        public int GroupId { get; set; }

        public virtual UNI_Group UNI_Group { get; set; }

        [StringLength(128)]
        public string NickName { get; set; }

        [StringLength(32)]
        [Required]
        public string Mobile { get; set; }

        [StringLength(128)]
        public string UploadImg { get; set; }

        [StringLength(32)]
        public string Province { get; set; }

        [StringLength(32)]
        public string City { get; set; }

        public bool Sex { get; set; }

        [StringLength(512)]
        public string ImgUrl { get; set; }

        [StringLength(32)]
        [Required]
        public string OpenId { get; set; }

        public int Status { get; set; }

        [StringLength(32)]
        public string StatusCode { get; set; }

        public int Mch_Amount { get; set; }

        [StringLength(64)]
        public string platform_code { get; set; }

        [StringLength(128)]
        public string MchBillNo { get; set; }

        public DateTime SendTime { get; set; }

        public DateTime? ReceiveTime { get; set; }
    }
}
