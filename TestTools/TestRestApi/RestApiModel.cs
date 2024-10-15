using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestRestApi
{
    public class RestApiModel
    {
        public string? Url { get; set; }
        public required List<Step> Steps { get; set; }
    }

    public class Step
    {
        public bool Iterative { get; set; } = false;
        public required List<Command> Commands { get; set; }
    }

    public class Command
    {
        public string? Path { get; set; }
        public string Method { get; set; } = "POST";
        public Dictionary<string, string> Header { get; set; } = new Dictionary<string, string>();
        public dynamic? Request { get; set; }
    }
}
