namespace FeedTime.Extensions
{
    using System;
    using System.Collections.ObjectModel;
    using FeedTime.Common.DataModel;
    using FeedTime.Strings;
    using FeedTime.ViewModels;

    public static class ModelExtensions
    {
        public static FamilyViewModel AsViewModel(this Family model)
        {
            return new FamilyViewModel
            {
                Id = model.Id
            };
        }

        public static BabyViewModel AsViewModel(this Baby model, Family family = null)
        {
            var dateOfBirthAsLocalTime = model.DateOfBirth.ToLocalTime();
            return new BabyViewModel()
            {
                Id = model.Id,
                GivenName = model.GivenName,
                AdditionalName = model.AdditionalName,
                FamilyName = model.FamilyName,
                DateOfBirth = dateOfBirthAsLocalTime,
                TimeOfBirth = dateOfBirthAsLocalTime.TimeOfDay,
                Gender = (Gender)model.Gender,
                Family = family != null ? family.AsViewModel() : new FamilyViewModel { Id = model.FamilyId }
            };
        }

        public static FeedActivityViewModel AsViewModel(this FeedActivity model, bool useMetricUnits, Baby baby = null)
        {
            if (model == null) return null;

            var startDate = model.StartTime.ToLocalTime();
            var millilitresConsumed = model.AmountConsumed.HasValue
                                        ? useMetricUnits
                                            ? model.AmountConsumed.Value
                                            : Convert.ToInt32(Math.Floor(model.AmountConsumed.Value * Constants.UNITCONVERSION_MILLILITRES_OUNCES))
                                        : model.AmountConsumed;

            return new FeedActivityViewModel
            {
                Id = model.Id,
                StartDate = startDate,
                StartTime = startDate.TimeOfDay,
                EndTime = model.EndTime.HasValue
                            ? model.EndTime.Value.ToLocalTime()
                            : (DateTimeOffset?)null,
                Duration = model.EndTime.HasValue
                            ? model.EndTime.Value - model.StartTime
                            : DateTimeOffset.Now - model.StartTime,
                MillilitresConsumed = millilitresConsumed,
                FeedingSide = model.FeedingSide.HasValue ? (Side)model.FeedingSide.Value : (Side?)null,
                Notes = model.Notes,
                HowBabyFelt = (Feeling?)model.HowBabyFelt,
                HowParentFelt = (Feeling?)model.HowParentFelt,
                CurrentVolumeUnit = useMetricUnits ? Constants.UNIT_SUFFIX_MILLILITRES : Constants.UNIT_SUFFIX_OUNCES,
                Baby = baby != null ? baby.AsViewModel() : new BabyViewModel { Id = model.BabyId }
            };
        }

        public static SleepActivityViewModel AsViewModel(this SleepActivity model, Baby baby = null)
        {
            if (model == null) return null;

            var startDate = model.StartTime.ToLocalTime();
            return new SleepActivityViewModel
            {
                Id = model.Id,
                StartDate = startDate,
                StartTime = startDate.TimeOfDay,
                EndTime = model.EndTime.HasValue
                            ? model.EndTime.Value.ToLocalTime()
                            : (DateTimeOffset?)null,
                Duration = model.EndTime.HasValue
                            ? model.EndTime.Value - model.StartTime
                            : DateTimeOffset.Now - model.StartTime,
                Notes = model.Notes,
                HowBabyFelt = (Feeling?)model.HowBabyFelt,
                HowParentFelt = (Feeling?)model.HowParentFelt,
                Baby = baby != null ? baby.AsViewModel() : new BabyViewModel { Id = model.BabyId }
            };
        }

        public static ChangeActivityViewModel AsViewModel(this ChangeActivity model, Baby baby = null)
        {
            if (model == null) return null;

            var startDate = model.StartTime.ToLocalTime();
            return new ChangeActivityViewModel
            {
                Id = model.Id,
                StartDate = startDate,
                StartTime = startDate.TimeOfDay,
                EndTime = model.EndTime.HasValue
                            ? model.EndTime.Value.ToLocalTime()
                            : (DateTimeOffset?)null,
                Duration = TimeSpan.Zero,
                Notes = model.Notes,
                HowBabyFelt = (Feeling?)model.HowBabyFelt,
                HowParentFelt = (Feeling?)model.HowParentFelt,
                Baby = baby != null ? baby.AsViewModel() : new BabyViewModel { Id = model.BabyId }
            };
        }

