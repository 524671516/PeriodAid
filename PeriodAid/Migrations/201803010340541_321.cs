namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _321 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.CRM_Contact", "province");
            DropColumn("dbo.CRM_Contact", "city");
            DropColumn("dbo.CRM_Contact", "district");
            DropColumn("dbo.CRM_Contact", "zip");
            DropColumn("dbo.CRM_Customer", "province");
            DropColumn("dbo.CRM_Customer", "city");
            DropColumn("dbo.CRM_Customer", "district");
            DropColumn("dbo.CRM_Customer", "zip");
        }
        
        public override void Down()
        {
            AddColumn("dbo.CRM_Customer", "zip", c => c.String());
            AddColumn("dbo.CRM_Customer", "district", c => c.String());
            AddColumn("dbo.CRM_Customer", "city", c => c.String());
            AddColumn("dbo.CRM_Customer", "province", c => c.String());
            AddColumn("dbo.CRM_Contact", "zip", c => c.String());
            AddColumn("dbo.CRM_Contact", "district", c => c.String());
            AddColumn("dbo.CRM_Contact", "city", c => c.String());
            AddColumn("dbo.CRM_Contact", "province", c => c.String());
        }
    }
}
