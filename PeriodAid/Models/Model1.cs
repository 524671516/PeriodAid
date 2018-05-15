namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Model1 : DbContext
    {
        public Model1()
            : base("name=Model1")
        {
        }

        public virtual DbSet<VM_Comment> VM_Comment { get; set; }
        public virtual DbSet<VM_Company> VM_Company { get; set; }
        public virtual DbSet<VM_Contact> VM_Contact { get; set; }
        public virtual DbSet<VM_ContentConfig> VM_ContentConfig { get; set; }
        public virtual DbSet<VM_Department> VM_Department { get; set; }
        public virtual DbSet<VM_Employee> VM_Employee { get; set; }
        public virtual DbSet<VM_VisitRecord> VM_VisitRecord { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VM_Company>()
                .HasMany(e => e.VM_Contact)
                .WithRequired(e => e.VM_Company)
                .HasForeignKey(e => e.Company_Id);

            modelBuilder.Entity<VM_Company>()
                .HasMany(e => e.VM_VisitRecord)
                .WithRequired(e => e.VM_Company)
                .HasForeignKey(e => e.Company_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<VM_Department>()
                .HasMany(e => e.VM_Employee)
                .WithRequired(e => e.VM_Department)
                .HasForeignKey(e => e.Department_Id);

            modelBuilder.Entity<VM_Employee>()
                .HasMany(e => e.VM_Comment)
                .WithRequired(e => e.VM_Employee)
                .HasForeignKey(e => e.Employee_Id);

            modelBuilder.Entity<VM_Employee>()
                .HasMany(e => e.VM_Company)
                .WithRequired(e => e.VM_Employee)
                .HasForeignKey(e => e.Employee_Id);

            modelBuilder.Entity<VM_Employee>()
                .HasMany(e => e.VM_VisitRecord)
                .WithRequired(e => e.VM_Employee)
                .HasForeignKey(e => e.Employee_Id)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<VM_Employee>()
                .HasMany(e => e.VM_VisitRecord1)
                .WithMany(e => e.VM_Employee1)
                .Map(m => m.ToTable("AttendEmployee_VisitRecord").MapLeftKey("EmployeeId").MapRightKey("VisitRecordId"));

            modelBuilder.Entity<VM_VisitRecord>()
                .HasMany(e => e.VM_Comment)
                .WithRequired(e => e.VM_VisitRecord)
                .HasForeignKey(e => e.VisitRecord_Id);
        }
    }
}
