namespace FeedTime.Extensions
{
    using System;
    using FeedTime.Common.DataModel;
    using FeedTime.Strings;
    using FeedTime.ViewModels;

    public static class ViewModelExtensions
    {
        public static Baby AsModel(this BabyViewModel viewModel)
        {
            return new Baby
            {
                Id = viewModel.Id,
                GivenName = viewModel.GivenName,
                AdditionalName = viewModel.AdditionalName,
                FamilyName = viewModel.FamilyName,
                DateOfBirth = viewModel.DateOfBirth
                                       .ToUniversalTime(),
                Gender = (int)viewModel.Gender,
                FamilyId = viewModel.Family.Id
            };
        }

        public static FeedActivity AsModel(this FeedActivityViewModel viewModel, bool viewModelUsesMetricUnits)
        {
            var millilitresConsumed = viewModel.MillilitresConsumed.HasValue
                                        ? viewModelUsesMetricUnits
                                            ? viewModel.MillilitresConsumed.Value
                                            : Convert.ToInt32(Math.Floor(viewModel.MillilitresConsumed.Value / Constants.UNITCONVERSION_MILLILITRES_OUNCES))
                                        : viewModel.MillilitresConsumed;

            return new FeedActivity
            {
                Id = viewModel.Id,
                StartTime = viewModel.StartDate.ToUniversalTime(),
                EndTime = viewModel.EndTime.HasValue
                            ? viewModel.EndTime.Value.ToUniversalTime()
                            : (DateTimeOffset?)null,
                AmountConsumed = millilitresConsumed,
                FeedingSide = viewModel.FeedingSide.HasValue ? (int)viewModel.FeedingSide.Value : (int?)null,
                Notes = viewModel.Notes,
                HowBabyFelt = (int?)viewModel.HowBabyFelt,
                HowParentFelt = (int?)viewModel.HowParentFelt,
                BabyId = viewModel.Baby.Id
            };
        }

        public static SleepActivity AsModel(this SleepActivityViewModel viewModel)
        {
            return new SleepActivity
            {
                Id = viewModel.Id,
                StartTime = viewModel.StartDate.ToUniversalTime(),
                EndTime = viewModel.EndTime.HasValue
                            ? viewModel.EndTime.Value.ToUniversalTime()
                            : (DateTimeOffset?)null,
                Notes = viewModel.Notes,
                HowBabyFelt = (int?)viewModel.HowBabyFelt,
                HowParentFelt = (int?)viewModel.HowParentFelt,
                BabyId = viewModel.Baby.Id
            };
        }

        public static ChangeActivity AsModel(this ChangeActivityViewModel viewModel)
        {
            return new ChangeActivity
            {
                Id = viewModel.Id,
                StartTime = viewModel.StartDate.ToUniversalTime(),
                EndTime = viewModel.EndTime.HasValue
                            ? viewModel.EndTime.Value.ToUniversalTime()
                            : (DateTimeOffset?)null,
                Notes = viewModel.Notes,
                HowBabyFelt = (int?)viewModel.HowBabyFelt,
                HowParentFelt = (int?)viewModel.HowParentFelt,
                NappiesUsed = viewModel.NappiesUsed,
                WipesUsed = viewModel.WipesUsed,
                BabyId = viewModel.Baby.Id
            };
        }

        public static Measurement AsModel(this MeasurementViewModel viewModel, bool viewModelUsesMetricUnits)
        {
            var length = viewModel.Length.HasValue
                            ? Math.Round(viewModelUsesMetricUnits
                                            ? viewModel.Length.Value
                                            : viewModel.Length.Value / Constants.UNITCONVERSION_CENTIMETRES_INCHES, 2)
                            : viewModel.Length;
            var weight = viewModel.Weight.HasValue
                            ? Math.Round(viewModelUsesMetricUnits
                                            ? viewModel.Weight.Value
                                            : viewModel.Weight.Value / Constants.UNITCONVERSION_KILOGRAMS_POUNDS, 2)
                            : viewModel.Weight;

            return new Measurement
            {
                Id = viewModel.Id,
                Length = length,
                Weight = weight,
                BabyId = viewModel.Baby.Id
            };
        }

        public static void From(this BabyViewModel thisViewModel, BabyViewModel otherViewModel)
        {
            thisViewModel.AdditionalName = otherViewModel.AdditionalName;
            thisViewModel.DateOfBirth = otherViewModel.DateOfBirth;
            thisViewModel.Family = otherViewModel.Family;
            thisViewModel.FamilyName = otherViewModel.FamilyName;
            thisViewModel.GivenName = otherViewModel.GivenName;
            thisViewModel.Id = otherViewModel.Id;
        }

        public static void From(this FeedActivityViewModel thisViewModel, FeedActivityViewModel otherViewModel)
        {
            thisViewModel.Baby = otherViewModel.Baby;
            thisViewModel.EndTime = otherViewModel.EndTime;
            thisViewModel.HowBabyFelt = otherViewModel.HowBabyFelt;
            thisViewModel.HowParentFelt = otherViewModel.HowParentFelt;
            thisViewModel.Id = otherViewModel.Id;
            thisViewModel.MillilitresConsumed = otherViewModel.MillilitresConsumed;
            thisViewModel.Notes = otherViewModel.Notes;
            thisViewModel.StartDate = otherViewModel.StartDate;
        }

        public static void From(this SleepActivityViewModel thisViewModel, SleepActivityViewModel otherViewModel)
        {
            thisViewModel.Baby = otherViewModel.Baby;
            thisViewModel.EndTime = otherViewModel.EndTime;
            thisViewModel.HowBabyFelt = otherViewModel.HowBabyFelt;
            thisViewModel.HowParentFelt = otherViewModel.HowParentFelt;
            thisViewModel.Id = otherViewModel.Id;
            thisViewModel.Notes = otherViewModel.Notes;
            thisViewModel.StartDate = otherViewModel.StartDate;
        }

        public static void From(this ChangeActivityViewModel thisViewModel, ChangeActivityViewModel otherViewModel)
        {
            thisViewModel.Baby = otherViewModel.Baby;
            thisViewModel.EndTime = otherViewModel.EndTime;
            thisViewModel.HowBabyFelt = otherViewModel.HowBabyFelt;
            thisViewModel.HowParentFelt = otherViewModel.HowParentFelt;
            thisViewModel.Id = otherViewModel.Id;
            thisViewModel.Notes = otherViewModel.Notes;
            thisViewModel.StartDate = otherViewModel.StartDate;
            thisViewModel.NappiesUsed = otherViewModel.NappiesUsed;
            thisViewModel.WipesUsed = otherViewModel.WipesUsed;
        }

        public static void From(this MeasurementViewModel thisViewModel, MeasurementViewModel otherViewModel)
        {
            thisViewModel.Baby = otherViewModel.Baby;
            thisViewModel.CreatedAt = otherViewModel.CreatedAt;
            thisViewModel.Id = otherViewModel.Id;
            thisViewModel.Length = otherViewModel.Length;
            thisViewModel.Weight = otherViewModel.Weight;
        }
    }
}