namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _03071226 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.WxRedPackOrder", "wishing", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.WxRedPackOrder", "wishing", c => c.String(nullable: false, maxLength: 128));
        }
    }
}
