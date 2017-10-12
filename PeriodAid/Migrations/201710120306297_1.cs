namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SS_SalesRecord", "Pay_Money", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SS_SalesRecord", "SubAccount_Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SS_SalesRecord", "SubAccount_Price");
            DropColumn("dbo.SS_SalesRecord", "Pay_Money");
        }
    }
}
