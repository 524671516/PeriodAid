namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _03251751 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Off_Manager_Request",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ManagerUserName = c.String(maxLength: 512),
                        StoreId = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        RequestType = c.String(maxLength: 64),
                        RuquestContent = c.String(maxLength: 1024),
                        RequestRemark = c.String(maxLength: 512),
                        RequestTime = c.DateTime(nullable: false),
                        ReplyContent = c.String(maxLength: 512),
                        ReplyUser = c.String(maxLength: 64),
                        ReplyTime = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Off_Store", t => t.StoreId, cascadeDelete: true)
                .Index(t => t.StoreId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Off_Manager_Request", "StoreId", "dbo.Off_Store");
            DropIndex("dbo.Off_Manager_Request", new[] { "StoreId" });
            DropTable("dbo.Off_Manager_Request");
        }
    }
}
