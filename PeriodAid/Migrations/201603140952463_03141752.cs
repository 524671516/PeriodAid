namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _03141752 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Off_Manager_Task", "NickName", c => c.String(maxLength: 32));
            AddColumn("dbo.Off_Manager_Task", "Photo", c => c.String(maxLength: 512));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Off_Manager_Task", "Photo");
            DropColumn("dbo.Off_Manager_Task", "NickName");
        }
    }
}
