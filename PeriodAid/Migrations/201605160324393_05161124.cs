namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _05161124 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.P_Presents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        openId = c.String(nullable: false, maxLength: 32),
                        status = c.Int(nullable: false),
                        source_ReceiverName = c.String(nullable: false, maxLength: 32),
                        source_ReceiverMobile = c.String(nullable: false, maxLength: 32),
                        source_ReceiverAddress = c.String(nullable: false, maxLength: 256),
                        target_ReceiverName = c.String(maxLength: 32),
                        target_ReceiverMobile = c.String(maxLength: 32),
                        target_ReceiverState = c.String(maxLength: 32),
                        target_ReceiverCity = c.String(maxLength: 32),
                        target_ReceiverDistrict = c.String(maxLength: 32),
                        target_ReceiverAddress = c.String(maxLength: 256),
                        target_ReceiverZip = c.String(maxLength: 16),
                        express_name = c.String(maxLength: 32),
                        mail_no = c.String(maxLength: 32),
                        create_time = c.DateTime(nullable: false),
                        plattform_code = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.P_Presents");
        }
    }
}
