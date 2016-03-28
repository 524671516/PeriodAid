namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _03281009 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Off_Manager_Request", "ManagerUserName", c => c.String(maxLength: 64));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Off_Manager_Request", "ManagerUserName", c => c.String(maxLength: 512));
        }
    }
}
