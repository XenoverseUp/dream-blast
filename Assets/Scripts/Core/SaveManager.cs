using UnityEngine;

public static class SaveManager
{
    private const string LevelKey = "CurrentLevel";
    
    public static void SaveLevel(int level)
    {
        PlayerPrefs.SetInt(LevelKey, level);
        PlayerPrefs.Save();
    }
    
    public static int LoadLevel()
    {
        return PlayerPrefs.HasKey(LevelKey) ? PlayerPrefs.GetInt(LevelKey) : 1;
    }
}
