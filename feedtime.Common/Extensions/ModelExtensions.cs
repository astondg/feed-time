namespace FeedTime.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using FeedTime.DataModel;
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
            return new BabyViewModel
            {
                Id = model.Id,
                GivenName = model.GivenName,
                AdditionalName = model.AdditionalName,
                FamilyName = model.FamilyName,
                DateOfBirth = model.DateOfBirth.Date,
                TimeOfBirth = model.DateOfBirth.TimeOfDay,
                Gender = model.Gender,
                Family = family != null ? family.AsViewModel() : new FamilyViewModel { Id = model.FamilyId }
            };
        }

        public static FeedActivityViewModel AsViewModel(this FeedActivity model, Baby baby = null)
        {
            return new FeedActivityViewModel
            {
                Id = model.Id,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Duration = model.EndTime.HasValue
                            ? Convert.ToInt32((model.StartTime - model.EndTime.Value).TotalMinutes) : 0,
                MillilitresConsumed = model.MillilitresConsumed,
                Notes = model.Notes,
                HowBabyFelt = model.HowBabyFelt,
                HowParentFelt = model.HowParentFelt,
                Baby = baby != null ? baby.AsViewModel() : new BabyViewModel { Id = model.BabyId }
            };
        }

        public static SleepActivityViewModel AsViewModel(this SleepActivity model, Baby baby = null)
        {
            return new SleepActivityViewModel
            {
                Id = model.Id,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Duration = model.EndTime.HasValue
                            ? Convert.ToInt32((model.StartTime - model.EndTime.Value).TotalMinutes) : 0,
                Notes = model.Notes,
                HowBabyFelt = model.HowBabyFelt,
                HowParentFelt = model.HowParentFelt,
                Baby = baby != null ? baby.AsViewModel() : new BabyViewModel { Id = model.BabyId }
            };
        }

        public static MeasurementViewModel AsViewModel(this Measurement model, Baby baby = null)
        {
            return new MeasurementViewModel
            {
                Id = model.Id,
                CreatedAt = model.CreatedAt,
                Length = model.Length,
                Weight = model.Weight,
                Baby = baby != null ? baby.AsViewModel() : new BabyViewModel { Id = model.BabyId }
            };
        }
    }
}
