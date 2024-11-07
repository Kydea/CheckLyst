using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Globalization;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;

namespace CheckListManager.ExternalSources.Logic.REST
{
    public class Source : SourceBase
    {
        private const int OPERATION_LIMIT = 100;
        private RESTParameters RESTParameters { get; set; }
        private Model Model { get; set; }
        private readonly List<JToken> Responses = new List<JToken>();
        private JToken LastResponse = null;
        private Dictionary<string, string> ParameterValues;

        public override void Dispose()
        {
        }

        public override void Open()
        {
            
        }

        public override void Parse(string Data)
        {
            RESTParameters = JsonConvert.DeserializeObject<RESTParameters>(Data);
            Parameters = RESTParameters.QueryParameters ?? new List<Parameter>(0);
            Model = JsonConvert.DeserializeObject<Model>(RESTParameters.JSON);
            Model.Validate();
        }

        public override void PrepareMetaData()
        {
            OperationTime = 0;
            Columns = Model.Columns;
        }

        public override async Task<IEnumerable<List<string>>> GetDataAsync(Dictionary<string, string> Parameters, int Page, string DecimalSeparator, string TimeZone)
        {
            var stopWatch = Stopwatch.StartNew();
            Command command = null;
            int commandIndex = 0;
            int operationIndex = 0;
            int rowCount = 0;
            List<List<string>> rows = new List<List<string>>();
            ParameterValues = Parameters;
            try
            {
                using (var client = new HttpClient())
                {
                    Responses.AddRange(Model.Commands.Select(c => default(JToken)));
                    while (commandIndex < Model.Commands.Count)
                    {
                        command = Model.Commands[commandIndex];
                        var url = (Model.Url ?? string.Empty) + ParseParameters(command.Path);
                        var request = new HttpRequestMessage(command.GetHttpMethod(), url);

                        if (command.FormParameters != null)
                        {
                            var formParameters = command.FormParameters.Select(p => new KeyValuePair<string, string>(p.Key, ParseParameters(p.Value)));
                            request.Content = new FormUrlEncodedContent(formParameters);

                        }
                        else if (command.JsonContent != null)
                        {
                            IterateJToken(command.JsonContent);
                            request.Content = new StringContent(JsonConvert.SerializeObject(command.JsonContent), Encoding.UTF8, "application/json");
                        }

                        if (command.Headers.Any())
                            foreach (var header in command.Headers)
                            {
                                var value = ParseParameters(header.Value);
                                request.Headers.Add(header.Key, value);
                            }

                        var response = await client.SendAsync(request);
                        try
                        {
                            response.EnsureSuccessStatusCode();
                        }
                        catch (HttpRequestException exception)
                        {
                            try
                            {
                                string errorContent = await response.Content.ReadAsStringAsync();
                                throw new Exception($"Response Body: {errorContent}", exception);
                            }
                            catch { throw exception; }
                        }

                        LastResponse = Responses[commandIndex++] = JToken.Parse(await response.Content.ReadAsStringAsync());

                        if (command.Rules != null)
                            foreach (var rule in command.Rules)
                            {
                                var pathValue = ParseParameters(rule.Path);
                                if (rule.Test(pathValue.ToString() ?? ""))
                                {
                                    switch (rule.Action)
                                    {
                                        case "goto":
                                            commandIndex = rule.CommandIndex.Value;
                                            break;
                                        case "error":
                                            throw new Exception(rule.TextValue);
                                        default:
                                            throw new ArgumentException(nameof(rule.Action));
                                    }
                                }
                            }

                        foreach (var row in Model.Rows)
                            if (row.CommandIndex == command.Index)
                            {
                                var jToken = ParsePath(row.RowsPath);

                                var array =
                                    jToken is JArray jArray ? jArray :
                                    jToken is JObject record ? new JArray(record) :
                                    null;

                                if (array != null)
                                    foreach (var item in array)
                                    {
                                        var result = Model.Columns.Select(c => string.Empty).ToList();
                                        foreach (var value in row.Values)
                                            result[value.Column.Value] =
                                                value.AbsolutePath == null ?
                                                    GetJsonValue(item, value.RelativePath) ?? "" :
                                                    ParseParameter(value.AbsolutePath);

                                        if (rowCount >= PAGE_SIZE * Page && rowCount < PAGE_SIZE * (Page + 1))
                                            rows.Add(result);

                                        ++rowCount;

                                        if (rowCount == PAGE_SIZE * (Page + 1))
                                            return rows;
                                    }
                            }

                        if (++operationIndex > OPERATION_LIMIT)
                            throw new Exception($"Too many operations: {OPERATION_LIMIT}");
                    }

                    return rows;
                }
            }
            catch (Exception exception)
            {
                if (command == null)
                    throw;
                else
                    throw new Exception($"Error, command: {command.Reference}, operation number: {operationIndex}, message: {exception.Message}", exception);
            }
            finally
            {
                OperationTime = stopWatch.Elapsed.TotalMilliseconds;
            }
        }

