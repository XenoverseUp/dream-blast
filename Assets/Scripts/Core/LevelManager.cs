using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    public static LevelManager Instance { get; private set; }

    [SerializeField] private GameObject popup;

    private List<IActionListener> listeners = new List<IActionListener>();
    
    private int moveCount = 0;
    private LevelData levelData;
    private Dictionary<CellItemType, int> obstacleCountMap = new Dictionary<CellItemType, int>();
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else Destroy(gameObject);
    }
    
    private void Start() {
        LoadCurrentLevel();
    }

    public void Reset() {
        LoadCurrentLevel();
    }
    
    private bool LoadCurrentLevel() {
        int levelNumber = GameManager.Instance.CurrentLevel;
        levelData = LevelData.LoadLevel(levelNumber);
        
        if (levelData == null) {
            Debug.LogError($"Failed to load level {levelNumber}");
            return false;
        }

        moveCount = levelData.MoveCount;
        obstacleCountMap = levelData.GetObstacleCountMap();

        Notify();

        return true;
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

    public Dictionary<CellItemType, int> GetBlocks() { return this.obstacleCountMap; }
    
    public List<CellItemType> GetObstacleTypes() { return new List<CellItemType>(obstacleCountMap.Keys); }

    public LevelData GetLevelData() { return this.levelData; }

    public bool HasMove() {
        return this.moveCount >= 0;
    }

    public void SpendMove() {
        this.moveCount -= 1;

        if (moveCount < 0) {
            popup.SetActive(true);
            BoardManager.Instance.SetState(BoardState.GameOver);
        }
        
        Notify();
    }

    public void UpdateObstacleCountMap(int box, int stone, int vase) {
        if (obstacleCountMap.ContainsKey(CellItemType.Box)) obstacleCountMap[CellItemType.Box] = box;
        if (obstacleCountMap.ContainsKey(CellItemType.Stone)) obstacleCountMap[CellItemType.Stone] = stone;
        if (obstacleCountMap.ContainsKey(CellItemType.Vase)) obstacleCountMap[CellItemType.Vase] = vase;

        Notify();

        if (box + stone + vase == 0) {
            LevelTransition.Instance.SetActiveScene(ActiveScene.MainMenu);
            GameManager.Instance.NextLevel();
        }
    }
}