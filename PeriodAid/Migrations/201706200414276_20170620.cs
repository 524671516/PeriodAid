namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20170620 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Subject", "PicUrl");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Subject", "PicUrl", c => c.String(maxLength: 256));
        }
    }
}
