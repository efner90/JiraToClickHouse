namespace JiraToCH.Services
{
    public interface IJiraApi
    {
        Task<Dictionary<string, object>?> GetIssueAsync(string key);
    }
}
