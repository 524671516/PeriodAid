namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _321 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SS_TrafficData", "Convert_Ratio");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SS_TrafficData", "Convert_Ratio", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
    }
}
