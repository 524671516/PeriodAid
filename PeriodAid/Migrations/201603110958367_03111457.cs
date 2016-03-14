namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _03111457 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Off_Manager_CheckIn",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Manager_EventId = c.Int(nullable: false),
                        Canceled = c.Boolean(nullable: false),
                        Location = c.String(maxLength: 64),
                        Location_Desc = c.String(maxLength: 128),
                        Photo = c.String(maxLength: 64),
                        Remark = c.String(maxLength: 64),
                        CheckIn_Time = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Off_Manager_Task", t => t.Manager_EventId, cascadeDelete: true)
                .Index(t => t.Manager_EventId);
            
            CreateTable(
                "dbo.Off_Manager_Task",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Status = c.Int(nullable: false),
                        UserName = c.String(maxLength: 32),
                        TaskDate = c.DateTime(nullable: false),
                        Event_Complete = c.String(maxLength: 512),
                        Event_UnComplete = c.String(maxLength: 512),
                        Event_Assistance = c.String(maxLength: 512),
                        Eval_Value = c.Int(),
                        Eval_Remark = c.String(maxLength: 256),
                        Eval_User = c.String(maxLength: 32),
                        Eval_Time = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            AlterColumn("dbo.Off_StoreManager", "UserName", c => c.String(maxLength: 32));
            AlterColumn("dbo.Off_StoreManager", "NickName", c => c.String(maxLength: 32));
            AlterColumn("dbo.Off_StoreManager", "Mobile", c => c.String(maxLength: 32));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Off_Manager_CheckIn", "Manager_EventId", "dbo.Off_Manager_Task");
            DropIndex("dbo.Off_Manager_CheckIn", new[] { "Manager_EventId" });
            AlterColumn("dbo.Off_StoreManager", "Mobile", c => c.String());
            AlterColumn("dbo.Off_StoreManager", "NickName", c => c.String());
            AlterColumn("dbo.Off_StoreManager", "UserName", c => c.String());
            DropTable("dbo.Off_Manager_Task");
            DropTable("dbo.Off_Manager_CheckIn");
        }
    }
}
