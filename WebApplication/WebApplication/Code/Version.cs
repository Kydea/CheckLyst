using System;

namespace CheckListManager.WebApplication.Code
{
    public static class Version
    {
        private static string Cache;

        public static string Get()
        {
            if (Cache == null)
            {
                var Version = typeof(MvcApplication).Assembly.GetName().Version;
                Cache = String.Format("{0}.{1}.{2}.{3}", Version.Major, Version.Minor, Version.Revision, Version.Build);
            }
            return Cache;
        }
    }
}