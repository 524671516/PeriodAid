namespace PeriodAid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _03161115 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Off_Manager_CheckIn", "Photo", c => c.String(maxLength: 128));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Off_Manager_CheckIn", "Photo", c => c.String(maxLength: 64));
        }
    }
}
