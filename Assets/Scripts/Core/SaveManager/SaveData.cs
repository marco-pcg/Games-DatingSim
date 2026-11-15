[System.Serializable]
public class SaveData
{
    public int version = 1;
    public int day = 1;
    public string sceneName = "dia1";
    public string playerName;
    public int affectionPoints;
    public string selectedGender;
    // Gender is the stable appearance identifier. Scene objects are never persisted.
}
