namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _201712291038 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SP_Seller", "Department_Id", c => c.Int(nullable: false));
            CreateIndex("dbo.SP_Seller", "Department_Id");
            AddForeignKey("dbo.SP_Seller", "Department_Id", "dbo.SP_Department", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.SP_Seller", "Department_Id", "dbo.SP_Department");
            DropIndex("dbo.SP_Seller", new[] { "Department_Id" });
            DropColumn("dbo.SP_Seller", "Department_Id");
        }
    }
}
