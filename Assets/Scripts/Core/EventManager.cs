using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {
    public static EventManager Instance { get; private set; }
    
    // Delegate types
    public delegate void CellActionHandler(int x, int y);
    public delegate void RocketActionHandler(Vector3 position, RocketEffect.RocketDirection direction);
    public delegate void BoardStateChangeHandler();
    public delegate void FallingCompleteHandler();
    
    // Events

    public event CellActionHandler OnRocketCreated;
    public event RocketActionHandler OnRocketActivated;
    public event Action<int, int, List<Vector2Int>> OnRocketChainTriggered;
    public event BoardStateChangeHandler OnBoardLocked;
    public event BoardStateChangeHandler OnBoardUnlocked;
    public event FallingCompleteHandler OnFallingComplete;
    public event Action<bool> OnGameStateChanged;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    // Event triggers
    
    public void TriggerRocketCreated(int x, int y) {
        OnRocketCreated?.Invoke(x, y);
    }
    
    public void TriggerRocketActivated(Vector3 position, RocketEffect.RocketDirection direction) {
        OnRocketActivated?.Invoke(position, direction);
    }
    
    public void TriggerRocketChainTriggered(int x, int y, List<Vector2Int> chainPositions) {
        OnRocketChainTriggered?.Invoke(x, y, chainPositions);
    }
    
    public void TriggerBoardLocked() {
        OnBoardLocked?.Invoke();
    }
    
    public void TriggerBoardUnlocked() {
        OnBoardUnlocked?.Invoke();
    }
    
    public void TriggerFallingComplete() {
        OnFallingComplete?.Invoke();
    }
    
    public void TriggerGameStateChanged(bool isPlaying) {
        OnGameStateChanged?.Invoke(isPlaying);
    }
}