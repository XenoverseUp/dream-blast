using UnityEngine;

public class EventManager : MonoBehaviour {
    public static EventManager Instance { get; private set; }
    
    // Delegate types
    public delegate void RocketActionHandler(Vector3 position, RocketEffect.RocketDirection direction);
    public delegate void BoardStateChangeHandler();
    public delegate void FallingCompleteHandler();
    
    // Events

    public event RocketActionHandler OnRocketActivated;
    public event BoardStateChangeHandler OnBoardUnlocked;
    public event FallingCompleteHandler OnFallingComplete;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    // Event triggers
    
    public void TriggerRocketActivated(Vector3 position, RocketEffect.RocketDirection direction) {
        OnRocketActivated?.Invoke(position, direction);
    }
    
    public void TriggerBoardUnlocked() {
        OnBoardUnlocked?.Invoke();
    }
    
    public void TriggerFallingComplete() {
        OnFallingComplete?.Invoke();
    }
}