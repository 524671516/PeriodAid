namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _05101449 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Off_Sales_Template", "RequiredStorage", c => c.Boolean(nullable: false));
            AddColumn("dbo.Off_Sales_Template", "RequiredAmount", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Off_Sales_Template", "RequiredAmount");
            DropColumn("dbo.Off_Sales_Template", "RequiredStorage");
        }
    }
}
