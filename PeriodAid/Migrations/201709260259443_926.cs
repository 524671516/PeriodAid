namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _926 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.SS_UploadRecord", "Plattform_Id", "dbo.SS_Plattform");
            DropIndex("dbo.SS_UploadRecord", new[] { "Plattform_Id" });
            DropColumn("dbo.SS_Product", "Carton_Spec");
            DropColumn("dbo.SS_Product", "Purchase_Price");
            DropColumn("dbo.SS_Storage", "Index");
            DropTable("dbo.SS_UploadRecord");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.SS_UploadRecord",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SalesRecord_Date = c.DateTime(nullable: false),
                        Plattform_Id = c.Int(nullable: false),
                        Upload_Date = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.SS_Storage", "Index", c => c.Int(nullable: false));
            AddColumn("dbo.SS_Product", "Purchase_Price", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.SS_Product", "Carton_Spec", c => c.Int(nullable: false));
            CreateIndex("dbo.SS_UploadRecord", "Plattform_Id");
            AddForeignKey("dbo.SS_UploadRecord", "Plattform_Id", "dbo.SS_Plattform", "Id");
        }
    }
}
