namespace PMDWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removedTestCompilationVariable : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.SecurityConfigurations", "test");
            DropColumn("dbo.AspNetUsers", "myNewVariable");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "myNewVariable", c => c.String());
            AddColumn("dbo.SecurityConfigurations", "test", c => c.Int(nullable: false));
        }
    }
}
