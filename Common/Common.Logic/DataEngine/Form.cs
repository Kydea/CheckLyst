using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.DataEngine
{
    public class Form
    {
        public Dictionary<string, Field> Fields { get; } = new Dictionary<string, Field>();

        public List<ValidationMessage> ValidationMessages { get; } = new List<ValidationMessage>();
    }
}
