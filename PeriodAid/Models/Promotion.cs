namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

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
        [StringLength(10, ErrorMessage ="�������ô���10���ַ�")]
        [Display(Name = "����")]
        public string name { get; set; }

        [Required]
        [StringLength(32, ErrorMessage = "�绰���벻�ô���32���ַ�")]
        [RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "�ֻ������ʽ����")]
        [Display(Name = "�ֻ�����")]
        public string mobile { get; set; }

        [StringLength(32, ErrorMessage ="ְλ���ô���32���ַ�")]
        [Display(Name = "ְҵ")]
        public string branch { get; set; }

        public int status { get; set; }

        [StringLength(128)]
        public string mch_billno { get; set; }

        [StringLength(32)]
        public string mch_result { get; set; }

        public DateTime? submit_time { get; set; }
    }
}
