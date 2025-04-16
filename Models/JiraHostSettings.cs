namespace JiraToCH.Models
{
    public class JiraHostSettings
    {
        public string Scheme { get; set; } = "https"; // Схема (http/https)
        public string Host { get; set; }             // Хост Jira
        public string Token { get; set; }            // Токен для доступа к API
        public string RequestIssue { get; set; }     // Путь для запроса задачи
        public double WaitDelaySeconds { get; set; } // Задержка между запросами
    }
}
