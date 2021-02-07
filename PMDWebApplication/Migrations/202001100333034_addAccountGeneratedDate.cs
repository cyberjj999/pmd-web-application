namespace PMDWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addAccountGeneratedDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "AccountGeneratedDate", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "AccountGeneratedDate");
        }
    }
}
