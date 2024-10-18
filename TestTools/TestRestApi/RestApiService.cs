using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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

        public async Task RunAsync()
        {
            Command? command = null;
            int commandIndex = 0;
            int operationIndex = 0;
            try
            {
                using var client = new HttpClient();
                Responses.Clear();
                Main.Instance?.LogTextBox.Clear();
                while (commandIndex < Model.Commands.Count)
                {
                    command = Model.Commands[commandIndex];
                    var request = new HttpRequestMessage(command.GetHttpMethod(), (Model.Url ?? string.Empty) + (command.Path ?? string.Empty));
                    if (command.FormParameters != null)
                        request.Content = new FormUrlEncodedContent(command.FormParameters.Select(p => new KeyValuePair<string, string>(p.Key, ParseParameters(p.Value))));
                    else if (command.JsonContent != null)
                    {
                        IterateJToken(command.JsonContent);
                        request.Content = new StringContent(JsonConvert.SerializeObject(command.JsonContent), Encoding.UTF8, "application/json");
                    }

                    foreach (var header in command.Headers)
                        request.Headers.Add(header.Key, ParseParameters(header.Value));

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

                    Responses[commandIndex++] = JToken.Parse(await response.Content.ReadAsStringAsync());

                    if (command.Rules != null)
                        foreach (var rule in command.Rules)
                        {
                            var pathValue = ParseParameter(rule.Path);
                            if (rule.Test(pathValue))
                                switch (rule.Action)
                                {
                                    case "goto":
                                        commandIndex = rule.CommandIndex!.Value;
                                        break;
                                    case "error":
                                        throw new Exception(rule.TextValue);
                                    default: 
                                        throw new ArgumentException(nameof(rule.Action));
                                }
                        }

                    foreach(var row in Model.Rows)
                        if (row.CommandIndex == command.Index)
                        {

                        }

                    if (++operationIndex > OperationLimit)
                        throw new Exception($"Too many operations: {OperationLimit}");
                }
            }
            catch (Exception exception)
            {
                if (command == null)
                    throw;
                else
                    throw new Exception($"Error in command ${command.Reference} on operation number {operationIndex}: {exception.Message}", exception);
            }

            try
            {

            }
            catch (Exception exception)
            {
                throw new Exception($"Error mapping results: {exception.Message}", exception);
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
                    Console.WriteLine($"Value: {token}");
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

        private string ParseParameter(string value)
        {
            try
            {
                if (value.StartsWith("commands["))
                {
                    var end = value.IndexOf("].", 9);
                    if (end == -1 || !int.TryParse(value[9..end], out int reponseIndex) || reponseIndex >= Responses.Count) return value;
                    return GetJsonValueByPath(Responses[reponseIndex], value[(end + 2)..]) ?? value;
                }
                else if (value.StartsWith("response."))
                    return GetJsonValueByPath(Responses[^1], value[9..]) ?? value;
                else if (Parameters.TryGetValue(value, out var parameterValue))
                    return parameterValue;
                else
                    return value;
            }
            catch
            {
                return value;
            }
        }

        public static string? GetJsonValueByPath(JToken? jToken, string path)
        {
            var pathTokens = path.Replace("[", ".[").Split('.');
            foreach (var partToken in pathTokens)
                if (jToken is JObject record)
                    jToken = record[partToken];
                else if (jToken is JArray array && int.TryParse(partToken.Trim('[', ']'), out int index))
                    jToken = array[index];
                else
                    return null;

            return jToken?.ToString();
        }

    }
}