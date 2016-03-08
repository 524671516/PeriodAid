namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _03071143 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WxRedPackOrder", "err_code_desc", c => c.String(maxLength: 128));
            AlterColumn("dbo.WxRedPackOrder", "hb_type", c => c.String(maxLength: 32));
            AlterColumn("dbo.WxRedPackOrder", "send_type", c => c.String(maxLength: 32));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.WxRedPackOrder", "send_type", c => c.String(nullable: false, maxLength: 32));
            AlterColumn("dbo.WxRedPackOrder", "hb_type", c => c.String(nullable: false, maxLength: 32));
            DropColumn("dbo.WxRedPackOrder", "err_code_desc");
        }
    }
}
