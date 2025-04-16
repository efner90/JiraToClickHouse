using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace JiraToCH.Services
{
    public class ExcelFileService
    {
        private readonly string _basePath;

        public ExcelFileService(string basePath)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            _basePath = basePath;

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        /// <summary>
        /// Создает новый Excel-файл с уникальным именем.
        /// </summary>
        public async Task CreateNewExcelFileAsync(Dictionary<string, object> issueData)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filePath = Path.Combine(_basePath, $"issue_{timestamp}.xlsx");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("IssueData");

                var flatData = FlattenDictionary(issueData);

                //worksheet.Cells[1, 1].Value = "ID";
                //worksheet.Cells[2, 1].Value = issueData.ContainsKey("id") ? issueData["id"]?.ToString() : "N/A";

                int colIndex = 1;
                foreach (var kvp in flatData)
                {
                    worksheet.Cells[1, colIndex].Value = kvp.Key;
                    worksheet.Cells[2, colIndex].Value = kvp.Value;

                    colIndex++;
                }

                FileInfo excelFile = new FileInfo(filePath);
                await package.SaveAsAsync(excelFile);
            }
        }

        /// <summary>
        /// Добавляет новую запись в общий Excel-файл (all_issues.xlsx).
        /// </summary>
        public async Task AppendToExistingExcelFileAsync(Dictionary<string, object> issueData)
        {
            string filePath = Path.Combine(_basePath, "all_issues.xlsx");
            FileInfo excelFile = new FileInfo(filePath);

            using (var package = excelFile.Exists ? new ExcelPackage(excelFile) : new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault() ?? package.Workbook.Worksheets.Add("AllIssues");

                int lastRow = worksheet.Dimension?.End.Row ?? 0;
                int newRow = lastRow + 1;

                var flatData = FlattenDictionary(issueData);

                // Проверяем, есть ли заголовки (первая строка)
                if (lastRow == 0 || worksheet.Cells[1, 1].Value == null)
                {
                    int colIndex = 1; // Начинаем с первого столбца
                    foreach (var kvp in flatData)
                    {
                        worksheet.Cells[1, colIndex].Value = kvp.Key; // Заголовки полей
                        colIndex++;
                    }
                    newRow = 2; // Следующая строка для данных
                }

                // Записываем данные начиная с первого столбца (без дублирования "ID")
                int colIdx = 1;
                foreach (var kvp in flatData)
                {
                    worksheet.Cells[newRow, colIdx].Value = kvp.Value;
                    colIdx++;
                }

                await package.SaveAsAsync(excelFile);
            }
        }


        /// <summary>
        /// Разворачивает вложенные JSON-объекты в плоский словарь "ключ.подключ" -> "значение".
        /// </summary>
        private Dictionary<string, object> FlattenDictionary(Dictionary<string, object> dict, string parentKey = "")
        {
            var result = new Dictionary<string, object>();

            foreach (var kvp in dict)
            {
                string newKey = string.IsNullOrEmpty(parentKey) ? kvp.Key : $"{parentKey}.{kvp.Key}";

                if (kvp.Value is JObject obj)
                {
                    var nestedDict = obj.ToObject<Dictionary<string, object>>();
                    foreach (var nested in FlattenDictionary(nestedDict, newKey))
                    {
                        result[nested.Key] = nested.Value;
                    }
                }
                else if (kvp.Value is Dictionary<string, object> subDict)
                {
                    foreach (var nested in FlattenDictionary(subDict, newKey))
                    {
                        result[nested.Key] = nested.Value;
                    }
                }
                else if (kvp.Value is IEnumerable<object> list)
                {
                    result[newKey] = string.Join(", ", list);
                }
                else
                {
                    result[newKey] = kvp.Value?.ToString();
                }
            }

            return result;
        }
    }
}
