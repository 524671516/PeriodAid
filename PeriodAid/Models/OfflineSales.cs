namespace PeriodAid.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class OfflineSales : DbContext
    {
        public OfflineSales()
            : base("name=OfflineSalesConnection")
        {
        }
        
        public virtual DbSet<TestAnswer> TestAnswer { get; set; }
        public virtual DbSet<TestType> TestType { get; set; }
        public virtual DbSet<TestQuestion> TestQuestion { get; set; }
        public virtual DbSet<Examination> Examination { get; set; }
        public virtual DbSet<ExaminationDetails> ExaminationDetails { get; set; }

        public virtual DbSet<CustomOrder> CustomOrder { get; set; }

        public virtual DbSet<WxPayProduct> WxPayProduct { get; set; }
        public virtual DbSet<WxPayOrder> WxPayOrder { get; set; }
        public virtual DbSet<WxPayStatistic> WxPayStatistic { get; set; }

        public virtual DbSet<Benefits> Benefits { get; set; }

        public virtual DbSet<Off_SalesInfo_Daily> Off_SalesInfo_Daily { get; set; }
        public virtual DbSet<Off_SalesInfo_Month> Off_SalesInfo_Month { get; set; }
        public virtual DbSet<Off_Seller> Off_Seller { get; set; }
        public virtual DbSet<Off_Store> Off_Store { get; set; }
        public virtual DbSet<Off_Expenses> Off_Expenses { get; set; }
        public virtual DbSet<Off_Expenses_Details> Off_Expenses_Details { get; set; }
        public virtual DbSet<Off_Expenses_Payment> Off_Expenses_Payment { get; set; }
        public virtual DbSet<Off_Membership_Bind> Off_Membership_Bind { get; set; }
        public virtual DbSet<Off_Checkin_Schedule> Off_Checkin_Schedule { get; set; }
        public virtual DbSet<Off_Checkin> Off_Checkin { get; set; }
        public virtual DbSet<Off_StoreManager> Off_StoreManager { get; set; }
        public virtual DbSet<Off_Manager_Task> Off_Manager_Task { get; set; }
        public virtual DbSet<Off_Manager_CheckIn> Off_Manager_CheckIn { get; set; }
        public virtual DbSet<Off_Manager_Announcement> Off_Manager_Announcement { get; set; }
        public virtual DbSet<Off_Manager_Request> Off_Manager_Request { get; set; }
        public virtual DbSet<Off_BonusRequest> Off_BonusRequest { get; set; }
        public virtual DbSet<Off_System> Off_System { get; set; }
        public virtual DbSet<Off_Checkin_Product> Off_Checkin_Product { get; set; }
        public virtual DbSet<Off_Product> Off_Product { get; set; }
        public virtual DbSet<Off_Daily_Product> Off_Daily_Product { get; set; }
        public virtual DbSet<Off_AVG_Info> Off_AVG_Info { get; set; }
        public virtual DbSet<Off_System_Setting> Off_System_Setting { get; set; }
        public virtual DbSet<Off_SellerTask> Off_SellerTask { get; set; }
        public virtual DbSet<Off_SellerTaskProduct> Off_SellerTaskProduct { get; set; }
        public virtual DbSet<Off_CompetitionInfo> Off_CompetitionInfo { get; set; }
        public virtual DbSet<Off_Recruit> Off_Recruit { get; set; }
        public virtual DbSet<Off_WeekendBreak> Off_WeekendBreak { get; set; }
        public virtual DbSet<Off_WeekendBreakRecord> Off_WeekendBreakRecord { get; set; }

        public virtual DbSet<Off_StoreSystem> Off_StoreSystem { get; set; }
        public virtual DbSet<Off_SalesEvent> Off_SalesEvent { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestType>()
                .HasMany(e => e.TestQuestion)
                .WithRequired(e => e.TestType)
                .HasForeignKey(e => e.TestTypeID);

            modelBuilder.Entity<TestType>()
                .HasMany(e => e.Examination)
                .WithRequired(e => e.TestType)
                .HasForeignKey(e => e.TestTypeID);

            modelBuilder.Entity<TestQuestion>()
                .HasMany(e => e.TestAnswer)
                .WithRequired(e => e.TestQuestion)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TestQuestion>()
                .HasMany(e => e.ExaminationDetails)
                .WithRequired(e => e.TestQuestion)
                .HasForeignKey(e => e.QuestionID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Examination>()
                .HasMany(e => e.ExaminationDetails)
                .WithRequired(e => e.Examination)
                .HasForeignKey(e => e.ExaminationID)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_SalesInfo_Daily>()
                .Property(e => e.Salary)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Off_SalesInfo_Daily>()
                .Property(e => e.Bonus)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Off_Seller>()
                .HasMany(e => e.Off_SalesInfo_Daily)
                .WithOptional(e => e.Off_Seller)
                .HasForeignKey(e => e.SellerId);

            modelBuilder.Entity<Off_Seller>()
                .HasMany(e => e.Off_Membership_Bind)
                .WithOptional(e => e.Off_Seller)
                .HasForeignKey(e => e.Off_Seller_Id)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Seller>()
                .HasMany(e => e.Off_Checkin)
                .WithRequired(e => e.Off_Seller)
                .HasForeignKey(e => e.Off_Seller_Id)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Seller>()
                .HasMany(e => e.Off_SellerTask)
                .WithRequired(e => e.Off_Seller)
                .HasForeignKey(e => e.SellerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Checkin_Schedule>()
                .HasMany(e => e.Off_Checkin)
                .WithRequired(e => e.Off_Checkin_Schedule)
                .HasForeignKey(e => e.Off_Schedule_Id)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Checkin_Schedule>()
                .HasMany(e => e.Off_WeekendBreak)
                .WithRequired(e => e.Off_Checkin_Schedule)
                .HasForeignKey(e => e.ScheduleId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Checkin>()
                .HasMany(e => e.Off_BonusRequest)
                .WithRequired(e => e.Off_Checkin)
                .HasForeignKey(e => e.CheckinId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Checkin>()
                .HasMany(e => e.Off_Checkin_Product)
                .WithRequired(e => e.Off_Checkin)
                .HasForeignKey(e => e.CheckinId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_SalesInfo_Daily>()
                .HasMany(e => e.Off_Daily_Product)
                .WithRequired(e => e.Off_SalesInfo_Daily)
                .HasForeignKey(e => e.DailyId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_SalesInfo_Daily)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_SalesInfo_Month)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_Seller)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_Checkin_Schedule)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.Off_Store_Id)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_Manager_Request)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_AVG_Info)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_SellerTask)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_CompetitionInfo)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_StoreSystem)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Membership_Bind)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Seller)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_StoreManager)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Checkin_Schedule)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Expenses)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Manager_Task)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Product)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Manager_Announcement)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_System_Setting)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_System>()
                .HasMany(e => e.Off_Recruit)
                .WithRequired(e => e.Off_System)
                .HasForeignKey(e => e.Off_System_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_StoreManager>()
                .HasMany(e => e.Off_WeekendBreak).WithRequired(e => e.Off_StoreManager).HasForeignKey(e => e.StoreManagerId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Product>()
                .HasMany(e => e.Off_Checkin_Product)
                .WithRequired(e => e.Off_Product)
                .HasForeignKey(e => e.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Product>()
                .HasMany(e => e.Off_Daily_Product)
                .WithRequired(e => e.Off_Product)
                .HasForeignKey(e => e.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Product>()
                .HasMany(e => e.Off_SellerTaskProduct)
                .WithRequired(e => e.Off_Product)
                .HasForeignKey(e => e.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Off_Expenses>()
                .HasMany(e => e.Off_Expenses_Details)
                .WithRequired(e => e.Off_Expenses)
                .HasForeignKey(e => e.ExpensesId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Expenses>()
                .HasMany(e => e.Off_Expenses_Payment)
                .WithRequired(e => e.Off_Expenses)
                .HasForeignKey(e => e.ExpensesId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Manager_Task>()
                .HasMany(e => e.Off_Manager_CheckIn)
                .WithRequired(e => e.Off_Manager_Task)
                .HasForeignKey(e => e.Manager_EventId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_SellerTask>()
                .HasMany(e => e.Off_SellerTaskProduct)
                .WithRequired(e => e.Off_SellerTask)
                .HasForeignKey(e => e.SellerTaskId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_WeekendBreak>().HasMany(e => e.Off_WeekendBreakRecord).WithRequired(e => e.Off_WeekendBreak).HasForeignKey(e => e.WeekendBreakId).WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_StoreSystem>().HasMany(e => e.Off_Store).WithRequired(e => e.Off_StoreSystem).HasForeignKey(e => e.Off_StoreSystemId).WillCascadeOnDelete(false);
        }
    }

    
    #region 考试系统
    /*---------- 考试系统 ----------*/
    [Table("TestAnswer")]
    public partial class TestAnswer
    {
        public int ID { get; set; }

        public int TestQuestionId { get; set; }

        [Required]
        [StringLength(50)]
        public string AnswerContent { get; set; }

        public bool AnswerProperty { get; set; }

        public virtual TestQuestion TestQuestion { get; set; }
    }

    [Table("TestType")]
    public partial class TestType
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TestType()
        {
            TestQuestion = new HashSet<TestQuestion>();
            Examination = new HashSet<Examination>();
        }

        public int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string CustomName { get; set; }

        [Required]
        [StringLength(50)]
        public string ModifyUser { get; set; }

        public DateTime ModifyDateTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TestQuestion> TestQuestion { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Examination> Examination { get; set; }
    }

    [Table("TestQuestion")]
    public partial class TestQuestion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TestQuestion()
        {
            TestAnswer = new HashSet<TestAnswer>();
            ExaminationDetails = new HashSet<ExaminationDetails>();
        }

        public int ID { get; set; }

        public int TestTypeID { get; set; }


        [Required]
        [StringLength(256)]
        public string QuestionContent { get; set; }

        [Required]
        [StringLength(50)]
        public string modifyUser { get; set; }

        public DateTime modifyDateTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TestAnswer> TestAnswer { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExaminationDetails> ExaminationDetails { get; set; }

        public virtual TestType TestType { get; set; }
    }

    [Table("Examination")]
    public partial class Examination
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Examination()
        {
            ExaminationDetails = new HashSet<ExaminationDetails>();
        }

        public int ID { get; set; }

        [Required]
        [StringLength(256)]
        public string OpenId { get; set; }

        [StringLength(256)]
        public string NickName { get; set; }

        public int TestTypeID { get; set; }

        public int? CurrentSequence { get; set; }

        public int MaxSequence { get; set; }

        public bool ExaminationStatus { get; set; }

        public DateTime ExaminationCreateTime { get; set; }

        public DateTime? ExaminationFinishTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ExaminationDetails> ExaminationDetails { get; set; }

        public virtual TestType TestType { get; set; }
    }

    [Table("ExaminationDetails")]
    public partial class ExaminationDetails
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ExaminationDetails()
        {
        }

        public int ID { get; set; }

        public int ExaminationID { get; set; }

        public int Sequence { get; set; }

        public int QuestionID { get; set; }

        public bool? Result { get; set; }

        public virtual Examination Examination { get; set; }

        public virtual TestQuestion TestQuestion { get; set; }
    }
    #endregion

    #region 个性化定制
    /*---------- 个性化定制 ----------*/
    [Table("CustomOrder")]
    public partial class CustomOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNumber { get; set; }

        [Required]
        [StringLength(256)]
        public string OpenId { get; set; }

        [Required]
        [StringLength(100)]
        public string NickName { get; set; }

        [StringLength(50)]
        public string OrignalImage { get; set; }

        [StringLength(50)]
        public string CropImage { get; set; }

        public int OrderStatus { get; set; }

        [StringLength(256)]
        public string CardId { get; set; }

        public int? CardStatus { get; set; }
    }
    #endregion

    #region 微信支付订单
    /*---------- 微信支付订单 ----------*/
    public partial class WxPayProduct
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public string ProductDetails { get; set; }

        public decimal? Total_Fee { get; set; }
    }
    public partial class WxPayStatistic
    {
        public int Id { get; set; }

        public string EventName { get; set; }

        public DateTime ApplicationDate { get; set; }

        public string Details { get; set; }
    }

    [Table("WxPayOrder")]
    public partial class WxPayOrder
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Trade_No { get; set; }

        [StringLength(64)]
        public string Prepay_Id { get; set; }

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
    }
    #endregion

    #region 百家企业送姜茶
    /*---------- 百家企业送姜茶 ----------*/
    [Table("Benefits")]
    public partial class Benefits
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string OpenId { get; set; }

        [Required]
        public int Status { get; set; }

        [Required]
        [StringLength(32)]
        public string Name { get; set; }

        [Required]
        [StringLength(32)]
        public string Mobile { get; set; }

        [StringLength(64)]
        public string JobTitle { get; set; }

        [Required]
        [StringLength(64)]
        public string Company { get; set; }

        [StringLength(256)]
        public string Reason { get; set; }

        [Required]
        public int Staff { get; set; }

        [Required]
        [StringLength(32)]
        public string Region { get; set; }

        [Required]
        [StringLength(32)]
        public string Industry { get; set; }

        [StringLength(128)]
        public string Address { get; set; }

        [Required]
        public bool Share { get; set; }
    }
    #endregion

    public partial class Off_System
    {
        public int Id { get; set; }

        [StringLength(64)]
        public string SystemName { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Membership_Bind> Off_Membership_Bind { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Seller> Off_Seller { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_StoreSystem> Off_StoreSystem { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_StoreManager> Off_StoreManager { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin_Schedule> Off_Checkin_Schedule { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Expenses> Off_Expenses { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Manager_Announcement> Off_Manager_Announcement { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Manager_Task> Off_Manager_Task { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Product> Off_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_System_Setting> Off_System_Setting { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Recruit> Off_Recruit { get; set; }
    }
    public partial class Off_System_Setting
    {
        public int Id { get; set; }
        public int Off_System_Id { get; set; }
        public virtual Off_System Off_System { get; set; }
        public string SettingName { get; set; }
        public bool SettingResult { get; set; }
        public string SettingValue { get; set; }
    }
    public partial class Off_Product
    {
        public int Id { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }

        public int status { get; set; }

        [StringLength(64)]
        public string ItemCode { get; set; }

        [Required]
        [StringLength(128)]
        public string ItemName { get; set; }

        [Required]
        [StringLength(8)]
        public string SimpleName { get; set; }

        [StringLength(32)]
        public string Spec { get; set; }

        public decimal? SalesPrice { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin_Product> Off_Checkin_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Daily_Product> Off_Daily_Product { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SellerTaskProduct> Off_SellerTaskProduct { get; set; }
    }

    public partial class Off_Store
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Off_Store()
        {
            Off_SalesInfo_Daily = new HashSet<Off_SalesInfo_Daily>();
            Off_SalesInfo_Month = new HashSet<Off_SalesInfo_Month>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string StoreName { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(50)]
        public string Longitude { get; set; }

        [StringLength(50)]
        public string Latitude { get; set; }

        public DateTime? UploadTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SalesInfo_Daily> Off_SalesInfo_Daily { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SalesInfo_Month> Off_SalesInfo_Month { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Seller> Off_Seller { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin_Schedule> Off_Checkin_Schedule { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_StoreManager> Off_StoreManager { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Manager_Request> Off_Manager_Request { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_AVG_Info> Off_AVG_Info { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SellerTask> Off_SellerTask { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_CompetitionInfo> Off_CompetitionInfo { get; set; }

        public int Off_StoreSystemId { get; set; }

        public virtual Off_StoreSystem Off_StoreSystem { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SalesEvent> Off_SalesEvent { get; set; }
    }

    public partial class Off_Seller
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Off_Seller()
        {
            Off_SalesInfo_Daily = new HashSet<Off_SalesInfo_Daily>();
            Off_Membership_Bind = new HashSet<Off_Membership_Bind>();
            Off_Checkin = new HashSet<Off_Checkin>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Mobile { get; set; }

        public int StoreId { get; set; }

        [StringLength(20)]
        public string AccountName { get; set; }

        [StringLength(20)]
        public string IdNumber { get; set; }

        [StringLength(50)]
        public string CardName { get; set; }

        [StringLength(50)]
        public string CardNo { get; set; }

        [StringLength(50)]
        public string AccountSource { get; set; }

        public int StandardSalary { get; set; }

        public DateTime? UploadTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SalesInfo_Daily> Off_SalesInfo_Daily { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Membership_Bind> Off_Membership_Bind { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin> Off_Checkin { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SellerTask> Off_SellerTask { get; set; }

        public virtual Off_Store Off_Store { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_SalesInfo_Daily
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        public int? SellerId { get; set; }

        public int? Attendance { get; set; }

        public decimal? Salary { get; set; }

        public decimal? Debit { get; set; }

        public decimal? Bonus { get; set; }

        public bool isMultiple { get; set; }

        public string remarks { get; set; }

        public DateTime? UploadTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        public virtual Off_Seller Off_Seller { get; set; }

        public virtual Off_Store Off_Store { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Daily_Product> Off_Daily_Product { get; set; }
    }
    public partial class Off_Daily_Product
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int DailyId { get; set; }

        [StringLength(64)]
        public string ItemCode { get; set; }

        public int? SalesCount { get; set; }

        public int? StorageCount { get; set; }

        public decimal? SalesAmount { get; set; }

        public virtual Off_SalesInfo_Daily Off_SalesInfo_Daily { get; set; }

        public virtual Off_Product Off_Product { get; set; }
    }
    public partial class Off_SalesInfo_Month
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        public int? Item_Brown { get; set; }

        public int? Item_Black { get; set; }

        public int? Item_Lemon { get; set; }

        public int? Item_Honey { get; set; }

        public int? Item_Dates { get; set; }

        public decimal? TotalFee { get; set; }

        public DateTime? UploadTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        public virtual Off_Store Off_Store { get; set; }
    }

    public partial class Off_Expenses
    {
        public int Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ApplicationDate { get; set; }

        public int Status { get; set; }

        public string StoreSystem { get; set; }

        public string Distributor { get; set; }

        public int PaymentType { get; set; }

        public string Remarks { get; set; }

        public DateTime? CheckTime { get; set; }

        public DateTime? BalanceTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        public DateTime? UploadTime { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Expenses_Details> Off_Expenses_Details { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Expenses_Payment> Off_Expenses_Payment { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_Expenses_Details
    {
        public int Id { get; set; }

        public int ExpensesId { get; set; }

        public int ExpensesType { get; set; }

        public string DetailsName { get; set; }

        public decimal DetailsFee { get; set; }

        public string Remarks { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        public DateTime? UploadTime { get; set; }

        public virtual Off_Expenses Off_Expenses { get; set; }
    }

    public partial class Off_Expenses_Payment
    {
        public int Id { get; set; }

        public int ExpensesId { get; set; }

        public DateTime? ApplicationDate { get; set; }

        public int VerifyType { get; set; }

        public decimal? VerifyCost { get; set; }

        public string Remarks { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        public DateTime? UploadTime { get; set; }

        public virtual Off_Expenses Off_Expenses { get; set; }
    }

    public partial class Off_Membership_Bind
    {
        public int Id { get; set; }

        [Required]
        [StringLength(128)]
        public string UserName { get; set; }

        [Required]
        [StringLength(64)]
        public string NickName { get; set; }

        [Required]
        [StringLength(64)]
        public string Mobile { get; set; }

        public bool Bind { get; set; }

        public bool Recruit { get; set; }

        public int? Off_Seller_Id { get; set; }

        public DateTime ApplicationDate { get; set; }

        public virtual Off_Seller Off_Seller { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }

        public int Type { get; set; }
    }

    public partial class Off_Checkin_Schedule
    {
        public int Id { get; set; }

        public int Off_Store_Id { get; set; }

        public DateTime Subscribe { get; set; }

        public DateTime Standard_CheckIn { get; set; }

        public DateTime Standard_CheckOut { get; set; }

        public decimal? Standard_Salary { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin> Off_Checkin { get; set; }

        public virtual Off_Store Off_Store { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_WeekendBreak> Off_WeekendBreak { get; set; }
    }
    public partial class Off_Checkin
    {
        public int Id { get; set; }

        public int Off_Schedule_Id { get; set; }

        public int Status { get; set; }

        public bool Proxy { get; set; }

        public int Off_Seller_Id { get; set; }

        public DateTime? CheckinTime { get; set; }

        [StringLength(64)]
        public string CheckinLocation { get; set; }

        [StringLength(64)]
        public string CheckinPhoto { get; set; }

        public DateTime? CheckoutTime { get; set; }

        [StringLength(64)]
        public string CheckoutPhoto { get; set; }

        [StringLength(64)]
        public string CheckoutLocation { get; set; }

        [StringLength(512, ErrorMessage = "不超过512个字符")]
        public string Rep_Image { get; set; }

        [StringLength(512, ErrorMessage = "不超过512个字符")]
        public string Remark { get; set; }

        public DateTime? Report_Time { get; set; }

        [StringLength(64, ErrorMessage = "不超过64个字符")]
        public string ConfirmUser { get; set; }

        public DateTime? ConfirmTime { get; set; }

        [StringLength(512, ErrorMessage = "不超过512个字符")]
        public string Confirm_Remark { get; set; }

        public decimal? Bonus { get; set; }

        [StringLength(128, ErrorMessage = "不超过128个字符")]
        public string Bonus_Remark { get; set; }

        [StringLength(64, ErrorMessage = "不超过64个字符")]
        public string Bonus_User { get; set; }

        [StringLength(64, ErrorMessage = "不超过64个字符")]
        public string SubmitUser { get; set; }

        public DateTime? SubmitTime { get; set; }

        public virtual Off_Seller Off_Seller { get; set; }

        public virtual Off_Checkin_Schedule Off_Checkin_Schedule { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_BonusRequest> Off_BonusRequest { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin_Product> Off_Checkin_Product { get; set; }
    }

    public partial class Off_Checkin_Product
    {
        public int Id { get; set; }

        public int CheckinId { get; set; }

        public int ProductId { get; set; }

        [StringLength(64)]
        public string ItemCode { get; set; }

        public int? SalesCount { get; set; }

        public int? StorageCount { get; set; }

        public decimal? SalesAmount { get; set; }

        public virtual Off_Checkin Off_Checkin { get; set; }

        public virtual Off_Product Off_Product { get; set; }
    }

    public partial class Off_StoreManager
    {
        public int Id { get; set; }

        public int Status { get; set; }

        [StringLength(32)]
        public string UserName { get; set; }

        [StringLength(32)]
        public string NickName { get; set; }

        [StringLength(32)]
        public string Mobile { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Store> Off_Store { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_WeekendBreak> Off_WeekendBreak { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_Manager_Task
    {
        public int Id { get; set; }

        public int Status { get; set; }
        [StringLength(32)]
        public string UserName { get; set; }

        [StringLength(32)]
        public string NickName { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TaskDate { get; set; }

        [StringLength(512)]
        public string Photo { get; set; }

        [StringLength(512)]
        public string Event_Complete { get; set; }

        [StringLength(512)]
        public string Event_UnComplete { get; set; }

        [StringLength(512)]
        public string Event_Assistance { get; set; }

        public int? Eval_Value { get; set; }

        [StringLength(256)]
        public string Eval_Remark { get; set; }

        [StringLength(32)]
        public string Eval_User { get; set; }

        public DateTime? Eval_Time { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Manager_CheckIn> Off_Manager_CheckIn { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_Manager_CheckIn
    {
        public int Id { get; set; }

        public int Manager_EventId { get; set; }

        public bool Canceled { get; set; }

        [StringLength(64)]
        public string Location { get; set; }

        [StringLength(128)]
        public string Location_Desc { get; set; }

        [StringLength(128)]
        public string Photo { get; set; }

        [StringLength(64)]
        public string Remark { get; set; }

        public DateTime CheckIn_Time { get; set; }

        public virtual Off_Manager_Task Off_Manager_Task { get; set; }
    }

    public partial class Off_AVG_Info
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        public int DayOfWeek { get; set; }

        public decimal? AVG_SalesData { get; set; }

        public decimal? AVG_AmountData { get; set; }

        public virtual Off_Store Off_Store { get; set; }
    }

    public partial class Off_Manager_Announcement
    {
        public int Id { get; set; }

        [StringLength(512)]
        public string ManagerUserName { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime FinishTime { get; set; }

        [Range(1, 5)]
        public int Priority { get; set; }

        [StringLength(64)]
        public string Title { get; set; }

        [StringLength(1024)]
        public string Content { get; set; }

        [StringLength(64)]
        public string SubmitUser { get; set; }

        public DateTime SubmitTime { get; set; }

        public bool Status { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_Manager_Request
    {
        public int Id { get; set; }

        [StringLength(64)]
        public string ManagerUserName { get; set; }

        public int StoreId { get; set; }

        public int Status { get; set; }

        [StringLength(64)]
        public string RequestType { get; set; }

        [StringLength(1024)]
        public string RequestContent { get; set; }

        [StringLength(512)]
        public string RequestRemark { get; set; }

        public DateTime RequestTime { get; set; }

        [StringLength(512)]
        public string ReplyContent { get; set; }

        [StringLength(64)]
        public string ReplyUser { get; set; }

        public DateTime? ReplyTime { get; set; }

        public virtual Off_Store Off_Store { get; set; }
    }

    public partial class Off_BonusRequest
    {
        public int Id { get; set; }

        public int CheckinId { get; set; }

        public int Status { get; set; }

        [StringLength(64)]
        public string ReceiveOpenId { get; set; }

        [StringLength(128)]
        public string ReceiveUserName { get; set; }

        public int ReceiveAmount { get; set; }

        [StringLength(128)]
        public string RequestUserName { get; set; }

        public DateTime RequestTime { get; set; }

        [StringLength(128)]
        public string CommitUserName { get; set; }

        public DateTime? CommitTime { get; set; }

        [StringLength(256)]
        public string Mch_BillNo { get; set; }

        public virtual Off_Checkin Off_Checkin { get; set; }
    }

    public partial class Off_SellerTask
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        public int SellerId { get; set; }

        public DateTime ApplyDate { get; set; }

        [StringLength(256)]
        public string TaskPhotoList { get; set; }

        [StringLength(64)]
        public string LastUpdateUser { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public virtual Off_Seller Off_Seller { get; set; }

        public virtual Off_Store Off_Store { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SellerTaskProduct> Off_SellerTaskProduct { get; set; }
    }

    public partial class Off_SellerTaskProduct
    {
        public int Id { get; set; }

        public int SellerTaskId { get; set; }

        public int ProductId { get; set; }

        [StringLength(64)]
        public string ItemCode { get; set; }

        public decimal? SalesAmount { get; set; }

        public int? SalesCount { get; set; }

        public int? StorageCount { get; set; }

        public virtual Off_SellerTask Off_SellerTask { get; set; }

        public virtual Off_Product Off_Product { get; set; }
    }

    public partial class Off_CompetitionInfo
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        [StringLength(512, ErrorMessage = "不超过512个字符")]
        public string CompetitionImage { get; set; }

        [StringLength(512, ErrorMessage = "不超过512个字符")]
        public string Remark { get; set; }

        [StringLength(64)]
        public string ReceiveOpenId { get; set; }

        [StringLength(128)]
        public string ReceiveUserName { get; set; }

        [StringLength(128)]
        public string NickName { get; set; }

        public decimal? BonusAmount { get; set; }

        public int Status { get; set; }

        public DateTime ApplicationDate { get; set; }

        public DateTime? BonusApplyDate { get; set; }

        public string BonusApplyUser { get; set; }

        [StringLength(256)]
        public string Mch_BillNo { get; set; }

        public virtual Off_Store Off_Store { get; set; }
    }

    public partial class Off_Recruit
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string Name { get; set; }

        [StringLength(32)]
        public string UserName { get; set; }

        [StringLength(32)]
        public string Mobile { get; set; }

        public int Status { get; set; }

        [StringLength(256)]
        public string Area { get; set; }

        [StringLength(64)]
        public string WorkType { get; set; }

        [StringLength(64)]
        public string IdNumber { get; set; }

        [StringLength(128)]
        public string RecommandUserId { get; set; }

        public bool Reward { get; set; }

        public DateTime ApplyTime { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }
    }

    public partial class Off_WeekendBreak
    {
        public int Id { get; set; }

        public int StoreManagerId { get; set; }

        public int ScheduleId { get; set; }

        public DateTime Subscribe { get; set; }

        public DateTime SignInTime { get; set; }

        [StringLength(32)]
        public string UserName { get; set; }

        public DateTime? LastUploadTime { get; set; }

        public int? TrailDefault { get; set; }

        public virtual Off_StoreManager Off_StoreManager { get; set; }

        public virtual Off_Checkin_Schedule Off_Checkin_Schedule { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_WeekendBreakRecord> Off_WeekendBreakRecord { get; set; }
    }

    public partial class Off_WeekendBreakRecord
    {
        public int Id { get; set; }

        public int WeekendBreakId { get; set; }

        public DateTime UploadTime { get; set; }

        public int SalesCount { get; set; }

        public int TrailCount { get; set; }

        public string SalesDetails { get; set; }

        public virtual Off_WeekendBreak Off_WeekendBreak { get; set; }
    }

    public partial class Off_StoreSystem
    {
        public int Id { get; set; }

        [StringLength(16)]
        public string SystemName { get; set; }

        [StringLength(32)]
        public string Distributor { get; set; }

        [StringLength(8)]
        public string Region { get; set; }

        public bool RequiredStorage { get; set; }

        public bool RequiredAmount { get; set; }

        [Required]
        public string ProductList { get; set; }

        public int Off_System_Id { get; set; }

        public virtual Off_System Off_System { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Store> Off_Store { get; set; }
    }

    public partial class Off_SalesEvent
    {
        public int Id { get; set; }

        public int Status { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [StringLength(32)]
        public string SerialNo { get; set; }

        public int Off_StoreSystem_Id { get; set; }

        public virtual Off_StoreSystem Off_StoreSystem { get; set; }

        [StringLength(16)]
        public string EventType { get; set; }

        [StringLength(32)]
        public string EventTypeName { get; set; }

        [StringLength(64)]
        public string EventTitle { get; set; }

        [StringLength(256)]
        public string EventDetails { get; set; }

        public decimal? OrderAmount { get; set; }

        public decimal? CostAmount { get; set; }

        [StringLength(256)]
        public string OrderDetials { get; set; }

        [StringLength(32)]
        public string CreateUserName { get; set; }

        public DateTime CreateDateTime { get; set; }

        [StringLength(32)]
        public string CommitUserName { get; set; }

        public DateTime? CommitDateTime { get; set; }

        public virtual ICollection<Off_Store> Off_Store { get; set; }
    }
}
