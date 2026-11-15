using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneRoutes
{
    public const string CharacterSetup = "GenderSelection";
    public const string FirstDay = "dia1";

    public static string Location(int day, char location)
    {
        switch (day)
        {
            case 1:
                switch (location)
                {
                    case 'S': return "dia1solda";
                    case 'P': return "dia1pc";
                }
                break;
            case 5:
                switch (location)
                {
                    case 'M': return "dia5mesa";
                    case 'F': return "dia5flip";
                }
                break;
        }
        return null; // These locations have no implemented scene yet.
    }

    public static bool IsEnabled(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) return false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            if (Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)) == sceneName)
                return true;
        }
        return false;
    }

    public static bool Load(string sceneName)
    {
        if (!IsEnabled(sceneName))
        {
            Debug.LogWarning($"Scene '{sceneName}' is unavailable in this build.");
            return false;
        }
        SceneManager.LoadScene(sceneName);
        return true;
    }
}
