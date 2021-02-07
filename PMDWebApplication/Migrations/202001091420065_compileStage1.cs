namespace PMDWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class compileStage1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "Age", c => c.Int(nullable: false));
            AddColumn("dbo.AspNetUsers", "isVerified", c => c.Boolean(nullable: false));
            AddColumn("dbo.AspNetUsers", "PromoCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "PromoCode");
            DropColumn("dbo.AspNetUsers", "isVerified");
            DropColumn("dbo.AspNetUsers", "Age");
        }
    }
}
