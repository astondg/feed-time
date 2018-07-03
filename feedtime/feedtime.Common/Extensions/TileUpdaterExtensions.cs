namespace FeedTime.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using FeedTime.Common.DataModel;
    using FeedTime.Common.Extensions;
    using Windows.Data.Xml.Dom;
    using Windows.System.UserProfile;
    using Windows.UI.Notifications;

    public static class TileUpdaterExtensions
    {
        /// <summary>
        /// Updates a scheduled a tile for this activity with info for the provided baby or creates & schedules a new tile for this activity if one doesn't exist
        /// </summary>
        public static void UpdateBabysActivityStatusTile(this TileUpdater tileUpdater, string activityName, BabyActivityStatus babysActivityStatus)
        {
            var existingTiles = tileUpdater.GetScheduledTileNotifications();
            var existingTile = existingTiles.SingleOrDefault(tile => tile.Tag == activityName);
            if (existingTile == null)
            {
                CreateActivityTile(tileUpdater, activityName, new[] { babysActivityStatus });
            }
            else
            {
                tileUpdater.RemoveFromSchedule(existingTile);

                var newTileContent = new XmlDocument();
                newTileContent.LoadXml(existingTile.Content.GetXml());
                var babysStatuses = newTileContent.GetElementsByTagName("text")
                                                  .Where(text => text.InnerText.StartsWith(babysActivityStatus.BabyGivenName));
                // TODO - handle long & short form for wide & medium tiles respectively
                foreach (var textElement in babysStatuses)
                    textElement.InnerText = CreateActivityStatusText(activityName, babysActivityStatus, false);

                tileUpdater.Update(new TileNotification(newTileContent) { Tag = activityName });
            }
        }

        /// <summary>
        /// Creates & schedules a tile for this activity that contains info for all the babies
        /// </summary>
        public static void CreateActivityTile(this TileUpdater tileUpdater, string activityName, IEnumerable<BabyActivityStatus> babiesActivityStatus)
        {
            // If no activities are running or scheduled then delete the tile for this activity
            if (babiesActivityStatus.Count(s => (s.ActivityIsRunning && s.LastActivityStartTime.HasValue) || s.NextActivityStartTime.HasValue) == 0)
            {
                var existingTile = tileUpdater.GetScheduledTileNotifications()
                                              .FirstOrDefault(t => t.Tag.Equals(activityName));
                if (existingTile != null)
                    tileUpdater.RemoveFromSchedule(existingTile);
                return;
            }

            TileNotification tileNotification;
            //if (babiesActivityStatus.Count() == 1 && babiesActivityStatus.All(s => s.ActivityIsRunning && s.LastActivityStartTime.HasValue))
            //{
            //    var activityStatus = babiesActivityStatus.First();
            //    var tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Block);
            //    var duration = DateTimeOffset.Now - activityStatus.LastActivityStartTime.Value;
            //    var textElements = tileXml.GetElementsByTagName("text");
            //    textElements[0].InnerText = duration.TotalMinutes.ToString("F0");
            //    textElements[1].InnerText = activityName;
            //    tileNotification = new TileNotification(tileXml) { Tag = activityName };
            //}
            var mediumTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquare150x150Text01);
            var mediumTileTextElements = mediumTileXml.GetElementsByTagName("text");
            var wideTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150Text01);
            var wideTileTextElements = wideTileXml.GetElementsByTagName("text");
            
            mediumTileTextElements[0].InnerText = activityName;
            wideTileTextElements[0].InnerText = activityName;

            int babyCount = 1;
            DateTimeOffset? latestStartTime = null;
            foreach (var activityStatus in babiesActivityStatus)
            {
                if (activityStatus.NextActivityStartTime > latestStartTime)
                    latestStartTime = activityStatus.NextActivityStartTime;

                string longStatusText = CreateActivityStatusText(activityName, activityStatus, true);
                string shortStatusText = CreateActivityStatusText(activityName, activityStatus, false);

                wideTileTextElements[babyCount].InnerText = longStatusText;
                if (babyCount <= 3)
                    mediumTileTextElements[babyCount].InnerText = shortStatusText;
                
                babyCount++;
                if (babyCount > 4) break;
            }

            // Import the wide tile payload into the medium tile
            var wideTileBindingElement = mediumTileXml.ImportNode(wideTileXml.GetElementsByTagName("binding").Item(0), true);
            mediumTileXml.GetElementsByTagName("visual").Item(0).AppendChild(wideTileBindingElement);

            // Create & schedule the tile notification
            tileNotification = new TileNotification(mediumTileXml) { Tag = activityName };
            if (latestStartTime.HasValue)
                tileNotification.ExpirationTime = latestStartTime.Value.ToLocalTime();

            tileUpdater.Update(tileNotification);
        }

        private static string CreateActivityStatusText(string activityName, BabyActivityStatus activityStatus, bool longForm)
        {
            if (activityStatus.ActivityIsRunning)
                return longForm
                        ? string.Format(CultureInfo.CurrentCulture,
                                        "{0} {1} since {2}",
                                        activityStatus.BabyGivenName,
                                        ConvertActivityNameToCurrentTense(activityName),
                                        activityStatus.LastActivityStartTime.ToStringWithSystemClock("shorttime shortdate"))
                        : string.Format(CultureInfo.CurrentCulture,
                                        "{0} 🕒 {1:t}",
                                        activityStatus.BabyGivenName,
                                        activityStatus.LastActivityStartTime.ToStringWithSystemClock("shorttime"));
            else if (activityStatus.NextActivityStartTime.HasValue)
                return longForm
                        ? string.Format(CultureInfo.CurrentCulture,
                                        "{0} due to {1} at {2}",
                                        activityStatus.BabyGivenName,
                                        activityName,
                                        activityStatus.NextActivityStartTime.ToStringWithSystemClock("shorttime shortdate"))
                        : string.Format(CultureInfo.CurrentCulture,
                                        "{0} ⏩ {1:t}",
                                        activityStatus.BabyGivenName,
                                        activityStatus.NextActivityStartTime.ToStringWithSystemClock("shorttime"));
            else
                return longForm
                        ? string.Format(CultureInfo.CurrentCulture,
                                     "Calculating {0}'s next {1}",
                                     activityStatus.BabyGivenName,
                                     activityName)
                        : string.Format(CultureInfo.CurrentCulture,
                                     "{0} ⏩ ???",
                                     activityStatus.BabyGivenName);
        }

        private static string ConvertActivityNameToCurrentTense(string word)
        {
            switch (word)
            {
                case "change":
                    return "changing";
                case "feed":
                case "sleep":
                default:
                    return word + "ing";
            }
        }
    }
}