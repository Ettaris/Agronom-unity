using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class LocalFileSaveProvider : ISaveProvider
{
    private readonly string _saveFolder;

    public LocalFileSaveProvider()
    {
        _saveFolder = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(_saveFolder))
            Directory.CreateDirectory(_saveFolder);
    }

    public async Task<bool> SaveAsync(string key, string data)
    {
        try
        {
            string path = GetFilePath(key);
            await File.WriteAllTextAsync(path, data);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Save failed: {ex.Message}");
            return false;
        }
    }

    public async Task<(bool success, string data)> LoadAsync(string key)
    {
        try
        {
            string path = GetFilePath(key);
            if (!File.Exists(path))
                return (false, null);
            string data = await File.ReadAllTextAsync(path);
            return (true, data);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Load failed: {ex.Message}");
            return (false, null);
        }
    }

    public Task<bool> DeleteAsync(string key)
    {
        try
        {
            string path = GetFilePath(key);
            if (File.Exists(path))
                File.Delete(path);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public bool HasSave(string key)
    {
        return File.Exists(GetFilePath(key));
    }

    private string GetFilePath(string key) => Path.Combine(_saveFolder, $"{key}.json");
}