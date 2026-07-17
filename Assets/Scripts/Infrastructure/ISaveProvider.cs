using System.Threading.Tasks;

public interface ISaveProvider
{
    Task<bool> SaveAsync(string key, string data);
    Task<(bool success, string data)> LoadAsync(string key);
    Task<bool> DeleteAsync(string key);
    bool HasSave(string key);
}