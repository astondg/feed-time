namespace FeedTime.Common.Extensions
{
    using System;
    using Windows.ApplicationModel;

    public static class PackageExtensions
    {
        public static string GetAppVersionNumber(this Package package)
        {
            return String.Format("{0}.{1}.{2}.{3}",
                                 package.Id.Version.Build,
                                 package.Id.Version.Major,
                                 package.Id.Version.Minor,
                                 package.Id.Version.Revision);
        }
    }
}