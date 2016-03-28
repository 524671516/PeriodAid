namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _03281106 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Off_Manager_Request", "RequestContent", c => c.String(maxLength: 1024));
            DropColumn("dbo.Off_Manager_Request", "RuquestContent");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Off_Manager_Request", "RuquestContent", c => c.String(maxLength: 1024));
            DropColumn("dbo.Off_Manager_Request", "RequestContent");
        }
    }
}
