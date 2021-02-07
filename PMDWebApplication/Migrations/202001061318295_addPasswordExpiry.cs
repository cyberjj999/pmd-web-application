namespace PMDWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addPasswordExpiry : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SecurityConfigurations", "PasswordExpiry", c => c.Int(nullable: false));
            DropColumn("dbo.SecurityConfigurations", "Password_Expiry");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SecurityConfigurations", "Password_Expiry", c => c.Int(nullable: false));
            DropColumn("dbo.SecurityConfigurations", "PasswordExpiry");
        }
    }
}
