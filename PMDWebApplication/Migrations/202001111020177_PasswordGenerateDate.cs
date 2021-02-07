namespace PMDWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PasswordGenerateDate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "PasswordGeneratedDate", c => c.DateTime());
            DropColumn("dbo.AspNetUsers", "AccountGeneratedDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "AccountGeneratedDate", c => c.DateTime());
            DropColumn("dbo.AspNetUsers", "PasswordGeneratedDate");
        }
    }
}
