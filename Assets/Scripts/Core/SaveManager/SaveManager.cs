using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    private SaveData saveData;
    private SaveHandler saveHandler;
    public static SaveManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (GameManager.Instance == null) new GameObject("GameManager").AddComponent<GameManager>();
        if (Instance == null) new GameObject("SaveManager").AddComponent<SaveManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (Instance == this) Instance = null;
    }

    private void Initialize()
    {
        if (saveHandler == null)
            saveHandler = new SaveHandler(Application.persistentDataPath, "game_save_slot");
        if (saveData == null) saveData = new SaveData();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Entering setup means starting a new character, including from the menu.
        if (scene.name == "GenderSelection") NewGame();
        else if (mode == LoadSceneMode.Single && IsStoryScene(scene.name) &&
                 GameManager.Instance != null && !string.IsNullOrWhiteSpace(GameManager.Instance.playerName) &&
                 !string.IsNullOrEmpty(GameManager.Instance.selectedGender)) SaveGame();
    }

    public void NewGame()
    {
        Initialize();
        saveData = new SaveData();
        if (GameManager.Instance != null) GameManager.Instance.LoadData(saveData);
    }

    public void LoadGame()
    {
        Initialize();
        saveData = saveHandler.Load() ?? new SaveData();
        if (GameManager.Instance != null) GameManager.Instance.LoadData(saveData);
    }

    public void OnPlay()
    {
        LoadGame();
        string destination = string.IsNullOrWhiteSpace(saveData.playerName) ||
                             string.IsNullOrEmpty(saveData.selectedGender)
            ? "GenderSelection" : saveData.sceneName;
        if (!IsStoryScene(destination) && destination != "GenderSelection")
            destination = saveData.day == 5 ? "dia5" : "dia1";
        if (Application.CanStreamedLevelBeLoaded(destination)) SceneManager.LoadScene(destination);
        else Debug.LogWarning($"Saved scene '{destination}' is not enabled in the build.");
    }

    public void SaveGame()
    {
        Initialize();
        if (GameManager.Instance == null) return;
        GameManager.Instance.SaveData(ref saveData);
        string scene = SceneManager.GetActiveScene().name;
        if (IsStoryScene(scene))
        {
            saveData.sceneName = scene;
            saveData.day = scene.StartsWith("dia5") ? 5 : 1;
            GameManager.Instance.currentDay = saveData.day;
        }
        saveHandler.Save(saveData);
    }

    public void SaveDayEnd(int nextDay)
    {
        Initialize();
        if (GameManager.Instance == null) return;
        GameManager.Instance.SaveData(ref saveData);
        saveData.day = nextDay == 5 ? 5 : 1;
        saveData.sceneName = saveData.day == 5 ? "dia5" : "dia1";
        saveHandler.Save(saveData);
    }

    public void SavePlayerInfo()
    {
        Initialize();
        if (GameManager.Instance == null) return;
        GameManager.Instance.SavePlayerInfo(ref saveData);
        saveHandler.Save(saveData);
    }

    public static bool IsStoryScene(string scene)
    {
        return scene == "dia1" || scene == "dia1pc" || scene == "dia1solda" ||
               scene == "dia5" || scene == "dia5mesa" || scene == "dia5flip";
    }
}
