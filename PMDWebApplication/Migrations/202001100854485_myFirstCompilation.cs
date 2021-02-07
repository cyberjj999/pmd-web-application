namespace PMDWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class myFirstCompilation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "myNewVariable", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "myNewVariable");
        }
    }
}
