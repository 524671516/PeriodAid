namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _555 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Assignment", "Target");
            DropColumn("dbo.SubTask", "Target");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SubTask", "Target", c => c.String(maxLength: 256));
            AddColumn("dbo.Assignment", "Target", c => c.String(maxLength: 512));
        }
    }
}
