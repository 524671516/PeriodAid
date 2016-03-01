namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20160301 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Off_Seller", "AccountName", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Off_Seller", "AccountName");
        }
    }
}
