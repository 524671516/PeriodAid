namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _04071739 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Off_BonusRequest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CheckinId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        ReceiveOpenId = c.String(maxLength: 64),
                        ReceiveUserName = c.String(maxLength: 128),
                        ReceiveAmount = c.Int(nullable: false),
                        RequestUserName = c.String(maxLength: 128),
                        RequestTime = c.DateTime(nullable: false),
                        CommitUserName = c.String(maxLength: 128),
                        CommitTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Off_Checkin", t => t.CheckinId, cascadeDelete: true)
                .Index(t => t.CheckinId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Off_BonusRequest", "CheckinId", "dbo.Off_Checkin");
            DropIndex("dbo.Off_BonusRequest", new[] { "CheckinId" });
            DropTable("dbo.Off_BonusRequest");
        }
    }
}
