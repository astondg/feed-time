namespace feedtimeService.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using feedtimeService.DataObjects;
    using feedtimeService.Extensions;
    using feedtimeService.Models;
    using Microsoft.WindowsAzure.Mobile.Service;

    public class DataTrendsController : ApiController
    {
        public ApiServices Services { get; set; }

        // GET api/DataTrends
        public DataTrends Get(string babyId)
        {
            using (var context = new feedtimeContext())
            {
                // Use rolling windows for last 'day' & 'week'
                string userId = User.GetId();
                var today = DateTimeOffset.UtcNow.AddHours(-24);
                var startOfWeek = DateTimeOffset.UtcNow.AddDays(-7);
                var dataTrends = new DataTrends { BabyId = babyId };

                var currentBaby = context.Set<Baby>()
                                         .Where(baby => baby.Id == babyId
                                                        && baby.Family.UserProfiles.Any(up => up.UserId == userId))
                                         .SingleOrDefault();
                if (currentBaby == null)
                    throw new HttpResponseException(HttpStatusCode.NotFound);

                dataTrends.StartOfDay = today;
                dataTrends.StartOfWeek = startOfWeek;
                dataTrends.BabysBirthDate = currentBaby.DateOfBirth;

                // Retrieve the averages for feeds & sleeps
                var feedsOverLastWeek = context.Set<FeedActivity>()
                                               .Where(activity => activity.BabyId == babyId
                                                                    && activity.StartTime >= startOfWeek
                                                                    && activity.EndTime.HasValue
                                                                    && !activity.Deleted)
                                                .AsEnumerable();
                var feedsOverLastWeekWithVolume = feedsOverLastWeek.Where(activity => activity.MillilitresConsumed.HasValue);
                var feedsOverLastWeekByDay = feedsOverLastWeek.GroupBy(f => f.StartTime.Date);
                dataTrends.AverageFeedsPerDay = feedsOverLastWeekByDay.Aggregate(0,
                                                                                 (a, b) => a + b.Count(),
                                                                                 a => feedsOverLastWeekByDay.Any()
                                                                                        ? Convert.ToInt32(Math.Round((double)a / feedsOverLastWeekByDay.Count()))
                                                                                        : 0);
                dataTrends.AverageFeedDuration = feedsOverLastWeek.Aggregate(0d,
                                                                            (a, b) => a + (b.EndTime.Value - b.StartTime).Duration().Ticks,
                                                                            a => feedsOverLastWeek.Any()
                                                                                    ? new TimeSpan(Convert.ToInt64(Math.Round((double)a / feedsOverLastWeek.Count())))
                                                                                    : TimeSpan.Zero);
                dataTrends.AverageFeedVolume = feedsOverLastWeekWithVolume.Aggregate(0,
                                                                                     (a, b) => a + b.MillilitresConsumed.Value,
                                                                                     a => feedsOverLastWeekWithVolume.Any()
                                                                                            ? Convert.ToInt32(Math.Round((double)a / feedsOverLastWeekWithVolume.Count()))
                                                                                            : 0);

                var sleepsOverLastWeek = context.Set<SleepActivity>()
                                                .Where(activity => activity.BabyId == babyId
                                                                    && activity.StartTime >= startOfWeek
                                                                    && activity.EndTime.HasValue
                                                                    && !activity.Deleted)
                                                .AsEnumerable();
                var sleepsOverLastWeekByDay = sleepsOverLastWeek.GroupBy(f => f.StartTime.Date);
                dataTrends.AverageSleepsPerDay = sleepsOverLastWeekByDay.Aggregate(0,
                                                                                   (a, b) => a + b.Count(),
                                                                                   a => sleepsOverLastWeekByDay.Any()
                                                                                        ? Convert.ToInt32(Math.Round((double)a / sleepsOverLastWeekByDay.Count()))
                                                                                        : 0);
                dataTrends.AverageSleepDuration = sleepsOverLastWeek.Aggregate(0d,
                                                                               (a, b) => a + (b.EndTime.Value - b.StartTime).Duration().Ticks,
                                                                               a => sleepsOverLastWeek.Any()
                                                                                    ? new TimeSpan(Convert.ToInt64(Math.Round((double)a / sleepsOverLastWeek.Count())))
                                                                                    : TimeSpan.Zero);

                var changesOverLastWeek = context.Set<ChangeActivity>()
                                                .Where(activity => activity.BabyId == babyId
                                                                    && activity.StartTime >= startOfWeek
                                                                    && activity.EndTime.HasValue
                                                                    && !activity.Deleted)
                                                .AsEnumerable();
                var changesOverLastWeekByDay = changesOverLastWeek.GroupBy(f => f.StartTime.Date);
                dataTrends.AverageChangesPerDay = changesOverLastWeekByDay.Aggregate(0,
                                                                                     (a, b) => a + b.Count(),
                                                                                     a => changesOverLastWeekByDay.Any()
                                                                                            ? Convert.ToInt32(Math.Round((double)a / changesOverLastWeekByDay.Count()))
                                                                                            : 0);

                // Retrieve the data for activities over the last 24 hours
                dataTrends.FeedsOverLastDay = context.Set<FeedActivity>()
                                                     .Where(activity => activity.BabyId == babyId
                                                                          && activity.StartTime >= today
                                                                          && !activity.Deleted)
                                                     .Select(activity => new ActivityTrend { StartTime = activity.StartTime, EndTime = activity.EndTime })
                                                     .ToList();
                dataTrends.SleepsOverLastDay = context.Set<SleepActivity>()
                                                      .Where(activity => activity.BabyId == babyId
                                                                           && activity.StartTime >= today
                                                                           && !activity.Deleted)
                                                      .Select(activity => new ActivityTrend { StartTime = activity.StartTime, EndTime = activity.EndTime })
                                                      .ToList();
                dataTrends.ChangesOverLastDay = context.Set<ChangeActivity>()
                                                       .Where(activity => activity.BabyId == babyId
                                                                            && activity.StartTime >= today
                                                                           && !activity.Deleted)
                                                       .Select(activity => new ActivityTrend { StartTime = activity.StartTime, EndTime = activity.EndTime })
                                                       .ToList();

                // Retrieve the data for mood over the last 7 days
                var sleepActivitiesWithFeeling = context.Set<SleepActivity>()
                                                        .Where(activity => activity.BabyId == babyId
                                                                            && activity.StartTime >= startOfWeek
                                                                            && (activity.HowBabyFelt != null || activity.HowParentFelt != null)
                                                                            && !activity.Deleted)
                                                        .Select(activity => new { StartTime = activity.StartTime, HowBabyFelt = activity.HowBabyFelt, HowParentFelt = activity.HowParentFelt })
                                                        .Union(context.Set<FeedActivity>()
                                                                        .Where(activity => activity.BabyId == babyId
                                                                                            && activity.StartTime >= startOfWeek
                                                                                            && (activity.HowBabyFelt != null || activity.HowParentFelt != null)
                                                                                            && !activity.Deleted)
                                                                        .Select(activity => new { StartTime = activity.StartTime, HowBabyFelt = activity.HowBabyFelt, HowParentFelt = activity.HowParentFelt }))
                                                        .Union(context.Set<ChangeActivity>()
                                                                        .Where(activity => activity.BabyId == babyId
                                                                                            && activity.StartTime >= startOfWeek
                                                                                            && (activity.HowBabyFelt != null || activity.HowParentFelt != null)
                                                                                            && !activity.Deleted)
                                                                        .Select(activity => new { StartTime = activity.StartTime, HowBabyFelt = activity.HowBabyFelt, HowParentFelt = activity.HowParentFelt }))
                                                        .AsEnumerable();

                dataTrends.BabysMoodOverLastWeek = sleepActivitiesWithFeeling.Where(activity => activity.HowBabyFelt.HasValue)
                                                                              .GroupBy(activity => activity.StartTime.Date)
                                                                              .AsEnumerable()
                                                                              .Select(activityGroup => new MoodTrend
                                                                              {
                                                                                  Date = activityGroup.Key,
                                                                                  Feeling = AverageFeeling(activityGroup.Select(ag => ag.HowBabyFelt))
                                                                              })
                                                                              .ToList();

                dataTrends.ParentsMoodOverLastWeek = sleepActivitiesWithFeeling.Where(activity => activity.HowParentFelt.HasValue)
                                                                              .GroupBy(activity => activity.StartTime.Date)
                                                                              .AsEnumerable()
                                                                              .Select(activityGroup => new MoodTrend
                                                                              {
                                                                                  Date = activityGroup.Key,
                                                                                  Feeling = AverageFeeling(activityGroup.Select(ag => ag.HowParentFelt))
                                                                              })
                                                                              .ToList();

                // Retrieve the data for length and weight since birth
                dataTrends.MeasurementsSinceBirth = context.Set<Measurement>()
                                                           .Where(m => m.Weight != null || m.Length != null && !m.Deleted)
                                                           .AsEnumerable()
                                                           .GroupBy(measurement => measurement.CreatedAt.Value.Date)
                                                           .Select(measurementGroup => measurementGroup.OrderBy(measurement => measurement.CreatedAt.Value)
                                                                                                       .Last())
                                                           .AsEnumerable()
                                                           .Select(measurement => new MeasurementTrend
                                                           {
                                                               Date = measurement.CreatedAt.Value,
                                                               Length = measurement.Length,
                                                               Weight = measurement.Weight
                                                           })
                                                           .ToList();

                dataTrends.TrendsGeneratedAt = DateTimeOffset.UtcNow;

                return dataTrends;
            }
        }

        private static int AverageFeeling(IEnumerable<int?> feelings)
        {
            var averageFeeling = feelings.Aggregate(0d,
                                                      (a, b) => a + b.Value,
                                                      a => feelings.Any()
                                                            ? a / feelings.Count()
                                                            : 0);

            return Convert.ToInt32(Math.Round(averageFeeling));
        }
    }
}