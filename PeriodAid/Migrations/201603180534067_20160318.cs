namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20160318 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Off_AVG_SalesData",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StoreId = c.Int(nullable: false),
                        DayOfWeek = c.Int(nullable: false),
                        AVG_BROWN = c.Decimal(precision: 18, scale: 2),
                        AVG_BLACK = c.Decimal(precision: 18, scale: 2),
                        AVG_LEMON = c.Decimal(precision: 18, scale: 2),
                        AVG_HONEY = c.Decimal(precision: 18, scale: 2),
                        AVG_DATES = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Off_AVG_SalesData");
        }
    }
}
