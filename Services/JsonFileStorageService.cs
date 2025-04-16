using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using JiraToCH.Models;

namespace JiraToCH.Services
{
    public class JsonFileStorageService : IDataStorageService
    {
        private readonly string _basePath;

        public JsonFileStorageService(string basePath)
        {
            _basePath = basePath;
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task SaveJsonAsync(string jsonData, string fileName = "issues.json")
        {
            var filePath = Path.Combine(_basePath, fileName);
            await SaveToFileAsync(filePath, jsonData);
        }

        private async Task SaveToFileAsync(string filePath, string jsonData)
        {
            await File.WriteAllTextAsync(filePath, jsonData);
        }
    }
}
