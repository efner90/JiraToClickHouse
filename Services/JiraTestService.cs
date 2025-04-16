using JiraToCH.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JiraToCH.Services
{
    public class JiraTestService
    {
        private readonly IJiraApi _jiraApi;
        private readonly ILogger<JiraTestService> _logger;
        private IDataStorageService _storage;
        private ExcelFileService _excelServise;

        public JiraTestService(IJiraApi jiraApi, ILogger<JiraTestService> logger, IDataStorageService storage, ExcelFileService excelService)
        {
            _jiraApi = jiraApi;
            _logger = logger;
            _storage = storage;
            _excelServise = excelService;
        }

        public async Task TestJiraApiAsync(string issueKey)
        {
            try
            {
                _logger.LogInformation($"Запрашиваем данные для задачи: {issueKey}");

                string timeStamp = DateTime.Now.ToString("ddMMyyyy_HHmmss");

                // Выполняем запрос к Jira API
                var issueData = await _jiraApi.GetIssueAsync(issueKey);

                if (issueData == null)
                {
                    _logger.LogError("Данные не получены. Проверьте ключ задачи и настройки Jira API.");
                    return;
                }

                _logger.LogInformation($"Задача готова: {issueData["key"]}");

                // Преобразуем JObject в строку JSON перед сохранением
                var serializedJson = JsonConvert.SerializeObject(issueData, Formatting.Indented);

                await _storage.SaveJsonAsync(serializedJson, $"jsontest_{timeStamp}.json");
                await _excelServise.CreateNewExcelFileAsync(issueData);
                await _excelServise.AppendToExistingExcelFileAsync(issueData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка при выполнении запроса: {ex.Message}");
            }
        }
    }
}
