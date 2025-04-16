using JiraToCH.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace JiraToCH.Services
{
    public class JiraApiService : IJiraApi
    {
        private readonly ILogger<JiraApiService> _logger;
        private readonly JiraHostSettings _jiraHostSettings;

        public JiraApiService(IOptions<JiraHostSettings> jiraHostSettings, ILogger<JiraApiService> logger)
        {
            _logger = logger;
            _jiraHostSettings = jiraHostSettings.Value;
        }

        public async Task<Dictionary<string, object>?> GetIssueAsync(string key)
        {
            CancellationToken token = new CancellationTokenSource().Token;
            var httpResponse = await RestRequestAsync($"{_jiraHostSettings.RequestIssue}/{key}", token);
            if (httpResponse is null) return null;

            var jsonString = await httpResponse.Content.ReadAsStringAsync(token);
            //var issueData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

            if (string.IsNullOrWhiteSpace(jsonString))
            {
                _logger.LogError("Ошибка: Получен пустой JSON");
                return null;
            }

            JObject issueData;
            try
            {
                issueData = JObject.Parse(jsonString);
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError("Ошибка парсинга JSON: {Message}", ex.Message);
                return null;
            }

            if (!issueData.ContainsKey("fields"))
            {
                _logger.LogError("Ошибка: В JSON отсутствует ключ 'fields'");
                return null;
            }

            var fields = issueData["fields"] as JObject;
            if (fields == null)
            {
                _logger.LogError("Ошибка: 'fields' не является объектом JSON");
                return null;
            }

            var issue = new Dictionary<string, object>
            {
                ["id"] = issueData["id"]?.ToString(),
                ["key"] = issueData["key"]?.ToString(),
                ["created"] = fields.Value<string>("created"),
                ["summary"] = fields.Value<string>("summary"),
                ["assignee"] = fields["assignee"]?["displayName"]?.ToString(),
                ["creator"] = fields["creator"]?["displayName"]?.ToString(),
                ["customfield_16211"] = fields["customfield_16211"]?.ToObject<List<string>>(),
                ["customfield_11000"] = fields.Value<string>("customfield_11000"),
                ["priority"] = fields["priority"]?["name"]?.ToString(),
                ["issuetype"] = fields["issuetype"]?["name"]?.ToString(),
                ["customfield_10001"] = ExtractSprintName(fields["customfield_10001"]?.ToString()),
                ["labels"] = fields["labels"]?.ToObject<List<string>>(),
                ["timetracking"] = new Dictionary<string, object>
                {
                    ["originalEstimate"] = fields["timetracking"]?["originalEstimate"]?.ToString(),
                    ["remainingEstimate"] = fields["timetracking"]?["remainingEstimate"]?.ToString(),
                    ["timeSpent"] = fields["timetracking"]?["timeSpent"]?.ToString()
                },
                ["status"] = fields["status"]?["name"]?.ToString(),
                ["customfield_14724"] = fields["customfield_14724"]?.ToString(),
                ["customfield_14725"] = fields["customfield_14725"]?.ToString(),
                ["customfield_14726"] = fields["customfield_14726"]?.ToString(),
                ["customfield_16513"] = fields["customfield_16513"]?.ToString()
            };

            _logger.LogInformation($"Задача {issue["key"]} успешно сохранена в JSON");



            return issue;

        }

        private string ExtractSprintName(string input)
        {
            var match = Regex.Match(input ?? "", @"name=([^,]+)");
            return match.Success ? match.Groups[1].Value : input;
        }


        private async Task<HttpResponseMessage?> RestRequestAsync(string path, CancellationToken token)
        {
            var timeOut = TimeSpan.FromSeconds(_jiraHostSettings.WaitDelaySeconds);

            using var httpClient = new HttpClient();
            httpClient.Timeout = timeOut;
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_jiraHostSettings.Token}");

            var uri = new UriBuilder
            {

                Scheme = _jiraHostSettings.Scheme,
                Host = _jiraHostSettings.Host
            };
            uri.Path += path;

            using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri.ToString());
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                var httpResponseMessage = await httpClient
                    .SendAsync(httpRequestMessage, token)
                    .WaitAsync(timeOut, token);

                if (httpResponseMessage.IsSuccessStatusCode)
                    return httpResponseMessage;
            }
            catch (Exception e)
            {
                _logger.LogWarning("Хост данных недоступен | Exception {Exception} | InnerException {InnerException}",
                    e.Message, e.InnerException?.Message);
            }

            _logger.LogError("Ошибка запроса");
            return null;
        }
    }
}
