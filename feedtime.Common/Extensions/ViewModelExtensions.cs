namespace FeedTime.Extensions
{
    using System;
    using FeedTime.DataModel;
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
                DateOfBirth = viewModel.DateOfBirth.Add(viewModel.TimeOfBirth),
                Gender = viewModel.Gender,
                FamilyId = viewModel.Family.Id
            };
        }

        public static FeedActivity AsModel(this FeedActivityViewModel viewModel)
        {
            return new FeedActivity
            {
                Id = viewModel.Id,
                StartTime = viewModel.StartTime,
                EndTime = viewModel.EndTime,
                MillilitresConsumed = viewModel.MillilitresConsumed,
                Notes = viewModel.Notes,
                HowBabyFelt = viewModel.HowBabyFelt,
                HowParentFelt = viewModel.HowParentFelt,
                BabyId = viewModel.Baby.Id
            };
        }

        public static SleepActivity AsModel(this SleepActivityViewModel viewModel)
        {
            return new SleepActivity
            {
                Id = viewModel.Id,
                StartTime = viewModel.StartTime,
                EndTime = viewModel.EndTime,
                Notes = viewModel.Notes,
                HowBabyFelt = viewModel.HowBabyFelt,
                HowParentFelt = viewModel.HowParentFelt,
                BabyId = viewModel.Baby.Id
            };
        }

        public static Measurement AsModel(this MeasurementViewModel viewModel)
        {
            return new Measurement
            {
                Id = viewModel.Id,
                Length = viewModel.Length,
                Weight = viewModel.Weight,
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
            thisViewModel.StartTime = otherViewModel.StartTime;
        }

        public static void From(this SleepActivityViewModel thisViewModel, SleepActivityViewModel otherViewModel)
        {
            thisViewModel.Baby = otherViewModel.Baby;
            thisViewModel.EndTime = otherViewModel.EndTime;
            thisViewModel.HowBabyFelt = otherViewModel.HowBabyFelt;
            thisViewModel.HowParentFelt = otherViewModel.HowParentFelt;
            thisViewModel.Id = otherViewModel.Id;
            thisViewModel.Notes = otherViewModel.Notes;
            thisViewModel.StartTime = otherViewModel.StartTime;
        }
    }
}