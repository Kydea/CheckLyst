using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CheckListManager.ExternalSources.Logic.REST
{
    public class Model
    {
        public string Url { get; set; }
        public List<Command> Commands { get; set; } = new List<Command>();
        public List<string> Columns { get; set; } = new List<string>();
        public List<Row> Rows { get; set; } = new List<Row>();
        public string ParameterLeftDelimiter { get; set; } = "{";
        public string ParameterRightDelimiter { get; set; } = "}";

        private Dictionary<string, Command> NamedCommands = new Dictionary<string, Command>();

        public void Validate()
        {
            var commandsGroup = Commands.Where(c => c.Name != null).GroupBy(c => c.Name);
            var duplicatedCommand = commandsGroup.FirstOrDefault(g => g.Count() > 1);
            if (duplicatedCommand != null)
                throw new ParseException($"Command name duplicated: {duplicatedCommand.First().Name}");
            NamedCommands = commandsGroup.ToDictionary(g => g.Key ?? "", v => v.Single());

            var commandIndex = 0;
            Commands.ForEach(c => c.Validate(this, commandIndex++));

            var rowIndex = 0;
            Rows.ForEach(r => r.Validate(this, rowIndex++));
        }

        public Command GetCommand(string name) => NamedCommands.TryGetValue(name, out var command) ? command : null;
        public bool TryGetCommand(string name, out Command command) => NamedCommands.TryGetValue(name, out command);
    }

    public class Command
    {
        private static readonly string[] Methods = new string[] { "GET", "POST", "PUT", "DELETE", "HEAD", "OPTIONS" };

        [JsonIgnore]
        public int Index { get; set; }
        public string Name { get; set; }
        public string Method { get; set; } = "POST";
        public string Path { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public dynamic JsonContent { get; set; }
        public Dictionary<string, string> FormParameters { get; set; }
        public List<Rule> Rules { get; set; } = new List<Rule>();

        [JsonIgnore]
        public string Reference => Name ?? $"#{Index}";


        public HttpMethod GetHttpMethod()
        {
            switch (Method.ToUpper())
            {
                case "GET": return HttpMethod.Get;
                case "POST": return HttpMethod.Post;
                case "PUT": return HttpMethod.Put;
                case "DELETE": return HttpMethod.Delete;
                case "HEAD": return HttpMethod.Head;
                case "OPTIONS": return HttpMethod.Options;
                default: throw new ArgumentException("Invalid HTTP method");
            };
        }

        public void Validate(Model model, int Index)
        {
            this.Index = Index;
            Method = Method.ToUpper();
            if (!Methods.Contains(Method))
                throw new ParseException($"Invalid method: {Method}, for command: ${Reference}");

            Rules.ForEach(r => r.Validate(model, this));
        }
    }

    public class Rule
    {
        private static readonly string[] NumberSigns = new string[] { "=", "!=", "<", ">", "<=", ">=" };
        private static readonly string[] TextSigns = new string[] { "=", "!=", "contains", "not contains", "starts with", "not starts with", "ends with", "not ends with" };
        private static readonly string[] Actions = new string[] { "goto", "error" };

        public string Path { get; set; }
        public string Sign { get; set; }
        public decimal? NumberValue { get; set; }
        public string TextValue { get; set; }

        public string Action { get; set; }
        public string CommandName { get; set; }
        public int? CommandIndex { get; set; }
        public string Error { get; set; }

        public void Validate(Model model, Command command)
        {
            if (!Actions.Contains(Action))
                throw new ParseException($"Invalid action: {Action}, for command: ${command.Reference}");

            var valuesCount =
                (NumberValue.HasValue ? 1 : 0) +
                (TextValue != null ? 1 : 0);

            if (valuesCount == 0) throw new ParseException($"No value found, for action: {Action}, for command: {command.Reference}");
            if (valuesCount > 1) throw new ParseException($"More values found, for action: {Action}, for command: {command.Reference}");
            if (NumberValue.HasValue && !NumberSigns.Contains(Sign)) throw new ParseException($"Invalid number sign: {Sign}, for command: ${command.Reference}");
            if (TextValue != null && !TextSigns.Contains(Sign)) throw new ParseException($"Invalid text sign: {Sign}, for command: ${command.Reference}");

            switch (Action)
            {
                case "goto":
                    if (CommandName != null && CommandIndex.HasValue)
                        throw new ParseException($"A rule cannot have both a CommandName and a CommandIndex, for action: {Action}, for command: {command.Reference}");

                    if (CommandName == null && !CommandIndex.HasValue)
                        throw new ParseException($"A rule must have a CommandName or a CommandIndex, for action: {Action}, for command: {command.Reference}");

                    if (CommandIndex.HasValue && model.Commands.Count <= CommandIndex.Value)
                        throw new ParseException($"Invalid CommandIndex: {CommandIndex}, for action: {Action}, for command: {command.Reference}");

                    if (CommandName != null)
                    {
                        if (!model.TryGetCommand(CommandName, out var refencedCommand))
                            throw new ParseException($"Invalid CommandName: {CommandName}, for action: {Action}, for command: {command.Reference}");

                        CommandIndex = refencedCommand.Index;
                    }
                    break;

                case "error":
                    if (Error == null)
                        throw new ParseException($"Provide an error, for action: {Action}, for command: {command.Reference}");
                    break;
            }
        }

        public bool Test(string value)
        {
            if (NumberValue.HasValue)
                return
                    decimal.TryParse(value, out var numberValue)
                    &&
                    (
                        Sign == "=" && NumberValue == numberValue
                        || Sign == "!=" && NumberValue != numberValue
                        || Sign == "<" && NumberValue < numberValue
                        || Sign == ">" && NumberValue > numberValue
                        || Sign == "<=" && NumberValue <= numberValue
                        || Sign == ">=" && NumberValue >= numberValue
                    );
            else if (TextValue != null)
                return
                    Sign == "=" && TextValue == value
                    || Sign == "!=" && TextValue != value
                    || Sign == "contains" && TextValue.Contains(value)
                    || Sign == "not contains" && !TextValue.Contains(value)
                    || Sign == "starts with" && TextValue.StartsWith(value)
                    || Sign == "not starts with" && !TextValue.StartsWith(value)
                    || Sign == "ends with" && TextValue.EndsWith(value)
                    || Sign == "not ends with" && !TextValue.EndsWith(value);
            throw new ArgumentOutOfRangeException(nameof(value));
        }
    }

    public class Row
    {
        [JsonIgnore]
        public int Index { get; set; }
        public string CommandName { get; set; }
        public int? CommandIndex { get; set; }
        public string RowsPath { get; set; }
        public List<RowValue> Values { get; set; } = new List<RowValue>();

        public void Validate(Model model, int index)
        {
            Index = index;

            if (CommandName != null && CommandIndex.HasValue)
                throw new ParseException($"A row cannot have both a CommandName and a CommandIndex, for row {index}");

            if (CommandName == null && !CommandIndex.HasValue)
                throw new ParseException($"A row must have a CommandName or a CommandIndex, for row {index}");

            if (CommandIndex.HasValue && model.Commands.Count <= CommandIndex.Value)
                throw new ParseException($"Invalid CommandIndex: {CommandIndex}, for row {index}");

            if (CommandName != null)
            {
                if (!model.TryGetCommand(CommandName, out var command))
                    throw new ParseException($"Invalid CommandName: {CommandName}, for row {index}");

                CommandIndex = command.Index;
            }

            foreach (var value in Values)
                value.Validate(model, index);
        }
    }

    public class RowValue
    {
        public int? Column { get; set; }
        public string RelativePath { get; set; }
        public string AbsolutePath { get; set; }

        public void Validate(Model model, int index)
        {
            if (!Column.HasValue || Column < 0 || Column >= model.Columns.Count)
                throw new ParseException($"Invalid Column: {Column}, for row {index}");

            if (RelativePath != null && AbsolutePath != null)
                throw new ParseException($"A value cannot have both a RelativePath and an AbsolutePath, for row {index}");

            if (RelativePath == null && AbsolutePath == null)
                throw new ParseException($"A value must have a RelativePath or an AbsolutePath, for row {index}");
        }
    }

    public class ParseException : Exception
    {
        public ParseException(string Message) : base(Message) { }
    }
}
