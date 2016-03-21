namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20160321 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.taskstatus", "totalcount", c => c.Int(nullable: false));
            AddColumn("dbo.taskstatus", "currentcount", c => c.Int(nullable: false));
            AlterColumn("dbo.taskstatus", "status", c => c.Int(nullable: false));
            DropColumn("dbo.taskstatus", "count");
        }
        
        public override void Down()
        {
            AddColumn("dbo.taskstatus", "count", c => c.Int(nullable: false));
            AlterColumn("dbo.taskstatus", "status", c => c.Boolean(nullable: false));
            DropColumn("dbo.taskstatus", "currentcount");
            DropColumn("dbo.taskstatus", "totalcount");
        }
    }
}
