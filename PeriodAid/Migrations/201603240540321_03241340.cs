namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _03241340 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.details", "refund", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.details", "refund", c => c.Decimal(precision: 18, scale: 2));
        }
    }
}
