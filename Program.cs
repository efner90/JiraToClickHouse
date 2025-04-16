using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using JiraToCH.Services;

namespace JiraToCH
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Тестовый вызов сервиса при старте
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var jiraTestService = services.GetRequiredService<JiraTestService>();
                    await jiraTestService.TestJiraApiAsync("DP-412");
                    await jiraTestService.TestJiraApiAsync("DP-1287"); 
                    await jiraTestService.TestJiraApiAsync("DP-736");
                    await jiraTestService.TestJiraApiAsync("DP-822"); 
                    await jiraTestService.TestJiraApiAsync("DP-1294");
                    await jiraTestService.TestJiraApiAsync("DP-812");
                    await jiraTestService.TestJiraApiAsync("DP-1251");
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Ошибка при выполнении тестового запроса.");
                }
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
