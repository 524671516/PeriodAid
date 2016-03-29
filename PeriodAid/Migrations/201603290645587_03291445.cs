namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _03291445 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Off_Manager_Request",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ManagerUserName = c.String(maxLength: 64),
                        StoreId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        RequestType = c.String(maxLength: 64),
                        RequestContent = c.String(maxLength: 1024),
                        RequestRemark = c.String(maxLength: 512),
                        RequestTime = c.DateTime(nullable: false),
                        ReplyContent = c.String(maxLength: 512),
                        ReplyUser = c.String(maxLength: 64),
                        ReplyTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Off_Store", t => t.StoreId, cascadeDelete: true)
                .Index(t => t.StoreId);
            
            CreateTable(
                "dbo.Off_Manager_Announcement",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ManagerUserName = c.String(maxLength: 512),
                        StartTime = c.DateTime(nullable: false),
                        FinishTime = c.DateTime(nullable: false),
                        Priority = c.Int(nullable: false),
                        Title = c.String(maxLength: 64),
                        Content = c.String(maxLength: 1024),
                        SubmitUser = c.String(maxLength: 64),
                        SubmitTime = c.DateTime(nullable: false),
                        Status = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Off_Manager_Request", "StoreId", "dbo.Off_Store");
            DropIndex("dbo.Off_Manager_Request", new[] { "StoreId" });
            DropTable("dbo.Off_Manager_Announcement");
            DropTable("dbo.Off_Manager_Request");
        }
    }
}
