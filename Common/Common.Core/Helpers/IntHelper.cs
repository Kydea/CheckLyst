using System;

namespace CheckListManager.Common.Core.Helpers
{
    public static class IntHelper
    {
        private static readonly string[] Suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        
        public static string BytesCountToString(this int byteCount)
        {
            if (byteCount == 0)
                return "0" + Suffixes[0];
            
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            
            return $"{Math.Sign(byteCount) * num}{Suffixes[place]}";
        }
    }
}