namespace PMDWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AccessFailDate1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUsers", "AccessFailDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "AccessFailDate", c => c.DateTime(nullable: false));
        }
    }
}
