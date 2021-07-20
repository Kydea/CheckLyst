using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.ExternalSources.Logic
{
    public abstract class SourceBase : IDisposable
    {
        public const int PAGE_SIZE = 20;
        public abstract void Parse(string Data);

        public abstract void Open();

        public abstract void Dispose();

        public abstract void PrepareMetaData();
        public abstract IEnumerable<List<string>> GetData(Dictionary<string, string> Parameters, int Page, string DecimalSeparator, string TimeZone);

        protected string Code { get; private set; }

        protected string Password { get; private set; }

        protected DateTimeOffset Version { get; private set; }

        public List<string> Columns { get; protected set; }

        public List<Parameter> Parameters { get; protected set; }

        public static SourceBase Build(string Class, string Code, string Password, DateTimeOffset Version, string Data)
        {
            var SourceType = Type.GetType($"CheckListManager.ExternalSources.Logic.{Class}.Source");
            if (SourceType == null || SourceType.IsAssignableFrom(typeof(SourceBase)))
                throw new InvalidOperationException();
            var SourceBase = (SourceBase)Activator.CreateInstance(SourceType);
            SourceBase.Code = Code;
            SourceBase.Password = Password;
            SourceBase.Version = Version;
            SourceBase.Parse(Data);
            return SourceBase;
        }

        public static string Format(decimal Number, string Separator)
        {
            return Number.ToString("0.#####").Replace(",", Separator).Replace(".", Separator);
        }

        public static string Format(DateTime DateTime)
        {
            return DateTime.ToString("yyyy-MM-dd HH:mm");
        }

        public static string Format(TimeSpan Time)
        {
            return Time.ToString("hh\\:mm");
        }

        public static string Format(DateTimeOffset DateTimeOffset, string TimeZone)
        {
            if (TimeZone == null) throw new ArgumentNullException();
            return TimeZoneInfo.ConvertTime(DateTimeOffset, TimeZoneInfo.FindSystemTimeZoneById(TimeZone)).ToString("yyyy-MM-dd HH:mm zzz");
        }
    }
}
