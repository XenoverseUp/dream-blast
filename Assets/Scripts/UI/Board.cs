using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
    private BlockFactory blockFactory;
    
    private int gridWidth;
    private int gridHeight;
    private float cellSize;
    private CellItem[,] grid;
    
    /* Direction Constants */
    private static readonly Vector2Int[] ADJACENT_DIRECTIONS = new Vector2Int[] {
        new(0, 1), 
        new(1, 0), 
        new(0, -1),
        new(-1, 0) 
    };

    public int GridHeight => gridHeight;
    public int GridWidth => gridWidth;

    /* Initialization & Destroy */
    public void Initialize(float cellSize) {
        this.cellSize = cellSize;
        this.gridWidth = LevelManager.Instance.GetLevelData().GridWidth;
        this.gridHeight = LevelManager.Instance.GetLevelData().GridHeight;
        
        InitializeGrid();
        blockFactory.Initialize(this, cellSize);
        PopulateGrid(LevelManager.Instance.GetLevelData().Grid);
        
        if (EventManager.Instance != null) {
            EventManager.Instance.OnBoardUnlocked += OnBoardUnlocked;
        }
    }

    private void OnDestroy() {
        if (EventManager.Instance != null) {
            EventManager.Instance.OnBoardUnlocked -= OnBoardUnlocked;
        }
        
        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                if (grid[x, y] != null) {
                    Destroy(grid[x, y].gameObject);
                }
            }
        }
    }

    private void  OnBoardUnlocked() {
        var (box, stone, vase) = GetObstacleCount();
        LevelManager.Instance.UpdateObstacleCountMap(box, stone, vase);
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

        UpdateRocketStateSprites();
    }

    public CellItem GetItem(int x, int y) {
        if (!IsWithinGrid(x, y)) return null;
        return grid[x, y];
    }

    public void RemoveItem(int x, int y) {
        if (!IsWithinGrid(x, y)) return;
        if (grid[x, y] != null) {
            CellItem item = grid[x, y];
            Destroy(item.gameObject);
            grid[x, y] = null;
        }
    }

    public bool SetItem(int x, int y, CellItem item) {
        if (!IsWithinGrid(x, y)) return false;
        
        if (grid[x, y] != null) 
            RemoveItem(x, y);
        
        grid[x, y] = item;
        item?.SetPosition(x, y);
        
        return true;
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
        if (!BoardManager.Instance.IsInteractable()) {
            Debug.Log("Board is currently locked");
            return false;
        }
        
        if (!IsWithinGrid(x, y) || grid[x, y] == null) {
            Debug.Log("Invalid cell or empty");
            return false;
        }
        
        CellItem item = grid[x, y];

        if (item.IsObstacle()) {
            AnimationManager.Instance.PlayInvalidBlast(item.gameObject);
            return false;
        }

        if (item.IsCube()) {
            return TryBlastCubes(x, y);
        } else if (item.IsRocket()) {
            return TryActivateRocket(x, y);
        }
        
        return false;
    }
    
    private bool TryBlastCubes(int x, int y) {
        CellItem item = grid[x, y];
        CellItemType itemType = item.GetItemType();
        HashSet<CellItem> connectedCells = FindConnectedCells(x, y, itemType);
        
        if (connectedCells.Count < 2) {
            AnimationManager.Instance.PlayInvalidBlast(item.gameObject);
            return false;
        }
        
        bool shouldCreateRocket = connectedCells.Count >= 4;
        Vector2Int clickPosition = new(x, y);
        
        ProcessConnectedCubes(connectedCells, clickPosition, shouldCreateRocket, itemType);
        
        BoardManager.Instance.SetState(BoardState.Processing);
        
        return true;
    }
    
    private bool TryActivateRocket(int x, int y) {
        List<Vector2Int> adjacentRockets = FindAdjacentRockets(x, y);
        
        if (adjacentRockets.Count > 0) {
            StartCoroutine(CreateRocketCombo(x, y, adjacentRockets));
        } else {
            CellItemType itemType = grid[x, y].GetItemType();
            RocketEffect.RocketDirection direction = itemType == CellItemType.HorizontalRocket ? 
                RocketEffect.RocketDirection.Horizontal : 
                RocketEffect.RocketDirection.Vertical;
            
            RemoveItem(x, y);
            EventManager.Instance?.TriggerRocketActivated(GetWorldPosition(x, y), direction);
        }
        
        BoardManager.Instance.SetState(BoardState.Processing);
        return true;
    }

    private void ProcessConnectedCubes(HashSet<CellItem> connectedCells, Vector2Int clickPosition, bool createRocket, CellItemType sourceType) {
        HashSet<Vector2Int> affectedObstacles = new HashSet<Vector2Int>();
        
        foreach (CellItem cell in connectedCells) {
            AnimationManager.Instance.PlayDestroyBlock(cell.gameObject).setOnComplete(() => RemoveItem(cell.X, cell.Y));
            cell.InstantiateParticleSystem();
            CheckAndDamageAdjacentObstacles(cell.X, cell.Y, affectedObstacles);
        }
        
        if (createRocket) {
            StartCoroutine(CreateRocketWithDelay(clickPosition.x, clickPosition.y, 0.3f));
        } else {
            StartCoroutine(ProcessFallingItemsAfterDelay(0.3f));
        }
    }

    private IEnumerator CreateRocketWithDelay(int x, int y, float delay) {
        yield return new WaitForSeconds(delay);
        
        bool isHorizontal = Random.Range(0, 2) == 0;
        CellItemType rocketType = isHorizontal ? CellItemType.HorizontalRocket : CellItemType.VerticalRocket;
        
        CellItem rocketItem = blockFactory.CreateItem(x, y, rocketType);
        grid[x, y] = rocketItem;
        
        if (rocketItem != null) {
            AnimationManager.Instance.PlayRocketCreation(rocketItem.gameObject);
        }
        
        yield return StartCoroutine(ProcessFallingItems());
        EventManager.Instance?.TriggerFallingComplete();
    }
    
    private IEnumerator CreateRocketCombo(int x, int y, List<Vector2Int> rocketPositions) {
        Vector3 centerPos = GetWorldPosition(x, y);
        
        RemoveItem(x, y);
        
        foreach (Vector2Int pos in rocketPositions) {
            AnimationManager.Instance.PlayDestroyBlock(grid[pos.x, pos.y].gameObject).setOnComplete(() => RemoveItem(pos.x, pos.y));
        }

        EventManager.Instance?.TriggerRocketActivated(centerPos, RocketEffect.RocketDirection.Horizontal);
        EventManager.Instance?.TriggerRocketActivated(centerPos, RocketEffect.RocketDirection.Vertical);
        
        Vector2Int[] plusShape = new Vector2Int[] {
            new(-1, 0), new(1, 0),
            new(0, -1), new(0, 1)
        };
        
        foreach (Vector2Int offset in plusShape) {
            int newX = x + offset.x;
            int newY = y + offset.y;
            
            if (IsWithinGrid(newX, newY)) {
                Vector3 rocketPos = GetWorldPosition(newX, newY);
                
                RocketEffect.RocketDirection direction = offset.y == 0 ? 
                    RocketEffect.RocketDirection.Vertical : 
                    RocketEffect.RocketDirection.Horizontal;
                    
                EventManager.Instance?.TriggerRocketActivated(rocketPos, direction);
            }
        }
        
        yield return null;
    }
    
    private List<Vector2Int> FindAdjacentRockets(int x, int y) {
        List<Vector2Int> positions = new List<Vector2Int>();
        
        foreach (Vector2Int dir in ADJACENT_DIRECTIONS) {
            int newX = x + dir.x;
            int newY = y + dir.y;
            
            if (IsWithinGrid(newX, newY) && grid[newX, newY] != null && grid[newX, newY].IsRocket()) {
                positions.Add(new Vector2Int(newX, newY));
            }
        }
        
        return positions;
    }

    /* Animations & New Block Spawning */
    public IEnumerator ProcessFallingItemsAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        
        BoardManager.Instance.SetState(BoardState.Falling);
        yield return StartCoroutine(ProcessFallingItems());
        
        EventManager.Instance?.TriggerFallingComplete();
    }
    
    public IEnumerator ProcessFallingItems() {
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
                          
            yield return new WaitForSeconds(AnimationManager.Instance.blockSpawnDelay);
            
        } while (itemsMoved);
        
        yield return StartCoroutine(SpawnNewCubesAtTop());
        UpdateRocketStateSprites();
    }

    private IEnumerator AnimateExistingCubeFall(int fromX, int fromY, int toX, int toY) {
        CellItem item = grid[fromX, fromY];
        if (item == null) yield break;
        
        grid[fromX, fromY] = null;
        grid[toX, toY] = item;
        
        item.SetPosition(toX, toY);
        
        Vector3 startPos = GetWorldPosition(fromX, fromY);
        Vector3 targetPos = GetWorldPosition(toX, toY);
        
        float duration = AnimationManager.Instance.AnimateCubeFall(item.gameObject, startPos, targetPos);
        yield return new WaitForSeconds(duration);
    }

    private IEnumerator SpawnNewCubesAtTop() {
        bool cubesAdded = false;
        List<Coroutine> fallAnimations = new List<Coroutine>();
        
        for (int x = 0; x < gridWidth; x++) {
            List<int> emptyCellsToFill = FindEmptyCellsToFill(x);
            
            if (emptyCellsToFill.Count > 0) {
                fallAnimations.Add(StartCoroutine(AnimateColumnFall(x, emptyCellsToFill)));
                cubesAdded = true;
            }
        }
        
        foreach (var coroutine in fallAnimations) {
            yield return coroutine;
        }
        
        if (cubesAdded) yield return new WaitForSeconds(AnimationManager.Instance.rocketStateUpdateDelay);
    }
    
    private IEnumerator AnimateColumnFall(int x, List<int> emptyCellsToFill) {
        if (emptyCellsToFill.Count == 0) yield break;
        
        float fallDistance = Vector3.Distance(
            GetWorldPosition(x, gridHeight),
            GetWorldPosition(x, emptyCellsToFill[0])
        );
        
        float maxFallDuration = (emptyCellsToFill.Count - 1) * (0.4f / AnimationManager.Instance.blockFallSpeed) + 
                               fallDistance / AnimationManager.Instance.blockFallSpeed;
        
        for (int i = 0; i < emptyCellsToFill.Count; i++) {
            int targetY = emptyCellsToFill[i];
            float spawnDelay = i * (0.4f / AnimationManager.Instance.blockFallSpeed);
            
            StartCoroutine(CreateAndAnimateCube(x, targetY, spawnDelay));
        }
        
        yield return new WaitForSeconds(maxFallDuration);
    }
    
    private IEnumerator CreateAndAnimateCube(int x, int targetY, float delay) {
        yield return new WaitForSeconds(delay);
        
        CellItem newCube = blockFactory.CreateRandomCube(x, targetY, true);
        grid[x, targetY] = newCube;
        
        AnimationManager.Instance.AnimateCubeFall(
            newCube.gameObject,
            GetWorldPosition(x, gridHeight),
            GetWorldPosition(x, targetY)
        );
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
                        
                        adjacentItem.DamageObstacle();
                    }
                }
            }
        }
    }

    public void UpdateRocketStateSprites() {
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
                
                if (connectedCells.Count >= 4) 
                    foreach (CellItem cell in connectedCells) cell.RenderRocketStateSprite();
                else 
                    foreach (CellItem cell in connectedCells) cell.RenderOriginalSprite();
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
}