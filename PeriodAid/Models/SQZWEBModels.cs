namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class SQZWEBModels : DbContext
    {
        public SQZWEBModels()
            : base("name=SQZWEBConnection")
        {
        }

        public virtual DbSet<AccessRecords> AccessRecords { get; set; }
        public virtual DbSet<CheckCode_Group> CheckCode_Group { get; set; }
        public virtual DbSet<CheckCodes> CheckCodes { get; set; }
        public virtual DbSet<Comments> Comments { get; set; }
        public virtual DbSet<FileAccessRecords> FileAccessRecords { get; set; }
        public virtual DbSet<SQZProducts> SQZProducts { get; set; }
        public virtual DbSet<Statistic_Group> Statistic_Group { get; set; }
        public virtual DbSet<Statistic_PageClick> Statistic_PageClick { get; set; }
        public virtual DbSet<Statistic_PageView> Statistic_PageView { get; set; }
        public virtual DbSet<verify_code> verify_code { get; set; }
        public virtual DbSet<verify_code_group> verify_code_group { get; set; }
        public virtual DbSet<Web_Event> Web_Event { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
           
            modelBuilder.Entity<CheckCode_Group>()
                .HasMany(e => e.AccessRecords)
                .WithRequired(e => e.CheckCode_Group)
                .HasForeignKey(e => e.CheckCode_Group_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CheckCode_Group>()
                .HasMany(e => e.CheckCodes)
                .WithRequired(e => e.CheckCode_Group)
                .HasForeignKey(e => e.CheckCode_Group_Id);

            modelBuilder.Entity<CheckCode_Group>()
                .HasMany(e => e.Statistic_PageClick)
                .WithRequired(e => e.CheckCode_Group)
                .HasForeignKey(e => e.CheckCode_Group_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Statistic_PageView>()
                .HasOptional(e => e.CheckCode_Group)
                .WithRequired(e => e.Statistic_PageView);

            modelBuilder.Entity<verify_code_group>()
                .HasMany(e => e.verify_code)
                .WithRequired(e => e.verify_code_group)
                .HasForeignKey(e => e.verify_group_id);
        }
    }
    public partial class SQZProducts
    {
        public int Id { get; set; }

        public int ProductType { get; set; }

        public string ProductName { get; set; }

        public string MainPhoto { get; set; }

        public string PhotoList { get; set; }

        public string Specification { get; set; }

        public string ExpireTime { get; set; }

        public string Content { get; set; }

        public string Discribe { get; set; }
    }
    public partial class Statistic_Group
    {
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string CreateUser { get; set; }

        public DateTime CreateTime { get; set; }

        public int? CheckCode_Group_Id { get; set; }
    }
    public partial class Statistic_PageClick
    {
        public int Id { get; set; }

        [StringLength(256)]
        public string Comment { get; set; }

        public DateTime ClickTime { get; set; }

        public int CheckCode_Group_Id { get; set; }

        public int ClickId { get; set; }

        public virtual CheckCode_Group CheckCode_Group { get; set; }
    }
    public partial class Statistic_PageView
    {
        public int Id { get; set; }

        public DateTime ViewTime { get; set; }

        [StringLength(64)]
        public string HostAddress { get; set; }

        public int CheckCode_Group_Id { get; set; }

        public virtual CheckCode_Group CheckCode_Group { get; set; }
    }
    public partial class verify_code
    {
        public int id { get; set; }

        public int verify_group_id { get; set; }

        [StringLength(32)]
        public string code { get; set; }

        public virtual verify_code_group verify_code_group { get; set; }
    }
    public partial class verify_code_group
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public verify_code_group()
        {
            verify_code = new HashSet<verify_code>();
        }

        public int id { get; set; }

        public DateTime create_time { get; set; }

        public int product_type { get; set; }

        public int count { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<verify_code> verify_code { get; set; }
    }
    public partial class Web_Event
    {
        public int Id { get; set; }

        public int Event_Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Event_Title { get; set; }

        [StringLength(256)]
        public string Event_Content { get; set; }

        public DateTime? Event_Date { get; set; }

        [StringLength(256)]
        public string Event_Img { get; set; }

        [Required]
        [StringLength(256)]
        public string Event_UpdateUser { get; set; }

        public DateTime Event_UpdateTime { get; set; }
    }
    public partial class AccessRecords
    {
        public int Id { get; set; }

        public DateTime AccessTime { get; set; }

        public int CheckCode_Group_Id { get; set; }

        public virtual CheckCode_Group CheckCode_Group { get; set; }
    }
    public partial class CheckCode_Group
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public CheckCode_Group()
        {
            AccessRecords = new HashSet<AccessRecords>();
            CheckCodes = new HashSet<CheckCodes>();
            Statistic_PageClick = new HashSet<Statistic_PageClick>();
        }

        public int Id { get; set; }

        [Required]
        public string GroupName { get; set; }

        public int EventType { get; set; }

        public bool EventStatus { get; set; }

        public string EventTitle { get; set; }

        public string EventDescription { get; set; }

        public string EventUrl { get; set; }

        public string EventPicUrl { get; set; }

        public bool Enable_Statistic { get; set; }

        [StringLength(256)]
        public string EventSourceUrl { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AccessRecords> AccessRecords { get; set; }

        public virtual Statistic_PageView Statistic_PageView { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<CheckCodes> CheckCodes { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Statistic_PageClick> Statistic_PageClick { get; set; }
    }
    public partial class CheckCodes
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; }

        public DateTime? ActivateTime { get; set; }

        public int CheckCode_Group_Id { get; set; }

        public virtual CheckCode_Group CheckCode_Group { get; set; }
    }
    public partial class Comments
    {
        public int Id { get; set; }

        public int EventCode { get; set; }

        [Required]
        [StringLength(100)]
        public string Content { get; set; }

        public bool IsNew { get; set; }

        public DateTime SubmitTime { get; set; }
    }
    public partial class FileAccessRecords
    {
        public int id { get; set; }

        [Required]
        public string CompanyName { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        public string Mobile { get; set; }

        public DateTime AccessTime { get; set; }
    }
    public class CheckCodeStatistics
    {
        public string  ClickTime { get; set; }
        public string  ViewTime { get; set; }
        public int? ViewNum { get; set; }
        public int? ClickId { get; set; }
        public int? ClickNum { get; set; }
    }


}