        public static MeasurementViewModel AsViewModel(this Measurement model, bool useMetricUnits, Baby baby = null)
        {
            if (model == null) return null;

            var length = model.Length.HasValue
                            ? Math.Round(useMetricUnits
                                            ? model.Length.Value
                                            : model.Length.Value * Constants.UNITCONVERSION_CENTIMETRES_INCHES, 2)
                            : model.Length;
            var weight = model.Weight.HasValue
                            ? Math.Round(useMetricUnits
                                            ? model.Weight.Value
                                            : model.Weight.Value * Constants.UNITCONVERSION_KILOGRAMS_POUNDS, 2)
                            : model.Weight;

            return new MeasurementViewModel
            {
                Id = model.Id,
                //CreatedAt = model.CreatedAt,
                Length = length,
                Weight = weight,
                Baby = baby != null ? baby.AsViewModel() : new BabyViewModel { Id = model.BabyId }
            };
        }

        public static ActivityScheduleViewModel AsViewModel(this ActivitySchedule model, bool useMetricUnits, Baby baby = null)
        {
            if (model == null)
                return new ActivityScheduleViewModel();

            return new ActivityScheduleViewModel
            {
                BabyId = model.BabyId,
                CurrentlyFeeding = model.CurrentlyFeeding,
                LastFeed = model.LastFeed.AsViewModel(useMetricUnits, baby),
                AverageTimeBetweenFeeds = model.AverageTimeBetweenFeeds,
                NextFeedDueAt = model.NextChangeDueAt.ToLocalTime(),

                CurrentlySleeping = model.CurrentlySleeping,
                LastSleep = model.LastSleep.AsViewModel(baby),
                AverageTimeBetweenSleeps = model.AverageTimeBetweenSleeps,
                NextSleepDueAt = model.NextSleepDueAt.ToLocalTime(),

                LastChange = model.LastChange.AsViewModel(baby),
                AverageTimeBetweenChanges = model.AverageTimeBetweenChanges,
                NextChangeDueAt = model.NextChangeDueAt.ToLocalTime(),

                ScheduleGeneratedAt = model.ScheduleGeneratedAt
            };
        }

        public static DataTrendsViewModel AsViewModel(this DataTrends model, DateTimeOffset babiesBirthDate)
        {
            if (model == null)
                return new DataTrendsViewModel();

            var viewModel = new DataTrendsViewModel
            {
                StartOfDay = model.StartOfDay.ToLocalTime(),
                StartOfWeek = model.StartOfWeek.ToLocalTime(),
                BabysBirthDate = model.BabysBirthDate.ToLocalTime(),
                AverageFeedsPerDay = model.AverageFeedsPerDay,
                AverageFeedDuration = model.AverageFeedDuration,
                AverageFeedVolume = model.AverageFeedVolume,
                AverageSleepsPerDay = model.AverageSleepsPerDay,
                AverageSleepDuration = model.AverageSleepDuration,
                AverageChangesPerDay = model.AverageChangesPerDay
            };

            // Hourly trends
            viewModel.FeedsOverLastDay = new ObservableCollection<ActivityTrendViewModel>(model.FeedsOverLastDay
                                                                                               .GenerateRangeFromValues(viewModel.StartOfDay,
                                                                                                                        24));
            viewModel.SleepsOverLastDay = new ObservableCollection<ActivityTrendViewModel>(model.SleepsOverLastDay
                                                                                                .GenerateRangeFromValues(viewModel.StartOfDay,
                                                                                                                         24));
            viewModel.ChangesOverLastDay = new ObservableCollection<ActivityTrendViewModel>(model.ChangesOverLastDay
                                                                                                 .GenerateRangeFromValues(viewModel.StartOfDay,
                                                                                                                          24));

            // Daily trends
            viewModel.BabysMoodOverLastWeek = new ObservableCollection<MoodTrendViewModel>(model.BabysMoodOverLastWeek
                                                                                                .GenerateRangeFromValues(viewModel.StartOfWeek.Date,
                                                                                                                         7));
            viewModel.ParentsMoodOverLastWeek = new ObservableCollection<MoodTrendViewModel>(model.ParentsMoodOverLastWeek
                                                                                                  .GenerateRangeFromValues(viewModel.StartOfWeek.Date,
                                                                                                                           7));

            // Trends since birth
            int numberOfWeeksSinceBirth = (DateTimeOffset.Now - model.BabysBirthDate).Days / 7;
            viewModel.LengthSinceBirth = new ObservableCollection<MeasurementTrendViewModel>(model.MeasurementsSinceBirth
                                                                                                  .GenerateRangeFromValues(model.BabysBirthDate,
                                                                                                                           numberOfWeeksSinceBirth,
                                                                                                                           m => m.Length));
            viewModel.WeightSinceBirth = new ObservableCollection<MeasurementTrendViewModel>(model.MeasurementsSinceBirth
                                                                                                  .GenerateRangeFromValues(model.BabysBirthDate,
                                                                                                                           numberOfWeeksSinceBirth,
                                                                                                                           m => m.Weight));

            return viewModel;
        }
    }
}
