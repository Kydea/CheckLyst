using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.DataEngine
{
    public class Field
    {
        public string Value { get; set; }

        public bool Editable { get; set; } = true;

        public bool Visible { get; set; } = true;

        public IEnumerable<Option> Options { get; set; } = new List<Option>();

        public List<ValidationMessage> ValidationMessages { get; set; } = new List<ValidationMessage>();
    }
}
