﻿namespace PeriodAid.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public class VisitManagementModels : DbContext
    {
        //您的上下文已配置为从您的应用程序的配置文件(App.config 或 Web.config)
        //使用“QualityControlModels”连接字符串。默认情况下，此连接字符串针对您的 LocalDb 实例上的
        //“SqzEvent.Models.QualityControlModels”数据库。
        // 
        //如果您想要针对其他数据库和/或数据库提供程序，请在应用程序配置文件中修改“QualityControlModels”
        //连接字符串。
        public VisitManagementModels()
            : base("name=VisitManagementConnection")
        {
        }

        //为您要在模型中包含的每种实体类型都添加 DbSet。有关配置和使用 Code First  模型
        //的详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=390109。

        // public virtual DbSet<MyEntity> MyEntities { get; set; }

        public virtual DbSet<VM_Company> VM_Company { get; set; }
        public virtual DbSet<VM_Department> VM_Department { get; set; }
        public virtual DbSet<VM_Employee> VM_Employee { get; set; }
        public virtual DbSet<VM_VisitRecord> VM_VisitRecord { get; set; }
        public virtual DbSet<VM_Comment> VM_Comment { get; set; }
        public virtual DbSet<VM_Contact> VM_Contact { get; set; }
        public virtual DbSet<VM_ContentConfig> VM_ContentConfig { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VM_Department>().HasMany(e => e.VM_Employee).WithRequired(e => e.VM_Department).HasForeignKey(e => e.Department_Id).WillCascadeOnDelete(true);
            modelBuilder.Entity<VM_Employee>().HasMany(e => e.VM_Company).WithRequired(e => e.VM_Employee).HasForeignKey(e => e.Employee_Id).WillCascadeOnDelete(true);
            modelBuilder.Entity<VM_VisitRecord>().HasMany(e => e.VM_Comment).WithRequired(e => e.VM_VisitRecord).HasForeignKey(e => e.VisitRecord_Id).WillCascadeOnDelete(true);
            modelBuilder.Entity<VM_Employee>().HasMany(e => e.VM_VisitRecord).WithRequired(e => e.VM_Employee).HasForeignKey(e => e.Employee_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<VM_Company>().HasMany(e => e.VM_Contact).WithRequired(e => e.VM_Company).HasForeignKey(e => e.Company_Id).WillCascadeOnDelete(true);
            modelBuilder.Entity<VM_Company>().HasMany(e => e.VM_VisitRecord).WithRequired(e => e.VM_Company).HasForeignKey(e => e.Company_Id).WillCascadeOnDelete(false);
            modelBuilder.Entity<VM_Employee>().HasMany(m => m.AttendVisit).WithMany(e => e.AttendEmployee).Map(m => { m.MapLeftKey("EmployeeId"); m.MapRightKey("VisitRecordId"); m.ToTable("AttendEmployee_VisitRecord"); });
           
           
        }
    }

    //公司表
    [Table("VM_Company")]
    public partial class VM_Company
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string Company_Name { get; set; }

        [StringLength(32)]
        public string Company_Type { get; set; }//客户类型（现代渠道，流通，餐饮，电商和APP）

        public int Company_Category { get; set; }//客户类别（0经销商，1渠道终端）

        [StringLength(32)]
        public string Company_Source { get; set; }//客户来源

        [StringLength(32)]
        public string Company_Source_Detail { get; set; }

        [StringLength(256)]
        public string Company_Address { get; set; }

        public int Employee_Count { get; set; }

        public int Store_Count { get; set; }//门店数量

        [StringLength(256)]
        public string Source_Name { get; set; }//渠道名称

        [StringLength(256)]
        public string Region { get; set; }//所属区域

        [StringLength(32)]
        public string Source_Type { get; set; }//渠道类型

        public int Entrance_Fee { get; set; }//进场费用（0有，-1无）

        public decimal EntranceFee_Count { get; set; }//进场费用数额

        public int? Entrance_Type { get; set; }//进场类型（0门店，1系统）

        public int Dedicated_Warehouse { get; set; }//专用仓库（0有，-1无）

        public int Warehouse_Area { get; set; }//仓库面积/平方米

        public decimal Sales_Amount { get; set; }//年销售额

        [StringLength(256)]
        public string Agent_FamousBrand { get; set; }//代理知名品牌

        public int Special_Source { get; set; }//特殊渠道（0有，-1无）

        [StringLength(256)]
        public string SpecialSource_Detail { get; set; }//特殊渠道详情

        [StringLength(256)]
        public string SpecialSource_OtherDetail { get; set; }//特殊渠道其他详情

        [StringLength(32)]
        public string Company_Phone { get; set; }

        public int Company_Status { get; set; }

        public DateTime? Create_Time { get; set; }

        public DateTime? Update_Time { get; set; }

        public int UpdateEmployee_Id { get; set; }

        public int Employee_Id { get; set; }

        public virtual VM_Employee VM_Employee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_VisitRecord> VM_VisitRecord { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_Contact> VM_Contact { get; set; }
    }

    //联系人表
    [Table("VM_Contact")]
    public partial class VM_Contact
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string Contact_Name { get; set; }

        [StringLength(32)]
        public string Contact_Type { get; set; }

        [StringLength(32)]
        public string Contact_Mobile { get; set; }

        [StringLength(32)]
        public string Contact_WeChat { get; set; }

        [StringLength(32)]
        public string Contact_Email { get; set; }

        public int Company_Id { get; set; }

        public virtual VM_Company VM_Company { get; set; }
    }

    //部门表
    [Table("VM_Department")]
    public partial class VM_Department
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string Department_Name { get; set; }

        [StringLength(32)]
        public string Department_Type { get; set; }

        public int Department_Status { get; set; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_Employee> VM_Employee { get; set; }
    }

    //用户表
    [Table("VM_Employee")]
    public partial class VM_Employee
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string Employee_Name { get; set; }

        public int Employee_Type { get; set; }

        public int Employee_Status { get; set; }

        [StringLength(32)]
        public string Employee_Mobile { get; set; }

        [StringLength(32)]
        public string Employee_Email { get; set; }

        public int Department_Id { get; set; }

        public virtual VM_Department VM_Department { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_Company> VM_Company { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_VisitRecord> AttendVisit { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_VisitRecord> VM_VisitRecord { get; set; }
    }

    //拜访记录表
    [Table("VM_VisitRecord")]
    public partial class VM_VisitRecord
    {
        public int Id { get; set; }

        public DateTime? Visit_Time { get; set; }//拜访时间

        public int Visit_Type { get; set; }//拜访方式（0面谈，1电话，2微信）

        public int Cooperation_Intention { get; set; }//合作意向（0有，-1无）

        [StringLength(256)]
        public string Intentional_Products { get; set; }//兴趣品相

        public decimal Intentional_Funds { get; set; }//操作资金

        public int Cooperation_Type { get; set; }//合作方式（0直营，1通过经销商）

        public string Company_Name { get; set; }//公司名称

        public string Contact_Mobile { get; set; }//联系人电话

        public DateTime? Reply_Time { get; set; }//回复日期

        public string NoIntention_Reason { get; set; }//无意向原因

        public int Smoothly { get; set; }//综合结果（0达到目的，-1未达到目的）

        public DateTime? ExpectedDelivery_Time { get; set; }//预计拿货时间

        public decimal ExpectedDelivery_Funds { get; set; }//预计拿货金额

        [StringLength(256)]
        public string Core_Problem { get; set; }//核心问题

        [StringLength(256)]
        public string Support { get; set; }//需要支持

        public int Reply_Status { get; set; }//需要我司回复（0是，-1否）

        [StringLength(256)]
        public string Reply_Detail { get; set; }//我司回复的详细

        public int CustomerReply_Status { get; set; }//需要对方回复（0是，-1否）

        public DateTime? CustomerReply_Time { get; set; }//对方回复的时间

        [StringLength(256)]
        public string CustomerReply_Detail { get; set; }//对方回复的详情

        public int NextVisit_Type { get; set; }//下次拜访方式（0实地，1电话）

        public DateTime? NextVisit_Time { get; set; }//下次拜访时间

        public string NextVisit_Detail { get; set; }//下次拜访事项

        public int status { get; set; }//拜访状态（0默认,1通过，-1驳回）

        [StringLength(256)]
        public string Veto_Detail { get; set; }

        [StringLength(128)]
        public string Veto_Cause { get; set; }

        public int Employee_Id { get; set; }//填表人

        public int Company_Id { get; set; }

        public DateTime? Create_Time { get; set; }

        public virtual VM_Company VM_Company { get; set; }

        public virtual VM_Employee VM_Employee { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_Comment> VM_Comment { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VM_Employee> AttendEmployee { get; set; }


    }

    //拜访评论表
    [Table("VM_Comment")]
    public partial class VM_Comment
    {
        public int Id { get; set; }

        [StringLength(256)]
        public string Comment_Detail { get; set; }

        public int VisitRecord_Id { get; set; }

        [StringLength(32)]
        public string Nick_Name { get; set; }

        [StringLength(64)]
        public string User_Name { get; set; }

        public DateTime? Comment_Time { get; set; }

        public int Comment_Type { get; set; }//0总评，1核心评论，2支持评论,3我方回复

        public int Comment_Status { get; set; }//0未读，1已读

        public virtual VM_VisitRecord VM_VisitRecord { get; set; }
    }


    //选项配置表
    [Table("VM_ContentConfig")]
    public partial class VM_ContentConfig
    {
        public int Id { get; set; }

        [StringLength(32)]
        public string Content_Name { get; set; }

        [StringLength(256)]
        public string Content_Detail { get; set; }

        public int Content_Type { get; set; }
    }

    public class VM_EmployeeViewModel
    {
        public string OpenId { get; set; }

        public string Name { get; set; }

        public string Mobile { get; set; }

        public string ImgUrl { get; set; }

        public string NickName { get; set; }
    }
}