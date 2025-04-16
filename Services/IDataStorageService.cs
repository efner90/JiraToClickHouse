using System.Collections.Generic;
using System.Threading.Tasks;
using JiraToCH.Models;

namespace JiraToCH.Services
{
    public interface IDataStorageService
    {
        Task SaveJsonAsync(string data, string filename);
    }
}
