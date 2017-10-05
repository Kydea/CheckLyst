using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.Common.Logic.DataEngine
{
    public class FormResponse
    {
        public Dictionary<string, Dictionary<string, string>> FieldsValues { get; } = new Dictionary<string, Dictionary<string, string>>();

        public List<ValidationMessage> ValidationMessages { get; } = new List<ValidationMessage>();
    }
}
