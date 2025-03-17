using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public enum ActiveScene {
    MainMenu,
    Level
};

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private string mainSceneName;
    [SerializeField] private string levelSceneName;

    [SerializeField] private ActiveScene activeScene = ActiveScene.MainMenu;
    public int currentLevel = 1;
    public int maxLevel = 10;

    public bool IsGameCompleted => currentLevel > maxLevel;
    
    private void Awake() {
        Debug.Assert(
            currentLevel > 0 && currentLevel <= maxLevel, 
            $"Level cannot be any lower than 0 or bigger than {maxLevel}."
        );
       
        Debug.Assert(
            mainSceneName == null || levelSceneName == null, 
            $"Please provide the scene names."
        );

        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else Destroy(gameObject);
        
    }

    public void SetActiveScene(ActiveScene scene) {
        if (activeScene == scene) return;

        switch (scene) {
            case ActiveScene.MainMenu: 
                SceneManager.LoadScene(mainSceneName);
                break;
            case ActiveScene.Level:
                if (currentLevel == maxLevel) return;
                SceneManager.LoadScene(levelSceneName);
                break;
        }
    }
}