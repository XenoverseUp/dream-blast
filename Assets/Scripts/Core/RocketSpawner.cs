using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RocketSpawner : MonoBehaviour {
    [SerializeField] private GameObject horizontalRocketPrefab;
    [SerializeField] private GameObject verticalRocketPrefab;
    [SerializeField] private GameObject explosionPrefab;
    
    // Parent transform where rockets should be instantiated
    [SerializeField] private Transform rocketParent;
    
    public static RocketSpawner Instance { get; private set; }
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        
        if (rocketParent == null)
            rocketParent = transform;
    }
    
    public void SpawnRocket(Vector3 position, RocketEffect.RocketDirection direction) {
        if (direction == RocketEffect.RocketDirection.Horizontal) {
            SpawnHorizontalRocket(position);
        } else {
            SpawnVerticalRocket(position);
        }
    }
    
    public void SpawnRandomRocket(Vector3 position) {
        if (Random.value < 0.5f) {
            SpawnHorizontalRocket(position);
        } else {
            SpawnVerticalRocket(position);
        }
    }
    
    public void SpawnRocketFromCell(CellItem cellItem) {
        Board board = cellItem.GetComponentInParent<Board>();
        if (board == null) return;
        
        Vector3 position = board.GetWorldPosition(cellItem.X, cellItem.Y);
        CellItemType itemType = cellItem.GetItemType();
        
        board.RemoveItem(cellItem.X, cellItem.Y);
        
        List<Vector2Int> rocketPositions = FindAdjacentRockets(board, cellItem.X, cellItem.Y);
        
        if (rocketPositions.Count > 0) {
            StartCoroutine(CreateRocketCombo(board, position, cellItem.X, cellItem.Y, rocketPositions));
        } else {
            bool isHorizontal = itemType == CellItemType.HorizontalRocket;
            RocketEffect.RocketDirection direction = isHorizontal ? 
                RocketEffect.RocketDirection.Horizontal : 
                RocketEffect.RocketDirection.Vertical;
                
            SpawnRocket(position, direction);
            
            StartCoroutine(TriggerFallingItemsAfterRocket(board));
        }
    }
    
    private List<Vector2Int> FindAdjacentRockets(Board board, int x, int y) {
        List<Vector2Int> positions = new List<Vector2Int>();
        Vector2Int[] directions = { new Vector2Int(0, 1), new Vector2Int(1, 0), 
                                    new Vector2Int(0, -1), new Vector2Int(-1, 0) };
        
        foreach (Vector2Int dir in directions) {
            int newX = x + dir.x;
            int newY = y + dir.y;
            
            if (newX >= 0 && newX < board.GridWidth && newY >= 0 && newY < board.GridHeight) {
                // Check if there's a rocket at this position
                // This would need to be implemented in the Board class
                // For now, we'll just return an empty list
            }
        }
        
        return positions;
    }
    
    private IEnumerator CreateRocketCombo(Board board, Vector3 position, int x, int y, List<Vector2Int> rocketPositions) {
        // Spawn explosion effect
        if (explosionPrefab != null) {
            Instantiate(explosionPrefab, position, Quaternion.identity);
        }
        
        // Create 3x3 grid of rockets
        for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 1; j++) {
                if (i == 0 && j == 0) continue; // Skip center
                
                int newX = x + i;
                int newY = y + j;
                
                if (newX >= 0 && newX < board.GridWidth && newY >= 0 && newY < board.GridHeight) {
                    Vector3 rocketPos = board.GetWorldPosition(newX, newY);
                    
                    // Randomly choose horizontal or vertical
                    RocketEffect.RocketDirection direction = Random.value < 0.5f ? 
                        RocketEffect.RocketDirection.Horizontal : 
                        RocketEffect.RocketDirection.Vertical;
                        
                    yield return new WaitForSeconds(0.1f);
                    SpawnRocket(rocketPos, direction);
                }
            }
        }
        
        // Remove the other rockets
        foreach (Vector2Int pos in rocketPositions) {
            board.RemoveItem(pos.x, pos.y);
        }
        
        // Trigger falling items after delay
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(TriggerFallingItemsAfterRocket(board));
    }
    
    private IEnumerator TriggerFallingItemsAfterRocket(Board board) {
        yield return new WaitForSeconds(0.25f);
        StartCoroutine(board.ProcessFallingItemsAfterDelay(0.3f));
    }
    
    public void SpawnHorizontalRocket(Vector3 position) {
        if (horizontalRocketPrefab != null) {
            GameObject rocket = Instantiate(horizontalRocketPrefab, position, Quaternion.identity, rocketParent);
            
            RocketEffect rocketEffect = rocket.GetComponent<RocketEffect>();
            if (rocketEffect != null) {
                rocketEffect.SetDirection(RocketEffect.RocketDirection.Horizontal);
            }
        }
    }
    
    public void SpawnVerticalRocket(Vector3 position) {
        if (verticalRocketPrefab != null) {
            GameObject rocket = Instantiate(verticalRocketPrefab, position, Quaternion.identity, rocketParent);
            
            RocketEffect rocketEffect = rocket.GetComponent<RocketEffect>();
            if (rocketEffect != null) {
                rocketEffect.SetDirection(RocketEffect.RocketDirection.Vertical);
            }
        }
    }
}