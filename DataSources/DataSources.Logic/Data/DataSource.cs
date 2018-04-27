using System;
using System.Collections.Generic;

namespace CheckListManager.DataSources.Logic.Data
{
    public class DataSource
    {
        public DataSource(string Class = null)
        {
            this.Class = Class;
        }

        private string Class { get; }

        private SourceBase Source { get; set; }

        public string Code { get; set; }

        public List<string> Columns { get => Source?.Columns ?? new List<string>(0); }

        public string Data
        {
            set
            {
                if (Class != null)
                {
                    var SourceType = Type.GetType("CheckListManager.DataSources.Logic.Data.Sources." + Class);
                    if (SourceType.IsAssignableFrom(typeof(SourceBase)))
                        throw new InvalidOperationException();

                    Source = (SourceBase)Activator.CreateInstance(SourceType);
                    Source.Parse(value);
                }
            }
        }
    }
}
