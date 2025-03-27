using UnityEngine;
using UnityEditor;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    
    private int _currentLevel;
    public int CurrentLevel {
        get { return _currentLevel; }
        set {
            _currentLevel = value;
            PlayerPrefs.SetInt("CurrentLevel", _currentLevel);
            PlayerPrefs.Save();
        }
    }
    
    private readonly int maxLevel = 10;

    public bool IsGameCompleted => CurrentLevel > maxLevel;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSavedLevel();
        } else {
            Destroy(gameObject);
        }
    }
    
    private void LoadSavedLevel() {
        _currentLevel = PlayerPrefs.HasKey("CurrentLevel") ? 
            PlayerPrefs.GetInt("CurrentLevel") : 1;
            
        Debug.Assert(
            _currentLevel > 0 && _currentLevel <= maxLevel + 1, 
            $"Level cannot be any lower than 0 or bigger than {maxLevel + 1}."
        );
    }

    public void NextLevel() {
        this.CurrentLevel += 1;
    }
    
    #if UNITY_EDITOR
    [MenuItem("Level/Level 1", false, 1)]
    static void SetLevel1() { SetSpecificLevel(1); }
    
    [MenuItem("Level/Level 2", false, 2)]
    static void SetLevel2() { SetSpecificLevel(2); }
    
    [MenuItem("Level/Level 3", false, 3)]
    static void SetLevel3() { SetSpecificLevel(3); }
    
    [MenuItem("Level/Level 4", false, 4)]
    static void SetLevel4() { SetSpecificLevel(4); }
    
    [MenuItem("Level/Level 5", false, 5)]
    static void SetLevel5() { SetSpecificLevel(5); }
    
    [MenuItem("Level/Level 6", false, 6)]
    static void SetLevel6() { SetSpecificLevel(6); }
    
    [MenuItem("Level/Level 7", false, 7)]
    static void SetLevel7() { SetSpecificLevel(7); }
    
    [MenuItem("Level/Level 8", false, 8)]
    static void SetLevel8() { SetSpecificLevel(8); }
    
    [MenuItem("Level/Level 9", false, 9)]
    static void SetLevel9() { SetSpecificLevel(9); }
    
    [MenuItem("Level/Level 10", false, 10)]
    static void SetLevel10() { SetSpecificLevel(10); }
    
    [MenuItem("Level/Finished", false, 11)]
    static void SetLevelFinished() { SetSpecificLevel(11); }
    
    private static void SetSpecificLevel(int levelToSet) {
        if (Instance != null) {
            Instance.CurrentLevel = levelToSet;
        } else {
            PlayerPrefs.SetInt("CurrentLevel", levelToSet);
            PlayerPrefs.Save();
        }
        EditorUtility.DisplayDialog("Success", $"Level set to {levelToSet}!", "OK");
    }
    #endif
}