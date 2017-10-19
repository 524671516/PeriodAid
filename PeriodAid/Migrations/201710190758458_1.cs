namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SS_Product", "Bar_Code", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SS_Product", "Bar_Code");
        }
    }
}
