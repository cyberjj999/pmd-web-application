namespace PMDWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mySecondCompilation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SecurityConfigurations", "test", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SecurityConfigurations", "test");
        }
    }
}
