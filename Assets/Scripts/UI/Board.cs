using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
    private BlockFactory blockFactory;
    
    private int gridWidth;
    private int gridHeight;
    private float cellSize;
    private CellItem[,] grid;
    
    private bool isFalling = false;

    /* Direction Constants */
    private static readonly Vector2Int[] ADJACENT_DIRECTIONS = new Vector2Int[] {
        new(0, 1),
        new(1, 0),
        new(0, -1),
        new(-1, 0)
    };

    public int GridHeight => gridHeight;
    public int GridWidth => gridWidth;

    /* Initialization */
    public void Initialize(float cellSize) {
        this.cellSize = cellSize;
        this.gridWidth = LevelManager.Instance.GetLevelData().GridWidth;
        this.gridHeight = LevelManager.Instance.GetLevelData().GridHeight;
        
        InitializeGrid();
        blockFactory.Initialize(this, cellSize);
        PopulateGrid(LevelManager.Instance.GetLevelData().Grid);
    }

    /* Grid Actions */
    private void InitializeGrid() {
        grid = new CellItem[gridWidth, gridHeight];

        blockFactory = GetComponent<BlockFactory>();
        
        if (blockFactory == null) {
            Debug.LogError("BlockFactory component is missing. Make sure to add BlockFactory before Board component.");
            return;
        }
    }

    private void PopulateGrid(List<string> levelGrid) {
        for (int i = 0; i < levelGrid.Count; i++) {
            int x = i % gridWidth;
            int y = Mathf.FloorToInt(i / gridWidth);
            
            string itemType = levelGrid[i];
            SpawnItemFromType(x, y, itemType);
        }

        RenderRocketStateSprites();
    }

    private void RemoveItem(int x, int y) {
        if (grid[x, y] != null) {
            CellItem item = grid[x, y];
            Destroy(item.gameObject);
            grid[x, y] = null;
        }
    }

    private void SpawnItemFromType(int x, int y, string itemType) {
        if (string.IsNullOrEmpty(itemType)) return;

        CellItem itemComponent = blockFactory.CreateItemFromTypeString(x, y, itemType);
        if (itemComponent != null) {
            grid[x, y] = itemComponent;
        }
    }

    /* Blasting Mechanic */
    public bool TryBlast(int x, int y) {
        if (isFalling) {
            Debug.Log("Cannot blast while items are falling");
            return false;
        }
        
        if (grid[x, y] == null) {
            Debug.Log("Cell is empty");
            return false;
        }
        
        CellItem item = grid[x, y];

        if (item.IsObstacle()) {
            AnimationManager.Instance.PlayInvalidBlast(item.gameObject);
            return false;
        }

        if (item.IsCube()) {
            CellItemType itemType = item.GetItemType();
            HashSet<CellItem> connectedCells = FindConnectedCells(x, y, itemType);
            
            if (connectedCells.Count < 2) {
                AnimationManager.Instance.PlayInvalidBlast(item.gameObject);
                return false;
            }
            
            bool shouldCreateRocket = connectedCells.Count >= 4;
            Vector2Int clickPosition = new(x, y);
            
            ProcessConnectedCubes(connectedCells, clickPosition, shouldCreateRocket, itemType);
            return true;
        } else if (item.IsRocket()) {
            Debug.Log("Rocket clicked - functionality will be implemented later");
            return false;
        }
        
        return false;
    }

    private void ProcessConnectedCubes(HashSet<CellItem> connectedCells, Vector2Int clickPosition, bool createRocket, CellItemType sourceType) {
        HashSet<Vector2Int> affectedObstacles = new HashSet<Vector2Int>();
        
        foreach (CellItem cell in connectedCells) {
            AnimationManager.Instance.PlayDestroyBlock(cell.gameObject).setOnComplete(() => RemoveItem(cell.X, cell.Y));
            cell.InstantiateParticleSystem();
            CheckAndDamageAdjacentObstacles(cell.X, cell.Y, affectedObstacles);
        }

        var (box, stone, vase) = GetObstacleCount();
        
        LevelManager.Instance.UpdateObstacleCountMap(box, stone, vase);
        LevelManager.Instance.SpendMove();
        
        if (createRocket) {
            StartCoroutine(CreateRocketWithDelay(clickPosition.x, clickPosition.y, 0.3f));
        } else {
            StartCoroutine(ProcessFallingItemsAfterDelay(0.3f));
        }
    }

    private IEnumerator CreateRocketWithDelay(int x, int y, float delay) {
        yield return new WaitForSeconds(0.3f);
        
        bool isHorizontal = Random.Range(0, 2) == 0;
        CellItemType rocketType = isHorizontal ? CellItemType.HorizontalRocket : CellItemType.VerticalRocket;
        
        CellItem rocketItem = blockFactory.CreateItem(x, y, rocketType);
        grid[x, y] = rocketItem;
        
        if (rocketItem != null) {
            AnimationManager.Instance.PlayRocketCreation(rocketItem.gameObject);
        }
        
        isFalling = true;
        yield return StartCoroutine(ProcessFallingItems());
        isFalling = false;

        RenderRocketStateSprites();
    }

    /* Animations & New Block Spawning */
    private IEnumerator ProcessFallingItemsAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        
        isFalling = true;
        yield return StartCoroutine(ProcessFallingItems());
        isFalling = false;

        RenderRocketStateSprites();
    }
    
    private IEnumerator ProcessFallingItems() {
        bool itemsMoved;
        
        do {
            itemsMoved = false;
            
            for (int y = 0; y < gridHeight - 1; y++) {
                for (int x = 0; x < gridWidth; x++) { 
                    if (grid[x, y] == null) {
                        int targetY = FindFirstFallableItemAbove(x, y);
                        if (targetY > y) {
                            StartCoroutine(AnimateExistingCubeFall(x, targetY, x, y));
                            itemsMoved = true;
                        }
                    }
                }
            }
                          
            yield return new WaitForSeconds(0.2f);
            
        } while (itemsMoved);
        
        yield return StartCoroutine(SpawnNewCubesAtTop());
    }

    private IEnumerator AnimateExistingCubeFall(int fromX, int fromY, int toX, int toY) {
        CellItem item = grid[fromX, fromY];
        if (item == null) yield break;
        
        grid[fromX, fromY] = null;
        grid[toX, toY] = item;
        
        item.SetPosition(toX, toY);
        
        Vector3 startPos = GetWorldPosition(fromX, fromY);
        Vector3 targetPos = GetWorldPosition(toX, toY);
        
        float speed = AnimationManager.Instance.blockFallSpeed;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / speed;
        
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            if (item == null) yield break;
            
            float t = elapsedTime / duration;
            item.transform.position = AnimationManager.Instance.LerpWithoutClamp(startPos, targetPos, AnimationManager.Instance.EaseOutBack(t));
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        if (item != null) {
            item.transform.position = targetPos;
            
            SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
            if (renderer != null) 
                renderer.sortingOrder = toY + 1;
        }
    }
    
    private IEnumerator SpawnNewCubesAtTop() {
        bool cubesAdded = false;
        
        for (int x = 0; x < gridWidth; x++) {
            List<int> emptyCellsToFill = FindEmptyCellsToFill(x);
            
            if (emptyCellsToFill.Count > 0) {
                foreach (int targetY in emptyCellsToFill) {
                    CellItem newCube = blockFactory.CreateRandomCube(x, targetY, true);
                    grid[x, targetY] = newCube;
                    
                    StartCoroutine(AnimateNewCubeFall(newCube.gameObject, newCube, 
                                                     GetWorldPosition(x, gridHeight), 
                                                     GetWorldPosition(x, targetY)));
                    
                    cubesAdded = true;
                    
                    yield return new WaitForSeconds(0.5f / AnimationManager.Instance.blockFallSpeed);
                }
            }
        }
        
        if (cubesAdded) yield return new WaitForSeconds(0.3f);
    }
        
    private IEnumerator AnimateNewCubeFall(GameObject item, CellItem itemComponent, Vector3 startPos, Vector3 targetPos) {
        if (item == null) yield return null; 

        SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
        if (renderer != null) 
            renderer.sortingOrder = itemComponent.Y + 1;
       
        float speed = AnimationManager.Instance.blockFallSpeed;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / speed;
        
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            
            float t = elapsedTime / duration;
            item.transform.position = AnimationManager.Instance.LerpWithoutClamp(startPos, targetPos, AnimationManager.Instance.EaseOutBack(t));
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        item.transform.position = targetPos;
    }

    private void CheckAndDamageAdjacentObstacles(int x, int y, HashSet<Vector2Int> processedObstacles) {
        foreach (var dir in ADJACENT_DIRECTIONS) {
            int newX = x + dir.x;
            int newY = y + dir.y;
            
            if (IsWithinGrid(newX, newY)) {
                Vector2Int obstaclePos = new Vector2Int(newX, newY);
                
                if (processedObstacles.Contains(obstaclePos)) continue;
                
                if (grid[newX, newY] != null) {
                    CellItem adjacentItem = grid[newX, newY];
                    
                    if (adjacentItem.IsObstacle()) {
                        if (adjacentItem.GetItemType() == CellItemType.Stone) continue;
                        
                        processedObstacles.Add(obstaclePos);
                        
                        bool destroyed = adjacentItem.TakeDamage();
                        if (destroyed) RemoveItem(newX, newY);
                    }
                }
            }
        }
    }

    private void RenderRocketStateSprites() {
        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();

        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                if (grid[x, y] == null || !grid[x, y].IsCube() || visitedPositions.Contains(new Vector2Int(x, y)))
                    continue;

                CellItemType itemType = grid[x, y].GetItemType();
                HashSet<CellItem> connectedCells = FindConnectedCells(x, y, itemType);
                
                foreach (CellItem cell in connectedCells) {
                    visitedPositions.Add(new Vector2Int(cell.X, cell.Y));
                }
                
                if (connectedCells.Count >= 4) {
                    foreach (CellItem cell in connectedCells)
                        cell.RenderRocketSprite();
                } else {
                    foreach (CellItem cell in connectedCells)
                        cell.RenderOriginalSprite();
                }
            }
        }
    }

    private void OnDestroy() {
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                if (grid[x, y] != null) {
                    Destroy(grid[x, y].gameObject);
                }
            }
        }
    }

    /* Helper Methods */
    private bool IsWithinGrid(int x, int y) {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    public Vector3 GetLocalPosition(int x, int y) {
        return new Vector3(
            (x * cellSize) + (cellSize / 2) - (gridWidth * cellSize / 2),       // x
            (y * cellSize) + (cellSize / 2) - (gridHeight * cellSize / 2) - 2f, // y
            0f                                                                  // z
        );
    }
    
    public Vector3 GetWorldPosition(int x, int y) {
        Vector3 localPos = GetLocalPosition(x, y);    
        return transform.TransformPoint(localPos);
    }

    private List<int> FindEmptyCellsToFill(int x) {
        List<int> emptyCells = new List<int>();
        
        List<int> obstaclePositions = new List<int>();
        for (int y = 0; y < gridHeight; y++) {
            if (grid[x, y] != null && !grid[x, y].CanFall()) 
                obstaclePositions.Add(y);
        }
        
        if (obstaclePositions.Count == 0) {
            for (int y = 0; y < gridHeight; y++) {
                if (grid[x, y] == null)
                    emptyCells.Add(y);
            }
            return emptyCells;
        }
        
        int highestObstacle = -1;
        foreach (int obstacleY in obstaclePositions) {
            if (obstacleY > highestObstacle) 
                highestObstacle = obstacleY;
        }
        
        for (int y = highestObstacle + 1; y < gridHeight; y++)
            if (grid[x, y] == null) emptyCells.Add(y);
        
        return emptyCells;
    }

    private int FindFirstFallableItemAbove(int x, int emptyY) {
        for (int checkY = emptyY + 1; checkY < gridHeight; checkY++) {
            if (grid[x, checkY] == null) 
                continue;
            
            if (grid[x, checkY] != null && !grid[x, checkY].CanFall()) {
                return -1;
            }
            
            if (grid[x, checkY] != null && grid[x, checkY].CanFall()) {
                return checkY;
            }
        }
        
        return -1; // No item found that can fall
    }

    private (int box, int stone, int vase) GetObstacleCount() {
        int box = 0, stone = 0, vase = 0;

        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                CellItem item = grid[x, y];
                if (item == null || !item.IsObstacle()) continue;

                switch (item.GetItemType()) {
                    case CellItemType.Box:
                        box += 1;
                        break;
                    case CellItemType.Stone:
                        stone += 1;
                        break;
                    case CellItemType.Vase:
                        vase += 1;
                        break;
                }
            }
        }

        return (box, stone, vase);
    }

    private HashSet<CellItem> FindConnectedCells(int startX, int startY, CellItemType targetType) {
        HashSet<CellItem> connectedCubes = new HashSet<CellItem>();
        HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        
        Vector2Int startPos = new Vector2Int(startX, startY);
        CellItem startItem = grid[startX, startY];
        
        queue.Enqueue(startPos);
        visitedPositions.Add(startPos);
        connectedCubes.Add(startItem);
        
        while (queue.Count > 0) {
            Vector2Int current = queue.Dequeue();
            
            foreach (var dir in ADJACENT_DIRECTIONS) {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;
                
                if (IsWithinGrid(newX, newY)) {
                    Vector2Int newPos = new Vector2Int(newX, newY);
                    if (!visitedPositions.Contains(newPos) && grid[newX, newY] != null) {
                        CellItem adjacentItem = grid[newX, newY];
                        if (adjacentItem.GetItemType() == targetType) {
                            visitedPositions.Add(newPos);
                            connectedCubes.Add(adjacentItem);
                            queue.Enqueue(newPos);
                        }
                    }
                }
            }
        }
        
        return connectedCubes;
    }
}