        private void IterateJToken(JToken token)
        {
            if (token == null)
                return;

            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var property in token.Children<JProperty>())
                        IterateJToken(property.Value);
                    break;

                case JTokenType.Array:
                    foreach (var item in token.Children())
                        IterateJToken(item);
                    break;

                case JTokenType.String:
                    var value = (JValue)token;
                    value.Value = ParseParameters(value?.Value?.ToString());
                    break;
            }
        }

        private string ParseParameters(string value)
        {
            string result = string.Empty;
            if (value != null)
            {
                int caret = 0;
                int start;
                while ((start = value.IndexOf(Model.ParameterLeftDelimiter, caret)) != -1)
                {
                    result += value.Substring(caret, start - caret);
                    caret = start + Model.ParameterLeftDelimiter.Length;
                    int end = value.IndexOf(Model.ParameterRightDelimiter, caret);
                    if (end == -1)
                    {
                        caret = start;
                        break;
                    }

                    var parameterValue = ParseParameter(value.Substring(caret, end - caret));
                    result += parameterValue;
                    caret = end + Model.ParameterRightDelimiter.Length;
                }
                result += value.Substring(caret);
            }

            return result;
        }

        private JToken ParsePath(string value)
        {
            if (value == "response")
                return LastResponse;

            if (value.StartsWith("response."))
                return GetJsonPath(LastResponse, value.Substring(9));


            if (value.StartsWith("commands["))
            {
                var end = value.IndexOf(']', 9);
                if (end == -1 || (end < value.Length - 1 && value[end + 1] != '.') || !int.TryParse(value.Substring(9, end - 9), out int reponseIndex) || reponseIndex >= Responses.Count) return value;
                return
                    end == value.Length ?
                        Responses[reponseIndex] :
                        GetJsonPath(Responses[reponseIndex], value.Substring(end + 2));
            }

            return null;
        }

        private string ParseParameter(string value)
        {
            if (value == "response")
                return LastResponse?.ToString() ?? string.Empty;

            if (value.StartsWith("response."))
                return GetJsonValue(LastResponse, value.Substring(9)) ?? value;

            if (value.StartsWith("commands["))
            {
                var end = value.IndexOf(']', 9);
                if (end == -1 || (end < value.Length - 1 && value[end + 1] != '.') || !int.TryParse(value.Substring(9, end - 9), out int reponseIndex) || reponseIndex >= Responses.Count) return value;
                return
                    end == value.Length ?
                        Responses[reponseIndex]?.ToString() ?? string.Empty :
                        GetJsonValue(Responses[reponseIndex], value.Substring(end + 2)) ?? value;
            }

            if (ParameterValues.TryGetValue(value, out var parameterValue))
                return parameterValue;

            if (value == "password") return Password;

            return value;
        }

        private static string GetJsonValue(JToken jToken, string path) =>
            GetJsonPath(jToken, path) is JValue value ? value.ToString() : null;

        private static JToken GetJsonPath(JToken jToken, string path)
        {
            var pathTokens = path.Replace("[", ".[").Split('.');
            foreach (var partToken in pathTokens)
            {
                if (jToken is JObject record)
                {
                    if (int.TryParse(partToken.Trim('[', ']'), out int index))
                    {
                        var dictionary = record.ToObject<Dictionary<int, dynamic>>();
                        jToken = dictionary[dictionary.Keys.ToList()[index]];
                    }
                    else
                        jToken = record[partToken];
                }
                else if (jToken is JArray array && int.TryParse(partToken.Trim('[', ']'), out int index))
                    jToken = array[index];
                else
                    return null;
            }
            return jToken;
        }
    }
}
