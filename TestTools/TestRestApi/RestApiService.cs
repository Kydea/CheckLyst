using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.Policy;
using System.Text;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace TestRestApi
{
    public class RestApiService(RestApiModel Model, Dictionary<string, string> Parameters)
    {
        private readonly static int OperationLimit = 100;
        private readonly List<JToken?> Responses = [];
        private JToken? LastResponse = null;

        public async Task<List<List<string?>>> RunAsync()
        {
            Command? command = null;
            int commandIndex = 0;
            int operationIndex = 0;
            List<List<string?>> rows = [];
            try
            {
                using var client = new HttpClient();
                Responses.AddRange(Model.Commands.Select(c => default(JToken?)));
                Main.Instance?.LogTextBox.Clear();
                while (commandIndex < Model.Commands.Count)
                {
                    command = Model.Commands[commandIndex];
                    Main.Instance?.LogTextBox.AppendText($"--- command {commandIndex}, operation {operationIndex} ---{Environment.NewLine}{Environment.NewLine}");

                    var url = (Model.Url ?? string.Empty) + ParseParameters(command.Path);

                    Main.Instance?.LogTextBox.AppendText($"{command.Method} {url}{Environment.NewLine}{Environment.NewLine}body:{Environment.NewLine}");
                    var request = new HttpRequestMessage(command.GetHttpMethod(), url);

                    if (command.FormParameters != null)
                    {
                        var formParameters = command.FormParameters.Select(p => new KeyValuePair<string, string>(p.Key, ParseParameters(p.Value)));
                        request.Content = new FormUrlEncodedContent(formParameters);
                        foreach(var formParameter in formParameters)
                        Main.Instance?.LogTextBox.AppendText($"\"{formParameter.Key}\": \"{formParameter.Value}\"{Environment.NewLine}");

                    }
                    else if (command.JsonContent != null)
                    {
                        IterateJToken(command.JsonContent);
                        request.Content = new StringContent(JsonConvert.SerializeObject(command.JsonContent), Encoding.UTF8, "application/json");
                        Main.Instance?.LogTextBox.AppendText($"{command.JsonContent}{Environment.NewLine}");
                    }

                    Main.Instance?.LogTextBox.AppendText($"{Environment.NewLine}");

                    if (command.Headers.Any())
                    {
                        Main.Instance?.LogTextBox.AppendText($"headers:{Environment.NewLine}");
                        foreach (var header in command.Headers)
                        {
                            var value = ParseParameters(header.Value);
                            request.Headers.Add(header.Key, value);
                            Main.Instance?.LogTextBox.AppendText($"\"{header.Key}\": \"{value}\"{Environment.NewLine}");
                        }
                        Main.Instance?.LogTextBox.AppendText(Environment.NewLine);
                    }

                    var response = await client.SendAsync(request);
                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (HttpRequestException exception)
                    {
                        Main.Instance?.LogTextBox.AppendText($"Errore HTTP: {exception.Message}\n");
                        Main.Instance?.LogTextBox.AppendText($"Status Code: {(int)response.StatusCode} - {response.StatusCode}\n");
                        Main.Instance?.LogTextBox.AppendText($"Response Headers: {response.Headers}\n");

                        try
                        {
                            string errorContent = await response.Content.ReadAsStringAsync();
                            Main.Instance?.LogTextBox.AppendText($"Response Body: {errorContent}");
                        }
                        catch { }

                        throw;
                    }

                    LastResponse = Responses[commandIndex++] = JToken.Parse(await response.Content.ReadAsStringAsync());

                    Main.Instance?.LogTextBox.AppendText($"response:{Environment.NewLine}{LastResponse}{Environment.NewLine}{Environment.NewLine}");

                    if (command.Rules != null)
                        foreach (var rule in command.Rules)
                        {
                            var pathValue = ParseParameters(rule.Path);
                            if (rule.Test(pathValue.ToString() ?? ""))
                                commandIndex = rule.Action switch
                                {
                                    "goto" => rule.CommandIndex!.Value,
                                    "error" => throw new Exception(rule.TextValue),
                                    _ => throw new ArgumentException(nameof(rule.Action)),
                                };
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
                                        result[value.Column!.Value] = GetJsonValue(item, value.ValuePath) ?? "";

                                    rows.Add(result!);
                                }
                        }

                    Main.Instance?.LogTextBox.AppendText($"{Environment.NewLine}");

                    if (++operationIndex > OperationLimit)
                        throw new Exception($"Too many operations: {OperationLimit}");
                }

                return rows;
            }
            catch (Exception exception)
            {
                if (command == null)
                    throw;
                else
                    throw new Exception($"Error, command: {command.Reference}, operation number: {operationIndex}, message: {exception.Message}", exception);
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

        private string ParseParameters(string? value)
        {
            string result = string.Empty;
            if (value != null)
            {
                int caret = 0;
                int start;
                while ((start = value.IndexOf(Model.ParameterLeftDelimiter, caret)) != -1)
                {
                    result += value[caret..start];
                    caret = start + Model.ParameterLeftDelimiter.Length;
                    int end = value.IndexOf(Model.ParameterRightDelimiter, caret);
                    if (end == -1)
                    {
                        caret = start;
                        break;
                    }

                    var parameterValue = ParseParameter(value[caret..end]);
                    result += parameterValue;
                    caret = end + Model.ParameterRightDelimiter.Length;
                }
                result += value[caret..];
            }

            return result;
        }

        private JToken? ParsePath(string value)
        {
            if (value == "response")
                return LastResponse;

            if (value.StartsWith("response."))
                return GetJsonPath(LastResponse, value[9..]);


            if (value.StartsWith("commands["))
            {
                var end = value.IndexOf(']', 9);
                if (end == -1 || (end < value.Length - 1 && value[^1] != '.') || !int.TryParse(value[9..end], out int reponseIndex) || reponseIndex >= Responses.Count) return value;
                return
                    end == value.Length ?
                        Responses[reponseIndex] :
                        GetJsonPath(Responses[reponseIndex], value[(end + 2)..]);
            }
                
            return null;
        }

        private string ParseParameter(string value)
        {
            if (value == "response")
                return LastResponse?.ToString() ?? string.Empty;

            if (value.StartsWith("response."))
                return GetJsonValue(LastResponse, value[9..]) ?? value;

            if (value.StartsWith("commands["))
            {
                var end = value.IndexOf(']', 9);
                if (end == -1 || (end < value.Length - 1 && value[^1] != '.') || !int.TryParse(value[9..end], out int reponseIndex) || reponseIndex >= Responses.Count) return value;
                return
                    end == value.Length ?
                        Responses[reponseIndex]?.ToString() ?? string.Empty :
                        GetJsonValue(Responses[reponseIndex], value[(end + 2)..]) ?? value;
            }

            if (Parameters.TryGetValue(value, out var parameterValue))
                return parameterValue;

            return value;
        }

        private static string? GetJsonValue(JToken? jToken, string path) =>
            GetJsonPath(jToken, path) is JValue value ? value.ToString() : null;

        private static JToken? GetJsonPath(JToken? jToken, string path)
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