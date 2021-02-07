namespace PMDWebApplication.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removepasswordresetdate3 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.AspNetUsers", "PasswordResetDate");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "PasswordResetDate", c => c.DateTime());
        }
    }
}
