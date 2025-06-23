namespace AkribisFAM.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserLevels",
                c => new
                    {
                        UserLevelId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 2147483647),
                        Level = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserLevelId);
            
            CreateTable(
                "dbo.UserRights",
                c => new
                    {
                        UserRightId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 2147483647),
                    })
                .PrimaryKey(t => t.UserRightId);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserID = c.Int(nullable: false, identity: true),
                        Username = c.String(maxLength: 30),
                        DisplayName = c.String(maxLength: 2147483647),
                        Password = c.String(maxLength: 2147483647),
                        Created = c.DateTime(nullable: false),
                        LastUpdate = c.DateTime(nullable: false),
                        UserLevelId = c.Int(nullable: false),
                        CreatorId = c.Int(),
                    })
                .PrimaryKey(t => t.UserID)
                .ForeignKey("dbo.Users", t => t.CreatorId)
                .ForeignKey("dbo.UserLevels", t => t.UserLevelId, cascadeDelete: true)
                .Index(t => t.Username, unique: true)
                .Index(t => t.UserLevelId)
                .Index(t => t.CreatorId);
            
            CreateTable(
                "dbo.LevelRight",
                c => new
                    {
                        UserLevelId = c.Int(nullable: false),
                        UserRightId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserLevelId, t.UserRightId })
                .ForeignKey("dbo.UserLevels", t => t.UserLevelId, cascadeDelete: true)
                .ForeignKey("dbo.UserRights", t => t.UserRightId, cascadeDelete: true)
                .Index(t => t.UserLevelId)
                .Index(t => t.UserRightId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Users", "UserLevelId", "dbo.UserLevels");
            DropForeignKey("dbo.Users", "CreatorId", "dbo.Users");
            DropForeignKey("dbo.LevelRight", "UserRightId", "dbo.UserRights");
            DropForeignKey("dbo.LevelRight", "UserLevelId", "dbo.UserLevels");
            DropIndex("dbo.LevelRight", new[] { "UserRightId" });
            DropIndex("dbo.LevelRight", new[] { "UserLevelId" });
            DropIndex("dbo.Users", new[] { "CreatorId" });
            DropIndex("dbo.Users", new[] { "UserLevelId" });
            DropIndex("dbo.Users", new[] { "Username" });
            DropTable("dbo.LevelRight");
            DropTable("dbo.Users");
            DropTable("dbo.UserRights");
            DropTable("dbo.UserLevels");
        }
    }
}
