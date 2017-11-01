namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _321 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SS_TrafficData", "Update", c => c.DateTime(nullable: false));
            DropColumn("dbo.SS_TrafficData", "DateFlow_Date");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SS_TrafficData", "DateFlow_Date", c => c.DateTime(nullable: false));
            DropColumn("dbo.SS_TrafficData", "Update");
        }
    }
}
