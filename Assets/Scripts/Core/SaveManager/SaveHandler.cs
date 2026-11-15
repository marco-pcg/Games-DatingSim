using System;
using System.IO;
using UnityEngine;

public class SaveHandler
{
    private readonly string path;

    [Serializable]
    private class Header
    {
        public int day;
        public int version;
        public string sceneName;
    }

    public SaveHandler(string directoryName, string fileName)
    {
        path = Path.Combine(directoryName, fileName);
    }

    public SaveData Load()
    {
        if (!File.Exists(path)) return null;
        try
        {
            string json = File.ReadAllText(path);
            Header header = JsonUtility.FromJson<Header>(json);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            // Old saves had no version/scene name; the day remains their checkpoint.
            if (data == null || header == null || (header.day != 1 && header.day != 5) ||
                data.version > 1 || data.version < 0 ||
                (data.selectedGender != null && data.selectedGender != "" &&
                 data.selectedGender != "Male" && data.selectedGender != "Female"))
                throw new InvalidDataException("Unsupported save data.");
            if (string.IsNullOrEmpty(header.sceneName))
                data.sceneName = data.day == 5 ? "dia5" : "dia1";
            return data;
        }
        catch (Exception exception) when (exception is IOException || exception is ArgumentException ||
                                           exception is UnauthorizedAccessException)
        {
            Debug.LogWarning($"Unable to load save '{path}': {exception.Message}");
            return null;
        }
    }

    public bool Save(SaveData data)
    {
        string temporaryPath = path + ".tmp";
        try
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(temporaryPath, JsonUtility.ToJson(data, true));
            // Preserve the previous save until the replacement has been fully written.
            if (File.Exists(path)) File.Replace(temporaryPath, path, null);
            else File.Move(temporaryPath, path);
            return true;
        }
        catch (Exception exception) when (exception is IOException || exception is ArgumentException ||
                                           exception is UnauthorizedAccessException)
        {
            Debug.LogWarning($"Unable to save game '{path}': {exception.Message}");
            return false;
        }
    }
}
