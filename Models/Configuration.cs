namespace PeriodAid.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations;

    public partial class Configuration : DbContext
    {
        public Configuration()
            : base("name=ConfigurationConnection")
        {
        }

        public virtual DbSet<WeChatConfigs> WeChatConfigs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }

    public partial class WeChatConfigs
    {
        public int Id { get; set; }

        [StringLength(200)]
        public string Key { get; set; }

        [StringLength(256)]
        public string Value { get; set; }

        public DateTime LastModify { get; set; }
    }
}
