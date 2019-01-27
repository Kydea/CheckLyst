using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace Matematici
{
    public static class StringExtensions
    {
        public static String RemoveAccent(this String Phrase)
        {
            return System.Text.Encoding.ASCII.GetString(System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(Phrase));
        }

        public static String Slugify(this String Phrase)
        {
            if (Phrase == null) return String.Empty;

            string Result = Phrase.RemoveAccent().ToLower();
            Result = Regex.Replace(Result, @"[^a-z0-9\s-]", "");
            Result = Regex.Replace(Result, @"\s+", " ").Trim();
            Result = Result.Substring(0, Result.Length <= 45 ? Result.Length : 45).Trim();
            Result = Regex.Replace(Result, @"\s", "-"); 
            return Result;
        }

        public static String ViewName(this String Name)
        {
            return (Name ?? "").Replace("-", String.Empty);
        }
    }
}