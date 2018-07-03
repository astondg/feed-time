namespace feedtimeService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FeedingSide : DbMigration
    {
        public override void Up()
        {
            AddColumn("feedtime.FeedActivities", "FeedingSide", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("feedtime.FeedActivities", "FeedingSide");
        }
    }
}
