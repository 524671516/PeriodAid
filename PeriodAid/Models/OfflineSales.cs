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

        public virtual DbSet<Event> Event { get; set; }
        public virtual DbSet<Product> Product { get; set; }
        public virtual DbSet<Sales_Data> Sales_Data { get; set; }
        public virtual DbSet<Sales_Details> Sales_Details { get; set; }
        public virtual DbSet<Seller> Seller { get; set; }
        public virtual DbSet<Store> Store { get; set; }
        public virtual DbSet<Store_System> Store_System { get; set; }
        public virtual DbSet<Store_Sales_Month> Store_Sales_Month { get; set; }
        public virtual DbSet<Store_Sales_Month_Details> Store_Sales_Month_Details { get; set; }


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

        public virtual DbSet<Off_Costs> Off_Costs { get; set; }
        public virtual DbSet<Off_SalesInfo_Daily> Off_SalesInfo_Daily { get; set; }
        public virtual DbSet<Off_SalesInfo_Month> Off_SalesInfo_Month { get; set; }
        public virtual DbSet<Off_Seller> Off_Seller { get; set; }
        public virtual DbSet<Off_Store> Off_Store { get; set; }
        public virtual DbSet<Off_StoreSystem_Costs> Off_StoreSystem_Costs { get; set; }
        public virtual DbSet<Off_Expenses> Off_Expenses { get; set; }
        public virtual DbSet<Off_Expenses_Details> Off_Expenses_Details { get; set; }
        public virtual DbSet<Off_Expenses_Payment> Off_Expenses_Payment { get; set; }
        public virtual DbSet<Off_Membership_Bind> Off_Membership_Bind { get; set; }
        public virtual DbSet<Off_Checkin_Schedule> Off_Checkin_Schedule { get; set; }
        public virtual DbSet<Off_Event> Off_Event { get; set; }
        public virtual DbSet<Off_Checkin> Off_Checkin { get; set; }
        public virtual DbSet<Off_StoreManager> Off_StoreManager { get; set; }
        public virtual DbSet<Off_Manager_Task> Off_Manager_Task { get; set; }
        public virtual DbSet<Off_Manager_CheckIn> Off_Manager_CheckIn { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .Property(e => e.Event_Summary)
                .IsFixedLength();

            modelBuilder.Entity<Store_Sales_Month_Details>()
                .Property(e => e.Sales_Amount)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Product>()
                .Property(e => e.Price)
                .HasPrecision(19, 4);

            modelBuilder.Entity<Product>()
                .HasMany(e => e.Sales_Details)
                .WithRequired(e => e.Product)
                .HasForeignKey(e => e.Product_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .HasMany(e => e.Store_Sales_Month_Details)
                .WithRequired(e => e.Product)
                .HasForeignKey(e => e.Product_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Sales_Data>()
                .HasMany(e => e.Sales_Details)
                .WithRequired(e => e.Sales_Data)
                .HasForeignKey(e => e.Sales_Data_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Store_Sales_Month>()
                .HasMany(e => e.Store_Sales_Month_Details)
                .WithRequired(e => e.Store_Sales_Month)
                .HasForeignKey(e => e.Store_Sales_Month_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Seller>()
                .HasMany(e => e.Sales_Data)
                .WithRequired(e => e.Seller)
                .HasForeignKey(e => e.Seller_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Store>()
                .HasMany(e => e.Event)
                .WithRequired(e => e.Store)
                .HasForeignKey(e => e.Store_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Store>()
                .HasMany(e => e.Sales_Data)
                .WithRequired(e => e.Store)
                .HasForeignKey(e => e.Store_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Store>()
                .HasMany(e => e.Store_Sales_Month)
                .WithRequired(e => e.Store)
                .HasForeignKey(e => e.Store_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Store_System>()
                .HasMany(e => e.Store)
                .WithRequired(e => e.Store_System)
                .HasForeignKey(e => e.Store_System_Id)
                .WillCascadeOnDelete(false);

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

            modelBuilder.Entity<Off_Checkin_Schedule>()
                .HasMany(e => e.Off_Checkin)
                .WithRequired(e => e.Off_Checkin_Schedule)
                .HasForeignKey(e => e.Off_Schedule_Id)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Store>()
                .HasMany(e => e.Off_Costs)
                .WithRequired(e => e.Off_Store)
                .HasForeignKey(e => e.StoreId)
                .WillCascadeOnDelete(false);

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

            modelBuilder.Entity<Off_Expenses>()
                .HasMany(e => e.Off_Expenses_Details)
                .WithRequired(e => e.Off_Expenses)
                .HasForeignKey(e => e.ExpensesId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Expenses>()
                .HasMany(e=>e.Off_Expenses_Payment)
                .WithRequired(e => e.Off_Expenses)
                .HasForeignKey(e => e.ExpensesId)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Off_Manager_Task>()
                .HasMany(e => e.Off_Manager_CheckIn)
                .WithRequired(e => e.Off_Manager_Task)
                .HasForeignKey(e => e.Manager_EventId)
                .WillCascadeOnDelete(true);
        }
    }

    #region 旧表
    [Table("Event")]
    public partial class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Event_Name { get; set; }

        [Required]
        public string Event_Date { get; set; }

        public int Store_Id { get; set; }

        [StringLength(512)]
        public string Event_Details { get; set; }

        [StringLength(10)]
        public string Event_Summary { get; set; }

        public virtual Store Store { get; set; }
    }

    [Table("Product")]
    public partial class Product
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Product()
        {
            Sales_Details = new HashSet<Sales_Details>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Product_Name { get; set; }

        [StringLength(16)]
        public string Product_Code { get; set; }

        [StringLength(16)]
        public string Product_Spec { get; set; }

        [Column(TypeName = "money")]
        public decimal? Price { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sales_Details> Sales_Details { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Store_Sales_Month_Details> Store_Sales_Month_Details { get; set; }
    }

    public partial class Sales_Data
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Sales_Data()
        {
            Sales_Details = new HashSet<Sales_Details>();
        }

        public int Id { get; set; }

        public DateTime Sales_Date { get; set; }

        public int Store_Id { get; set; }

        public int? Trial_Count { get; set; }

        public int Seller_Id { get; set; }

        public int? Max_Sale { get; set; }

        public int? Feedback { get; set; }

        [StringLength(32)]
        public string Comsumption_Age { get; set; }

        [StringLength(256)]
        public string Summary { get; set; }

        public bool Event { get; set; }

        [StringLength(32)]
        public string Event_Type { get; set; }

        public virtual Seller Seller { get; set; }

        public virtual Store Store { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sales_Details> Sales_Details { get; set; }
    }
    public partial class Store_Sales_Month
    {
        public int Id { get; set; }

        public int Sales_Year { get; set; }

        public int Sales_Month { get; set; }

        public int Store_Id { get; set; }

        public virtual Store Store { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Store_Sales_Month_Details> Store_Sales_Month_Details { get; set; }
    }

    public partial class Store_Sales_Month_Details
    {

        public int Id { get; set; }

        public int Store_Sales_Month_Id { get; set; }

        public int Product_Id { get; set; }

        public int Sales_Num { get; set; }

        [Column(TypeName = "money")]
        public decimal Sales_Amount { get; set; }

        public virtual Product Product { get; set; }

        public virtual Store_Sales_Month Store_Sales_Month { get; set; }
    }

    public partial class Sales_Details
    {
        public int Id { get; set; }

        public int Sales_Data_Id { get; set; }

        public int Product_Id { get; set; }

        public int Report_Num { get; set; }

        public int? Checkout_Num { get; set; }

        public virtual Product Product { get; set; }

        public virtual Sales_Data Sales_Data { get; set; }
    }

    [Table("Seller")]
    public partial class Seller
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Seller()
        {
            Sales_Data = new HashSet<Sales_Data>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(16)]
        public string Name { get; set; }

        [StringLength(32)]
        public string Contact { get; set; }

        public bool? Sex { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sales_Data> Sales_Data { get; set; }
    }

    [Table("Store")]
    public partial class Store
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Store()
        {
            Event = new HashSet<Event>();
            Sales_Data = new HashSet<Sales_Data>();
        }

        public int Id { get; set; }

        [Required]
        public int Store_System_Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Store_Name { get; set; }

        [StringLength(128)]
        public string Address { get; set; }

        [StringLength(32)]
        public string Contact { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Event> Event { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Sales_Data> Sales_Data { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Store_Sales_Month> Store_Sales_Month { get; set; }

        public virtual Store_System Store_System { get; set; }
    }

    public partial class Store_System
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Store_System()
        {
            Store = new HashSet<Store>();
        }

        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string System_Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Store> Store { get; set; }
    }
    #endregion

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

    public partial class Off_StoreSystem_Costs
    {
        public int Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ApplicationDate { get; set; }
        
        [StringLength(50)]
        public String StoreSystem { get; set; }

        [StringLength(50)]
        public String Distributor { get; set; }

        public decimal TotalFee { get; set; }

        public decimal? Cash { get; set; }

        public decimal? MortgageGoods { get; set; }

        [StringLength(50)]
        public string Warrant { get; set; }

        public bool Completed { get; set; }

        public bool Checked { get; set; }

        public bool Canceled { get; set; }

        public DateTime? UploadTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

    }

    public partial class Off_Costs
    {
        public int Id { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ApplicationDate { get; set; }

        public int StoreId { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? StartDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndDate { get; set; }

        public bool Event_HB { get; set; }

        public bool Event_DT { get; set; }

        public bool Event_TG { get; set; }

        public bool Event_DJ { get; set; }

        public bool Event_DL { get; set; }

        public bool Event_TJ { get; set; }

        public bool Event_SY { get; set; }

        public bool Event_Other { get; set; }

        public decimal? TotalFee { get; set; }

        public decimal? Cash { get; set; }

        public decimal? MortgageGoods { get; set; }

        [StringLength(50)]
        public string Warrant { get; set; }

        public bool Checked { get; set; }

        public bool Completed { get; set; }

        public bool Canceled { get; set; }

        public virtual Off_Store Off_Store { get; set; }

        public DateTime? UploadTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }
    }

    public partial class Off_Store
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Off_Store()
        {
            Off_Costs = new HashSet<Off_Costs>();
            Off_SalesInfo_Daily = new HashSet<Off_SalesInfo_Daily>();
            Off_SalesInfo_Month = new HashSet<Off_SalesInfo_Month>();
        }

        public int Id { get; set; }

        [StringLength(50)]
        public string StoreSystem { get; set; }

        [Required]
        [StringLength(255)]
        public string StoreName { get; set; }

        [StringLength(20)]
        public string Region { get; set; }

        [StringLength(50)]
        public string Distributor { get; set; }

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
        public virtual ICollection<Off_Costs> Off_Costs { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SalesInfo_Daily> Off_SalesInfo_Daily { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SalesInfo_Month> Off_SalesInfo_Month { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Seller> Off_Seller { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin_Schedule> Off_Checkin_Schedule { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Event> Off_Event { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_StoreManager> Off_StoreManager { get; set; }
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

        public DateTime? UploadTime { get; set; }

        [StringLength(255)]
        public string UploadUser { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_SalesInfo_Daily> Off_SalesInfo_Daily { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Membership_Bind> Off_Membership_Bind { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Checkin> Off_Checkin { get; set; }

        public virtual Off_Store Off_Store { get; set; }
        
    }

    public partial class Off_SalesInfo_Daily
    {
        public int Id { get; set; }

        public int StoreId { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }

        public int? Item_Brown { get; set; }

        public int? Item_Black { get; set; }

        public int? Item_Lemon { get; set; }

        public int? Item_Honey { get; set; }

        public int? Item_Dates { get; set; }

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

        public int? Off_Seller_Id { get; set; }
        
        public DateTime ApplicationDate { get; set; }

        public virtual Off_Seller Off_Seller { get; set; }
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
    }
    public partial class Off_Checkin
    {
        public int Id { get; set; }

        public int Off_Schedule_Id { get; set; }

        public int Status { get; set; }

        public int Off_Seller_Id { get; set; }

        public DateTime? CheckinTime { get; set; }

        [StringLength(64)]
        public string CheckinLocation { get; set; }

        [StringLength(64)]
        public string CheckinPhoto { get; set; }

        public DateTime? CheckoutTime { get; set; }

        [StringLength(64)]
        public string CheckoutLocation { get; set; }

        public int? Rep_Brown { get; set; }

        public int? Rep_Black { get; set; }

        public int? Rep_Honey { get; set; }

        public int? Rep_Lemon { get; set; }

        public int? Rep_Dates { get; set; }

        public int? Rep_Other { get; set; }

        [StringLength(512, ErrorMessage ="不超过512个字符")]
        public string Rep_Image { get; set; }

        [StringLength(512,ErrorMessage ="不超过512个字符")]
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

    }

    public partial class Off_Event
    {
        public int Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime FinishDate { get; set; }

        public int Priority_Level { get; set; }

        public decimal? Default_Salary { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Off_Store> Off_Store { get; set; }
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
    }

    public partial class Off_Manager_Task
    {
        public int Id { get; set; }

        public int Status { get; set; }
        [StringLength(32)]
        public string UserName{get;set;}

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime TaskDate { get; set; }
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

        [StringLength(64)]
        public string Photo { get; set; }

        [StringLength(64)]
        public string Remark { get; set; }

        public DateTime CheckIn_Time { get; set; }

        public virtual Off_Manager_Task Off_Manager_Task { get; set; }
    }
}
