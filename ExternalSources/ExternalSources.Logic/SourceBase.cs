using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.ExternalSources.Logic
{
    public abstract class SourceBase : IDisposable
    {
        public abstract void Parse(string Data);

        public abstract void Open();

        public abstract void Dispose();

        public abstract void Test();

        protected string Code { get; private set; }

        protected string Password { get; private set; }

        protected DateTimeOffset Version { get; private set; }

        public List<string> Columns { get; protected set; }

        public List<Parameter> Parameters { get; protected set; }

        public static SourceBase Build(string Class, string Code, string Password, DateTimeOffset Version, string Data)
        {
            var SourceType = Type.GetType($"CheckListManager.ExternalSources.Logic.{Class}.Source");
            if (SourceType.IsAssignableFrom(typeof(SourceBase)))
                throw new InvalidOperationException();
            var SourceBase = (SourceBase)Activator.CreateInstance(SourceType);
            SourceBase.Code = Code;
            SourceBase.Password = Password;
            SourceBase.Version = Version;
            SourceBase.Parse(Data);
            return SourceBase;
        }
    }
}
