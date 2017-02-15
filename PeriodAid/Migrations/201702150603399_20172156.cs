namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20172156 : DbMigration
    {
        public override void Up()
        {
            
        }
        
        public override void Down()
        {
            AddColumn("dbo.Off_SalesEvent", "EventDetials", c => c.String(maxLength: 256));
            DropColumn("dbo.Off_SalesEvent", "CommitDateTime");
            DropColumn("dbo.Off_SalesEvent", "CommitUserName");
            DropColumn("dbo.Off_SalesEvent", "CreateDateTime");
            DropColumn("dbo.Off_SalesEvent", "CreateUserName");
            DropColumn("dbo.Off_SalesEvent", "EventDetails");
        }
    }
}
