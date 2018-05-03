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

        public List<string> Columns { get; protected set; }

        public List<Parameter> Parameters { get; protected set; }

        public static SourceBase Build(string Class)
        {
            var SourceType = Type.GetType($"CheckListManager.ExternalSources.Logic.{Class}.Source");
            if (SourceType.IsAssignableFrom(typeof(SourceBase)))
                throw new InvalidOperationException();
            return (SourceBase)Activator.CreateInstance(SourceType);
        }
    }
}
