namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201603211800 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.invoices", "invoice_content", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.invoices", "invoice_content", c => c.String(maxLength: 128));
        }
    }
}
