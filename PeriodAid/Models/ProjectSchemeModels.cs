namespace PeriodAid.Models
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ProjectSchemeModels : DbContext
    {
        //您的上下文已配置为从您的应用程序的配置文件(App.config 或 Web.config)
        //“PeriodAid.Models.ProjectSchemeMdels”数据库。
        // 
        //如果您想要针对其他数据库和/或数据库提供程序，请在应用程序配置文件中修改“ProjectSchemeMdels”
        //连接字符串。
        public ProjectSchemeModels()
            : base("name=PROJECTConnection")
        {
        }

        //为您要在模型中包含的每种实体类型都添加 DbSet。有关配置和使用 Code First  模型
        //的详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=390109。

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        public virtual DbSet<Department> Department { get; set; }
        public virtual DbSet<Employee> Employee { get; set; }
        public virtual DbSet<Subject> Subject { get; set; }
        public virtual DbSet<ProcedureTemplate> ProcedureTemplate { get; set; }
        public virtual DbSet<Procedure> Procedure { get; set; }
        public virtual DbSet<Assignment> Assignment { get; set; }
        public virtual DbSet<SubTask> SubTask { get; set; }
        public virtual DbSet<AssignmentComment> AssignmentComment { get; set; }
        public virtual DbSet<SubjectAttachment> SubjectAttachment { get; set; }
        public virtual DbSet<OperationLogs> OperationLogs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Department>().HasMany(m => m.Employee).WithOptional(m => m.Department).HasForeignKey(m => m.DepartmentId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Employee>().HasMany(e => e.SupervisorDepartment).WithOptional(e => e.Supervisor).HasForeignKey(e => e.SupervisorId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Employee>().HasMany(m => m.Subject).WithRequired(m => m.Holder).HasForeignKey(e => e.HolderId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Employee>().HasMany(e => e.CollaborateAssignment).WithMany(e => e.Collaborator).Map(m => { m.MapLeftKey("CollaboratorId"); m.MapRightKey("AssignmentId"); m.ToTable("Collaborator_Assignment"); });
            modelBuilder.Entity<Employee>().HasMany(e => e.HolderAssignment).WithRequired(e => e.Holder).HasForeignKey(e => e.HolderId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Employee>().HasMany(e => e.ExecuteTask).WithRequired(e => e.Executor).HasForeignKey(e => e.ExecutorId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Employee>().HasMany(e => e.AssignmentComment).WithRequired(e => e.Composer).HasForeignKey(e => e.ComposerId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Employee>().HasMany(e => e.UploadedAttachment).WithRequired(e => e.Uploader).HasForeignKey(e => e.UploaderId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Employee>().HasMany(e => e.OperationLogs).WithRequired(e => e.Employee).HasForeignKey(e => e.UserId).WillCascadeOnDelete(false);

            modelBuilder.Entity<ProcedureTemplate>().HasMany(m => m.Subject).WithRequired(m => m.ProcedureTemplate).HasForeignKey(e => e.TemplateId).WillCascadeOnDelete(false);
            modelBuilder.Entity<ProcedureTemplate>().HasMany(m => m.Procedure).WithRequired(e => e.ProcedureTemplate).HasForeignKey(e => e.TemplateId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Procedure>().HasMany(e => e.Assignment).WithRequired(e => e.Procedure).HasForeignKey(e => e.ProcedureId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Assignment>().HasMany(e => e.SubTask).WithRequired(e => e.Assignment).HasForeignKey(e => e.AssignmentId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Assignment>().HasMany(e => e.AssignmentComment).WithRequired(e => e.Assignment).HasForeignKey(e => e.AssignmentId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Subject>().HasMany(e => e.SubjectAttachment).WithRequired(e => e.Subject).HasForeignKey(e => e.SubjectId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Subject>().HasMany(e => e.Assignment).WithRequired(e => e.Subject).HasForeignKey(e => e.SubjectId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Subject>().HasMany(e => e.OperationLogs).WithRequired(e => e.Subject).HasForeignKey(e => e.SubjectId).WillCascadeOnDelete(false);

        }
    }

    [Table("Department")]
    public partial class Department // 部门表
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string DepartmentName { get; set; } // 部门名称

        public int? SupervisorId { get; set; } // 主管ID

        public int Status { get; set; } // 状态 -1：已删除 1：正常状态

        public virtual Employee Supervisor { get; set; } // 主管实体

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employee> Employee { get; set; } // 所属员工
    }

    [Table("Employee")]
    public partial class Employee // 职员表
    {
        public int Id { get; set; }

        public int? DepartmentId { get; set; } // 部门ID

        public virtual Department Department { get; set; } // 部门实体 可以为空（新注册员工可以未分配）

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Department> SupervisorDepartment { get; set; } // 所有主管的部门

        [Required, StringLength(32)]
        public string NickName { get; set; } // 真实姓名

        [Required, StringLength(32)]
        public string UserName { get; set; } // 用户名 和会员系统连接

        public int Status { get; set; } // 用户状态 -1：离职 1：正常

        public int Type { get; set; } // 用户类型 1：普通用户（仅自己的项目及内容） 2：主管（所有项目信息）

        public DateTime SignupDate { get; set; } //注册日期

        public DateTime LastLoginDate { get; set; } // 上次登陆时间

        [StringLength(128)]
        public string ImgUrl { get; set; } // 头像地址

        [StringLength(16), RegularExpression("1[3|5|7|8|][0-9]{9}", ErrorMessage = "手机号码格式错误")]
        public string Mobile { get; set; } // 手机号码

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Subject> Subject { get; set; } // 所有拥有的项目

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assignment> CollaborateAssignment { get; set; } // 合作的项目

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assignment> HolderAssignment { get; set; } // 负责的任务

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SubTask> ExecuteTask { get; set; } // 负责的子任务

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AssignmentComment> AssignmentComment { get; set; } // 评论的内容

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SubjectAttachment> UploadedAttachment { get; set; } // 上传的附件

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OperationLogs> OperationLogs { get; set; }

    }

    [Table("Subject")]
    public partial class Subject // 项目表
    {
        public int Id { get; set; }

        [StringLength(32), Required]
        public string SubjectTitle { get; set; }

        public int HolderId { get; set; } // 项目所有人ID

        public virtual Employee Holder { get; set; } // 项目所有人实体 不可以为空(默认为创建人 可转移)

        public int Status { get; set; } // 状态 0：已归档 1：进行中

        public int TemplateId { get; set; } // 过程模板ID

        public DateTime CreateTime { get; set; } // 创建时间

        [StringLength(256)]
        public string Remarks { get; set; } // 备注信息

        [StringLength(256)]
        public string PicUrl { get; set; } // 项目封面图片

        public virtual ProcedureTemplate ProcedureTemplate { get; set; } // 过程模板实体

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SubjectAttachment> SubjectAttachment { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assignment> Assignment { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<OperationLogs> OperationLogs { get; set; }
    }

    [Table("ProcedureTemplate")]
    public partial class ProcedureTemplate // 过程模板
    {
        public int Id { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Subject> Subject { get; set; }

        [Required, StringLength(32)]
        public string Title { get; set; } // 模板标题

        public bool Default { get; set; } // 默认模板 默认值为False

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Procedure> Procedure { get; set; }
    }

    [Table("Procedure")]
    public partial class Procedure // 过程组
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string ProcedureTitle { get; set; } // 过程组标题

        public int TemplateId { get; set; } // 过程模板ID

        public virtual ProcedureTemplate ProcedureTemplate { get; set; } // 过程模板实体

        public int Sort { get; set; } // 排序 由低到高

        public int Status { get; set; } // 状态 -1：已删除（未知） 1：正常 

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Assignment> Assignment { get; set; }
    }

    [Table("Assignment")]
    public partial class Assignment // 任务表
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string AssignmentTitle { get; set; }

        public int Status { get; set; } // 状态 -1：删除 0：归档 1：未完成 2：已完成

        public int Priority { get; set; } // 优先级 1-5

        public DateTime CreateTime { get; set; } // 添加日期 

        public DateTime? Deadline { get; set; } // 截至时间(可为空)

        public DateTime? RemindDate { get; set; } // 提醒时间(可为空)

        public DateTime? CompleteDate { get; set; } // 完成时间(可为空)

        [StringLength(512)]
        public string Remarks { get; set; } // 备注

        public bool Repeat { get; set; } // 是否重复

        public int ProcedureId { get; set; } // 过程组ID

        public virtual Procedure Procedure { get; set; } // 过程组实例

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Employee> Collaborator { get; set; } // 合作者实例

        public int HolderId { get; set; } // 任务负责人ID (可以更换)

        public virtual Employee Holder { get; set; } // 负责人实例

        public int SubjectId { get; set; } // 项目ID

        public virtual Subject Subject { get; set; } // 项目实例

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SubTask> SubTask { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AssignmentComment> AssignmentComment { get; set; }
    }

    [Table("SubTask")]
    public partial class SubTask // 子任务表 
    {
        public int Id { get; set; }

        public int Status { get; set; } // 状态 -1：删除 0：归档 1：未完成 2：已完成

        [Required, StringLength(32)]
        public string TaskTitle { get; set; } // 子任务标题

        public int ExecutorId { get; set; } // 执行人ID

        public virtual Employee Executor { get; set; } // 执行人实例

        public int AssignmentId { get; set; } // 所属主任务ID

        public virtual Assignment Assignment { get; set; } // 所属主任务实例

        public DateTime CreateTime { get; set; } // 添加日期 

        public DateTime? Deadline { get; set; } // 截至时间(可为空)

        public DateTime? RemindDate { get; set; } // 提醒时间(可为空)

        public DateTime? CompleteDate { get; set; } // 完成时间(可为空)

        [StringLength(256)]
        public string Remarks { get; set; }
    }

    [Table("AssignmentComment")]
    public partial class AssignmentComment // 任务评论信息表
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; } // 任务ID

        public virtual Assignment Assignment { get; set; } // 任务实例

        public int ComposerId { get; set; } // 撰写人ID

        public virtual Employee Composer { get; set; } // 撰写人实例

        [Required, StringLength(256)]
        public string CommentContent { get; set; } // 评论内容

        public int Status { get; set; } // 状态 -1：已删除 1：正常

        public DateTime CreateTime { get; set; } // 创建时间
    }

    [Table("SubjectAttachment")]
    public partial class SubjectAttachment // 项目附件（第一阶段不制作）
    {
        public int Id { get; set; }

        [Required, StringLength(32)]
        public string AttachmentTitle { get; set; } //附件标题

        [Required, StringLength(512)]
        public string AttachmentSource { get; set; } // 附件地址

        [Required, StringLength(64)]
        public string ContentType { get; set; } // 文件类型

        public int AttachmentType { get; set; } // 文件分类 0：文档 1：图片 2：视频

        public int AttachmentSize { get; set; } // 文件大小

        public int UploaderId { get; set; } // 上传者ID

        public virtual Employee Uploader { get; set; } // 上传者实例

        public DateTime UploadTime { get; set; } // 上传时间

        public int SubjectId { get; set; } // 项目ID

        public virtual Subject Subject { get; set; } // 项目实例
    }

    [Table("OperationLogs")]
    public partial class OperationLogs
    {
        public int Id { get; set; }

        public int SubjectId { get; set; } // 项目ID

        public int UserId { get; set; } // 用户ID
        
        public int LogCode { get; set; } // 日志代码

        [Required, StringLength(256)]
        public string LogContent { get; set; } // 日志内容

        public DateTime LogTime { get; set; } // 日志记录时间

        public virtual Subject Subject { get; set; } // 项目实例

        public virtual Employee Employee { get; set; } // 员工实例
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
    public static class EmployeeStatus
    {
        /// <summary>
        /// 离职
        /// </summary>
        public static int DEVOICE = -1;
        /// <summary>
        ///  普通
        /// </summary>
        public static int NORMAL = 0;
        /// <summary>
        /// 未知
        /// </summary>
        public static int UNKNOW = -2;
        /// <summary>
        /// 管理员
        /// </summary>
        public static int ADMINISTRATOR = 1;
    }

    public static class DepartmentStatus
    {
        /// <summary>
        /// 已移除
        /// </summary>
        public static int REMOVED = -1;
        /// <summary>
        ///  正常
        /// </summary>
        public static int NORMAL = 1;
    }

    public static class CommentStatus
    {
        /// <summary>
        /// 已移除
        /// </summary>
        public static int REMOVED = -1;
        /// <summary>
        ///  正常
        /// </summary>
        public static int NORMAL = 1;
    }

    public static class SubjectStatus
    {
        /// <summary>
        /// 已删除
        /// </summary>
        public static int DELETED = -1;
        /// <summary>
        /// 已归档
        /// </summary>
        public static int ARCHIVED = 0;
        /// <summary>
        ///  进行中
        /// </summary>
        public static int ACTIVE = 1;
    }

    public static class ProcedureStatus
    {
        /// <summary>
        /// 已移除
        /// </summary>
        public static int REMOVED = -1;
        /// <summary>
        ///  正常
        /// </summary>
        public static int NORMAL = 1;
    }

    public static class AssignmentStatus
    {
        /// <summary>
        /// 已删除
        /// </summary>
        public static int DELETED = -1;
        /// <summary>
        ///  已归档
        /// </summary>
        public static int ARCHIVED = 0;
        /// <summary>
        ///  未完成
        /// </summary>
        public static int UNFINISHED = 1;
        /// <summary>
        ///  已完成
        /// </summary>
        public static int FINISHED = 2;       
    }

    public static class LogCode
    {
        /// <summary>
        /// 创建项目
        /// </summary>
        public static int CREATESUBJECT = 101;
        /// <summary>
        /// 修改项目
        /// </summary>
        public static int EDITSUBJECT = 102;
        /// <summary>
        /// 归档项目
        /// </summary>
        public static int ARCHIVESUBJECT = 103;
        /// <summary>
        /// 删除项目
        /// </summary>
        public static int DELETESUBJECT = 104;

        /// <summary>
        /// 恢复项目
        /// </summary>
        public static int RESETSUBJECT = 201;

        /// <summary>
        /// 添加任务
        /// </summary>
        public static int CREATETASK = 105;
        /// <summary>
        /// 修改任务
        /// </summary>
        public static int EDITTASK = 106;
        /// <summary>
        /// 归档任务
        /// </summary>
        public static int ARCHIVETASK = 107;
        /// <summary>
        /// 删除任务
        /// </summary>
        public static int DELETETASK = 108;

        /// <summary>
        /// 添加子任务
        /// </summary>
        public static int CREATESUBTASK = 109;
        /// <summary>
        /// 修改子任务
        /// </summary>
        public static int EDITSUBTASK = 110;
        /// <summary>
        /// 归档子任务
        /// </summary>
        public static int ARCHIVESUBTASK = 111;
        /// <summary>
        /// 删除子任务
        /// </summary>
        public static int DELETESUBTASK = 112;

        /// <summary>
        ///更改个人信息
        /// </summary>
        public static int EDITPERSONALINFO = 113;

    }


    public class SubjectFileArea
    {
        public int SubjectId { get; set; }
        public ICollection<SubjectFolder> SubjectFolder { get; set; }
    }


    public class SubjectFolder
    {
        public int FolderCode { get; set; }
        public string FolderKey { get; set; }
        public string FolderName { get; set; }
        public int FileNum { get; set; }
    }
    public static class ContentTypeCode
    {

        /// <summary>
        ///未知类型
        /// </summary>
        public static class UNKNOWN
        {
            public static int Code = 401;
            public static string Key = "unknown";
            public static string TypeName = "未知";
            public static class PDF
            {
                public static int Code = 4011;
                public static string Key = "pdf";
            }
        }

        /// <summary>
        ///图片
        /// </summary>
        public static class IMAGE
        {
            public static int Code = 100;
            public static string Key = "image";
            public static string TypeName = "图片";
        }

        /// <summary>
        ///视频
        /// </summary>
        public static class VIDEO
        {
            public static int Code = 101;
            public static string Key = "video";
            public static string TypeName = "视频";
        }

        /// <summary>
        ///音频
        /// </summary>
        public static class AUDIO
        {
            public static int Code = 102;
            public static string Key = "audio";
            public static string TypeName = "音频";
        }


        /// <summary>
        ///文本
        /// </summary>
        public static class TEXT
        {
            public static int Code = 103;
            public static string TypeName = "文本";
            public static string Key = "text";
            public static class EXCEL
            {
                public static int Code = 1031;
                public static string Key = "sheet";
            }
            public static class PPT
            {
                public static int Code = 1032;
                public static string Key = "presentation";
            }
            public static class WORD
            {
                public static int Code = 1033;
                public static string Key = "document";
            }
        }
    }



}