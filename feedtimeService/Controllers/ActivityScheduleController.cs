namespace feedtimeService.Controllers
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using feedtimeService.DataObjects;
    using feedtimeService.Extensions;
    using feedtimeService.Models;
    using Microsoft.WindowsAzure.Mobile.Service;

    public class ActivityScheduleController : ApiController
    {
        public ApiServices Services { get; set; }

        // GET api/Schedule/abc123
        public ActivitySchedule Get(string babyId)
        {
            using (var context = new feedtimeContext())
            {
                string userId = User.GetId();
                var currentBaby = context.Set<Baby>()
                                         .Where(baby => baby.Id == babyId
                                                        && baby.Family.UserProfiles.Any(up => up.UserId == userId))
                                         .SingleOrDefault();
                if (currentBaby == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                // Use rolling windows for last 'week'
                var now = DateTimeOffset.UtcNow;
                var today = DateTimeOffset.UtcNow.Date;
                var startOfWeek = DateTimeOffset.UtcNow.AddDays(-7);

                // Calculate feed schedule for last 7 days
                long? averageTimeBetweenFeeds = null;
                //long? averageFeedTime = null;
                DateTimeOffset nextFeedDueAt = currentBaby.DateOfBirth.Add(TimeSpan.FromHours(2));
                var feedActivities = context.FeedActivities
                                             .Where(ca => ca.BabyId == babyId
                                                          && ca.StartTime >= startOfWeek
                                                          && !ca.Deleted)
                                             .OrderByDescending(ca => ca.StartTime);
                
                var latestFeedActivity = feedActivities.FirstOrDefault();
                if (latestFeedActivity != null)
                {
                    var feedActivityStartTimes = feedActivities.Select(ca => ca.StartTime)
                                                               .ToList();

                    // Calculate average time between feeds
                    long totalTimeBetweenFeeds = 0;
                    feedActivityStartTimes.Aggregate((a, b) =>
                        {
                            totalTimeBetweenFeeds += (today.Add(a - b) - today).Ticks;
                            return b;
                        });
                    averageTimeBetweenFeeds = Convert.ToInt64(Math.Round((double)totalTimeBetweenFeeds / feedActivityStartTimes.Count()));

                    //// Calculate average feed time
                    //var completedFeedActivities = feedActivities.Where(a => a.EndTime.HasValue);
                    //var totalFeedTime = completedFeedActivities.Aggregate(0L, (a, b) => a + (b.EndTime.Value - b.StartTime).Ticks);
                    //averageFeedTime = totalFeedTime / completedFeedActivities.Count();

                    // Calculate next feed time
                    var minimumNextFeedTime = latestFeedActivity.StartTime.Add(new TimeSpan(averageTimeBetweenFeeds.Value));
                    var feedActivitiesByDate = feedActivities.ToList()
                                                                .GroupBy(a => a.StartTime.Date)
                                                                // For each date select the first feed after 'now'
                                                                .Select(ag =>
                                                                    {
                                                                        var ascendingStartTimes = ag.OrderBy(a => a.StartTime.TimeOfDay);
                                                                        var nextFeed = ascendingStartTimes
                                                                                        .FirstOrDefault(a => a.StartTime.TimeOfDay > minimumNextFeedTime.TimeOfDay);
                                                                        if (nextFeed == null)
                                                                            nextFeed = ascendingStartTimes.LastOrDefault();

                                                                        return nextFeed == null
                                                                            ? null
                                                                            : Tuple.Create(nextFeed, nextFeed.StartTime.TimeOfDay - now.TimeOfDay);
                                                                    })
                                                                .Where(a => a != null);

                    if (feedActivitiesByDate.Any())
                    {
                        var averageTimeToNextFeed = Convert.ToInt64(Math.Round((double)feedActivitiesByDate.Aggregate(0L, (a, b) => a + b.Item2.Ticks)
                                                                                / feedActivitiesByDate.Count()));
                        nextFeedDueAt = now.Add(new TimeSpan(averageTimeToNextFeed));
                    }
                }

                // Calculate sleep schedule for last 7 days
                long? averageTimeBetweenSleeps = null;
                DateTimeOffset nextSleepDueAt = currentBaby.DateOfBirth.Add(TimeSpan.FromHours(2));
                var sleepActivities = context.SleepActivities
                                             .Where(ca => ca.BabyId == babyId
                                                          && ca.StartTime >= startOfWeek
                                                          && !ca.Deleted)
                                             .OrderByDescending(ca => ca.StartTime);
                var latestSleepActivity = sleepActivities.FirstOrDefault();
                if (latestSleepActivity != null)
                {
                    var sleepActivityStartTimes = sleepActivities.Select(ca => ca.StartTime)
                                                                 .ToList();
                    long totalTimeBetweenSleeps = 0;
                    sleepActivityStartTimes.Aggregate((a, b) =>
                        {
                            totalTimeBetweenSleeps += (today.Add(a - b) - today).Ticks;
                            return b;
                        });
                    averageTimeBetweenSleeps = Convert.ToInt64(Math.Round((double)totalTimeBetweenSleeps / sleepActivityStartTimes.Count()));

                    // Calculate next sleep time                    
                    var minimumNextSleepTime = latestSleepActivity.StartTime.Add(new TimeSpan(averageTimeBetweenSleeps.Value));
                    var sleepActivitiesByDate = sleepActivities.ToList()
                                                                .GroupBy(a => a.StartTime.Date)
                                                                // For each date select the first sleep after 'now'
                                                                .Select(ag =>
                                                                    {
                                                                        var ascendingStartTimes = ag.OrderBy(a => a.StartTime.TimeOfDay);
                                                                        var nextSleep = ascendingStartTimes
                                                                                        .FirstOrDefault(a => a.StartTime.TimeOfDay > minimumNextSleepTime.TimeOfDay);
                                                                        if (nextSleep == null)
                                                                            nextSleep = ascendingStartTimes.LastOrDefault();

                                                                        return nextSleep == null
                                                                                ? null
                                                                                : Tuple.Create(nextSleep, nextSleep.StartTime.TimeOfDay - now.TimeOfDay);
                                                                    })
                                                                .Where(a => a != null);

                    if (sleepActivitiesByDate.Any())
                    {
                        var averageTimeToNextSleep = Convert.ToInt64(Math.Round((double)sleepActivitiesByDate.Aggregate(0L, (a, b) => a + b.Item2.Ticks)
                                                                                / sleepActivitiesByDate.Count()));
                        nextSleepDueAt = now.Add(new TimeSpan(averageTimeToNextSleep));
                    }
                }

                // Calculate change schedule for last 7 days
                long? averageTimeBetweenChanges = null;
                DateTimeOffset nextChangeDueAt = currentBaby.DateOfBirth.Add(TimeSpan.FromHours(3));
                var changeActivities = context.ChangeActivities
                                              .Where(ca => ca.BabyId == babyId
                                                           && ca.StartTime >= startOfWeek
                                                           && !ca.Deleted)
                                              .OrderByDescending(ca => ca.StartTime);
                var latestChangeActivity = changeActivities.FirstOrDefault();
                if (latestChangeActivity != null)
                {
                    var changeActivityStartTimes = changeActivities.Select(ca => ca.StartTime)
                                                                   .ToList();
                    long totalTimeBetweenChanges = 0;
                    changeActivityStartTimes.Aggregate((a, b) =>
                     {
                         totalTimeBetweenChanges += (today.Add(a - b) - today).Ticks;
                         return b;
                     });
                    averageTimeBetweenChanges = Convert.ToInt64(Math.Round((double)totalTimeBetweenChanges / changeActivityStartTimes.Count()));

                    // Calculate next change time
                    var minimumNextChangeTime = latestChangeActivity.StartTime.Add(new TimeSpan(averageTimeBetweenChanges.Value));
                    var changeActivitiesByDate = changeActivities.ToList()
                                                                 .GroupBy(a => a.StartTime.Date)
                                                                 // For each date select the first feed after 'now'
                                                                 .Select(ag =>
                                                                     {
                                                                         var ascendingStartTimes = ag.OrderBy(a => a.StartTime.TimeOfDay);
                                                                         var nextChange = ascendingStartTimes
                                                                                            .FirstOrDefault(a => a.StartTime.TimeOfDay > minimumNextChangeTime.TimeOfDay);
                                                                         if (nextChange == null)
                                                                             nextChange = ascendingStartTimes.LastOrDefault();

                                                                         return nextChange == null
                                                                                 ? null
                                                                                 : Tuple.Create(nextChange, nextChange.StartTime.TimeOfDay - now.TimeOfDay);
                                                                     })
                                                                 .Where(a => a != null);
                    if (changeActivitiesByDate.Any())
                    {
                        var averageTimeToNextChange = Convert.ToInt64(Math.Round((double)changeActivitiesByDate.Aggregate(0L, (a, b) => a + b.Item2.Ticks)
                                                                                / changeActivitiesByDate.Count()));
                        nextChangeDueAt = now.Add(new TimeSpan(averageTimeToNextChange));
                    }
                }

                // Generate the complete schedule
                return new ActivitySchedule
                {
                    BabyId = babyId,
                    ScheduleGeneratedAt = DateTimeOffset.UtcNow,

                    CurrentlyFeeding = latestFeedActivity != null
                                        ? !latestFeedActivity.EndTime.HasValue : false,
                    LastFeed = latestFeedActivity,
                    AverageTimeBetweenFeeds = averageTimeBetweenFeeds.HasValue
                                                ? new TimeSpan(averageTimeBetweenFeeds.Value) : (TimeSpan?)null,
                    NextFeedDueAt = nextFeedDueAt,

                    CurrentlySleeping = latestSleepActivity != null
                                        ? !latestSleepActivity.EndTime.HasValue : false,
                    LastSleep = latestSleepActivity,
                    AverageTimeBetweenSleeps = averageTimeBetweenSleeps.HasValue
                                                ? new TimeSpan(averageTimeBetweenSleeps.Value) : (TimeSpan?)null,
                    NextSleepDueAt = nextSleepDueAt,

                    LastChange = latestChangeActivity,
                    AverageTimeBetweenChanges = averageTimeBetweenChanges.HasValue
                                                ? new TimeSpan(averageTimeBetweenChanges.Value) : (TimeSpan?)null,
                    NextChangeDueAt = nextChangeDueAt

                };
            }
        }

    }
}
