using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    
    public int currentLevel = 1;
    public int maxLevel = 10;

    public bool IsGameCompleted => currentLevel > maxLevel;
    
    private void Awake() {
        Debug.Assert(
            currentLevel > 0 && currentLevel <= maxLevel, 
            $"Level cannot be any lower than 0 or bigger than {maxLevel}."
        );
       

        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else Destroy(gameObject);
        
    }
}