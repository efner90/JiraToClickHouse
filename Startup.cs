using JiraToCH.Models;
using JiraToCH.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JiraToCH
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<JiraHostSettings>(Configuration.GetSection("JiraHostSettings"));
            services.AddControllers();
            services.AddScoped<IJiraApi, JiraApiService>();
            services.AddScoped<IDataStorageService, JsonFileStorageService>(provider =>
                new JsonFileStorageService(Path.Combine(Directory.GetCurrentDirectory(), "Data")));
            services.AddScoped<JiraTestService>();
            services.AddScoped<ExcelFileService>(provider =>
                new ExcelFileService(Path.Combine(Directory.GetCurrentDirectory(), "Data")));            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
