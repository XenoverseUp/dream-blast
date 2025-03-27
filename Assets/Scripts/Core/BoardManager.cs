using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BoardState {
    Ready,
    Processing,
    Falling,
    GameOver
}

public struct RocketActivationData {
    public Vector3 position;
    public RocketEffect.RocketDirection direction;
    
    public RocketActivationData(Vector3 position, RocketEffect.RocketDirection direction) {
        this.position = position;
        this.direction = direction;
    }
}

public class BoardManager : MonoBehaviour {
    public static BoardManager Instance { get; private set; }

    [SerializeField] private float processingDelay = 2f;
    
    private BoardState currentState = BoardState.Ready;
    private Queue<RocketActivationData> rocketQueue = new Queue<RocketActivationData>();
    private bool processingRockets = false;
    private bool waitingForChainedRockets = false;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }
    
    private void Start() {
        if (EventManager.Instance != null) {
            EventManager.Instance.OnRocketActivated += OnRocketActivated;
            EventManager.Instance.OnFallingComplete += OnFallingComplete;

        }
    }
    
    private void Update() {
        if (currentState == BoardState.Processing && !processingRockets && rocketQueue.Count > 0) {
            StartCoroutine(ProcessRocketQueue());
        }
    }
    
    public bool IsInteractable() {
        return currentState == BoardState.Ready;
    }
    
    public void SetState(BoardState newState) {
        if (newState == currentState) return;
        currentState = newState;

        if (newState == BoardState.Ready) {
            EventManager.Instance?.TriggerBoardUnlocked();
        }
    }
    
    private void OnRocketActivated(Vector3 position, RocketEffect.RocketDirection direction) {
        rocketQueue.Enqueue(new RocketActivationData(position, direction));
        
        if (currentState == BoardState.Ready) SetState(BoardState.Processing);
        
        waitingForChainedRockets = true;
        
        CancelInvoke("CheckForRemainingRockets");
        Invoke("CheckForRemainingRockets", 0.4f);
    }
    
    private void CheckForRemainingRockets() {
        waitingForChainedRockets = false;
        
        if (!processingRockets && currentState == BoardState.Processing) {
            StopAllCoroutines();
            StartCoroutine(ProcessRocketQueue());
        }
    }
    
    private IEnumerator ProcessRocketQueue() {
        processingRockets = true;
        
        while (rocketQueue.Count > 0) {
            RocketActivationData data = rocketQueue.Dequeue();
            RocketSpawner.Instance.SpawnRocket(data.position, data.direction);
        }
        
        processingRockets = false;
        
        if (rocketQueue.Count == 0 && !waitingForChainedRockets) {
            yield return new WaitForSeconds(processingDelay);
            SetState(BoardState.Falling);
            
            Board board = FindFirstObjectByType<Board>();
            if (board != null) {
                yield return StartCoroutine(board.ProcessFallingItemsAfterDelay(AnimationManager.Instance.blockSpawnDelay));
            }
        }
    }
    
    private void OnFallingComplete() {
        SetState(BoardState.Ready);
    }
    
    private void OnDestroy() {
        if (EventManager.Instance != null) {
            EventManager.Instance.OnRocketActivated -= OnRocketActivated;
            EventManager.Instance.OnFallingComplete -= OnFallingComplete;
        }
        
        CancelInvoke();
    }
}