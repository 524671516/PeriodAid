namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _03221147 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.taskstatus", "type", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.taskstatus", "type");
        }
    }
}
