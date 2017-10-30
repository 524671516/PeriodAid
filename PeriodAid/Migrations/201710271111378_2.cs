namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.SS_Channel",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Channel_Name = c.String(maxLength: 16),
                        Terrace_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SS_DateFlow",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DateFlow_Date = c.DateTime(nullable: false),
                        Product_Flow = c.Int(nullable: false),
                        Product_Visitor = c.Int(nullable: false),
                        Product_Customer = c.Int(nullable: false),
                        Order_Count = c.Int(nullable: false),
                        Convert_Ratio = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SS_Terrace",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Terrace_Name = c.String(maxLength: 10),
                        Channel_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.SS_Terrace");
            DropTable("dbo.SS_DateFlow");
            DropTable("dbo.SS_Channel");
        }
    }
}
