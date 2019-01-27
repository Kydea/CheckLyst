using System;
using System.Diagnostics;

namespace CheckListManager.WebApplication.Code
{
    public static class Version
    {
        private static string Cache;

        public static string Get()
        {
            if (Cache == null)
            {
                var assembly = typeof(MvcApplication).Assembly;
                var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                Cache = string.Format(versionInfo.FileVersion);
            }
            return Cache;
        }
    }
}