using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    public static LevelManager Instance { get; private set; }

    [SerializeField] private List<MonoBehaviour> listenerObjects = new List<MonoBehaviour>();

    private List<IActionListener> listeners = new List<IActionListener>();
    
    private int moveCount = 0;
    private LevelData currentLevel;
    private Dictionary<string, int> obstacleCountMap = new Dictionary<string, int>();
    private bool loaded = false;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else Destroy(gameObject);

        loaded = LoadCurrentLevel();
    }
    
    private void Start() {
        if (!loaded) return;
        
        InitializeListeners();
        Notify();
    }
    
    
    private bool LoadCurrentLevel() {
        int levelNumber = GameManager.Instance.currentLevel;
        currentLevel = LevelData.LoadLevel(levelNumber);
        
        if (currentLevel == null) {
            Debug.LogError($"Failed to load level {levelNumber}");
            return false;
        }

        moveCount = currentLevel.MoveCount;
        obstacleCountMap = currentLevel.GetObstacleCountMap();

        return true;
    }

    private void InitializeListeners() {
        foreach (MonoBehaviour obj in listenerObjects) {
            if (obj is IActionListener listener) listeners.Add(listener);
            else Debug.LogWarning($"Object {obj.name} does not implement IActionListener interface.");
        }
    }
    
    private void Notify() {
        foreach (IActionListener listener in listeners) listener.OnAction(moveCount, obstacleCountMap);
    }
    
    public void Subscribe(IActionListener listener) {
        if (!listeners.Contains(listener)) {
            listeners.Add(listener);
            listener.OnAction(moveCount, obstacleCountMap);
        }
    }
    
    public void Unsubscribe(IActionListener listener) {
        if (listeners.Contains(listener)) listeners.Remove(listener);
    }

    public Dictionary<string, int> GetBlocks() { return this.obstacleCountMap; }
    
    public List<string> GetObstacleTypes() { return new List<string>(obstacleCountMap.Keys); }
}