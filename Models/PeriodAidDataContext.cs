namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Spatial;

    public partial class PeriodAidDataContext : DbContext
    {
        public PeriodAidDataContext()
            : base("name=PeriodAidConnection")
        {
        }

        public virtual DbSet<PeriodData> PeriodData { get; set; }
        public virtual DbSet<PeriodUserInfo> PeriodUserInfo { get; set; }
        public virtual DbSet<SMSRecord> SMSRecord { get; set; }
        public virtual DbSet<CreditsRecord> CreditsRecord { get; set; }
        public virtual DbSet<CreditsType> CreditsType { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            
            modelBuilder.Entity<CreditsType>()
                .HasMany(e => e.CreditsRecord)
                .WithRequired(e => e.CreditsType)
                .HasForeignKey(e => e.CreditsType_Id)
                .WillCascadeOnDelete(false);
        }
    }
    [Table("PeriodData")]
    public partial class PeriodData
    {
        public int Id { get; set; }

        public DateTime MC_Begin { get; set; }

        public int MC_Days { get; set; }

        public int MC_Cycle { get; set; }

        public int Period_Type { get; set; }

        public DateTime MC_Finish { get; set; }

        [Required]
        [StringLength(255)]
        public string UserName { get; set; }
    }

    [Table("PeriodUserInfo")]
    public partial class PeriodUserInfo
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string UserName { get; set; }

        public int UserAge { get; set; }

        public DateTime Last_MC_Begin { get; set; }

        public int MC_days { get; set; }

        public int MC_Cycle { get; set; }

        public bool Pregnancy { get; set; }

        public bool Pre_Pregnancy { get; set; }
    }

    [Table("SMSRecord")]
    public partial class SMSRecord
    {
        public int Id { get; set; }

        [Required]
        [StringLength(16)]
        public string Mobile { get; set; }

        [Required]
        [StringLength(8)]
        public string ValidateCode { get; set; }

        public bool Status { get; set; }

        public int SMS_Type { get; set; }

        public DateTime SendDate { get; set; }

        public bool SMS_Reply { get; set; }
    }
    [Table("CreditsRecord")]
    public partial class CreditsRecord
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string UserName { get; set; }

        [Required]
        public int CreditsType_Id { get; set; }

        [Required]
        public int Credits { get; set; }

        [Required]
        public DateTime RecordDate { get; set; }

        [StringLength(64)]
        public string CreditsDESC { get; set; }

        public virtual CreditsType CreditsType { get; set; }
    }

    [Table("CreditsType")]
    public partial class CreditsType
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string CreditsTypeName { get; set; }

        [Required]
        public int Credits { get; set; }

        [StringLength(128)]
        public string CreditsTypeDESC { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CreditsRecord> CreditsRecord { get; set; }
    }
}
