using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.Collections.Generic;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    public class ThreeLevelAddressModel:DbContext
    {
        public ThreeLevelAddressModel()
            : base("name=ThreeLevelAddressConnection")
        {
        }
        public virtual DbSet<Province> Province { get; set; }
        public virtual DbSet<City> City { get; set; }
        public virtual DbSet<Area> Area { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Province>().HasMany(m => m.City).WithRequired(m => m.Province).HasForeignKey(m => m.P_code).WillCascadeOnDelete(false);
            modelBuilder.Entity<City>().HasMany(m => m.Area).WithRequired(m => m.City).HasForeignKey(m => m.C_code).WillCascadeOnDelete(false);

        }

    }
    [Table("Province")]
    public partial class Province
    {
        [Key]
        public int Province_code { get; set; }

        [StringLength(64)]
        public string Province_name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<City> City { get; set; }
    }

    [Table("City")]
    public partial class City
    {
        [Key]
        public int City_code { get; set; }

        [StringLength(64)]
        public string City_name { get; set; }

        public int P_code { get; set; }

        public virtual Province Province { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Area> Area { get; set; }
    }

    [Table("Area")]
    public partial class Area
    {
        [Key]
        public int Area_code { get; set; }

        [StringLength(64)]
        public string Area_name { get; set; }

        public int C_code { get; set; }

        public virtual City City { get; set; }
    }
}