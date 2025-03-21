using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {
    private GameObject particleSystemPrefab;

    /* Sprite Containers */
    private Dictionary<CellItemType, Sprite> cubeSprites = new Dictionary<CellItemType, Sprite>();
    private Dictionary<CellItemType, Sprite> rocketStateSprites = new Dictionary<CellItemType, Sprite>();
    private Dictionary<CellItemType, Sprite> crackSprites = new Dictionary<CellItemType, Sprite>();
    private Dictionary<CellItemType, Sprite> obstacleSprites = new Dictionary<CellItemType, Sprite>();
    private Sprite damagedVaseSprite;
    private Sprite horizontalRocketSprite, verticalRocketSprite;

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

    /* Setters and Initializers */
    public void SetBlockSprites(Sprite redCube, Sprite greenCube, Sprite blueCube, Sprite yellowCube) {
        cubeSprites[CellItemType.RedCube] = redCube;
        cubeSprites[CellItemType.GreenCube] = greenCube;
        cubeSprites[CellItemType.BlueCube] = blueCube;
        cubeSprites[CellItemType.YellowCube] = yellowCube;
    }

    public void SetRocketStateSprites(
        Sprite redRocketStateSprite, 
        Sprite greenRocketStateSprite, 
        Sprite blueRocketStateSprite, 
        Sprite yellowRocketStateSprite
    ) {
        rocketStateSprites[CellItemType.RedCube] = redRocketStateSprite;
        rocketStateSprites[CellItemType.GreenCube] = greenRocketStateSprite;
        rocketStateSprites[CellItemType.BlueCube] = blueRocketStateSprite;
        rocketStateSprites[CellItemType.YellowCube] = yellowRocketStateSprite;
    }

    public void SetObstacleSprites(Sprite box, Sprite stone, Sprite vase, Sprite damagedVase) {
        obstacleSprites[CellItemType.Box] = box;
        obstacleSprites[CellItemType.Stone] = stone;
        obstacleSprites[CellItemType.Vase] = vase;
        this.damagedVaseSprite = damagedVase;
    }
    
    public void SetCrackSprites(Sprite redCrack, Sprite greenCrack, Sprite blueCrack, Sprite yellowCrack) {
        crackSprites[CellItemType.RedCube] = redCrack;
        crackSprites[CellItemType.GreenCube] = greenCrack;
        crackSprites[CellItemType.BlueCube] = blueCrack;
        crackSprites[CellItemType.YellowCube] = yellowCrack;
    }

    public void SetArtifactSprites(Sprite horizontalRocket, Sprite verticalRocket) {
        this.horizontalRocketSprite = horizontalRocket;
        this.verticalRocketSprite = verticalRocket;
    }
    
    public void SetParticleSystemPrefab(GameObject prefab) {
        this.particleSystemPrefab = prefab;
    }
    
    public void Initialize(float cellSize) {
        this.cellSize = cellSize;
        this.gridWidth = LevelManager.Instance.GetLevelData().GridWidth;
        this.gridHeight = LevelManager.Instance.GetLevelData().GridHeight;
        
        InitializeGrid();
        PopulateGrid(LevelManager.Instance.GetLevelData().Grid);
    }

    /* Grid Actions */
    private void InitializeGrid() {
        grid = new CellItem[gridWidth, gridHeight];
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
        CellItemType type = CellItemType.Empty;
        Sprite sprite = null;
        Sprite crackSprite = null;
        Sprite rocketStateSprite = null;
        
        switch (itemType) {
            case "r":
                type = CellItemType.RedCube;
                break;
            case "g":
                type = CellItemType.GreenCube;
                break;
            case "b":
                type = CellItemType.BlueCube;
                break;
            case "y":
                type = CellItemType.YellowCube;
                break;
            case "rand":
                string[] colors = { "r", "g", "b", "y" };
                SpawnItemFromType(x, y, colors[UnityEngine.Random.Range(0, colors.Length)]);
                return;
            case "vro":
                type = CellItemType.VerticalRocket;
                sprite = verticalRocketSprite;
                break;
            case "hro":
                type = CellItemType.HorizontalRocket;
                sprite = horizontalRocketSprite;
                break;
            case "bo":
                type = CellItemType.Box;
                sprite = obstacleSprites[CellItemType.Box];
                break;
            case "s":
                type = CellItemType.Stone;
                sprite = obstacleSprites[CellItemType.Stone];
                break;
            case "v":
                type = CellItemType.Vase;
                sprite = obstacleSprites[CellItemType.Vase];
                break;
            default:
                return; // Empty cell
        }

        // For cubes, get sprites from the dictionaries
        if (IsCubeType(type)) {
            sprite = cubeSprites[type];
            crackSprite = crackSprites[type];
            rocketStateSprite = rocketStateSprites[type];
        }
        
        SpawnItem(x, y, type, sprite, crackSprite, rocketStateSprite);
    }
    
    private CellItem CreateCellItem(int x, int y, CellItemType type, Sprite sprite, Sprite crackSprite, Sprite rocketStateSprite, bool isNewCube = false) {
        Vector3 position = isNewCube ? GetWorldPosition(x, gridHeight) : GetLocalPosition(x, y);
        
        GameObject item = new GameObject($"Block_{type}");
        
        RectTransform itemRect = item.AddComponent<RectTransform>();
        if (!isNewCube) {
            itemRect.anchoredPosition = new Vector2(position.x, position.y);
        }
        itemRect.sizeDelta = new Vector2(cellSize, cellSize);
        
        item.transform.SetParent(transform, false);
        if (isNewCube) {
            item.transform.position = position;
        }
        
        SpriteRenderer spriteRenderer = item.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = y + 1;
        
        ScaleSpriteToFit(item, sprite, cellSize);
        
        CellItem itemComponent = item.AddComponent<CellItem>();
        itemComponent.SetSprites(sprite, crackSprite);
        itemComponent.SetParticleSystemPrefab(particleSystemPrefab);
        
        if (type == CellItemType.Vase) {
            itemComponent.SetDamagedSprite(damagedVaseSprite);
        }
        
        itemComponent.Initialize(type, x, y);

        if (IsCubeType(type)) {
            itemComponent.SetRocketStateSprite(rocketStateSprite);
        }
              
        return itemComponent;
    }
    
    public void SpawnItem(int x, int y, CellItemType type, Sprite sprite, Sprite crackSprite, Sprite rocketStateSprite) {
        if (type == CellItemType.Empty) return;

        CellItem itemComponent = CreateCellItem(x, y, type, sprite, crackSprite, rocketStateSprite);
        grid[x, y] = itemComponent;
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
            
            ProcessConnectedCubes(connectedCells);
            return true;
        } else if (item.IsRocket()) {
            Debug.Log("Rocket clicked, but rocket functionality is disabled");
            return false;
        }
        
        return false;
    }

    private void ProcessConnectedCubes(HashSet<CellItem> connectedCells) {
        HashSet<Vector2Int> affectedObstacles = new HashSet<Vector2Int>();
        foreach (CellItem cell in connectedCells) {
            AnimationManager.Instance.PlayDestroyBlock(cell.gameObject).setOnComplete(() => RemoveItem(cell.X, cell.Y));
            cell.InstantiateParticleSystem();
            CheckAndDamageAdjacentObstacles(cell.X, cell.Y, affectedObstacles);
        }

        var (box, stone, vase) = GetObstacleCount();
        
        LevelManager.Instance.UpdateObstacleCountMap(box, stone, vase);
        LevelManager.Instance.SpendMove();
                    
        StartCoroutine(ProcessFallingItemsAfterDelay(0.3f));
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
                    CellItemType cubeType = GetRandomCubeType();
                    
                    Sprite cubeSprite = cubeSprites[cubeType];
                    Sprite crackSprite = crackSprites[cubeType];
                    Sprite rocketStateSprite = rocketStateSprites[cubeType];
                    
                    CellItem itemComponent = CreateCellItem(x, targetY, cubeType, cubeSprite, crackSprite, rocketStateSprite, true);
                    grid[x, targetY] = itemComponent;
                    
                    StartCoroutine(AnimateNewCubeFall(itemComponent.gameObject, itemComponent, 
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
    private CellItemType GetRandomCubeType() {
        CellItemType[] cubeTypes = new CellItemType[] {
            CellItemType.RedCube,
            CellItemType.GreenCube,
            CellItemType.BlueCube,
            CellItemType.YellowCube
        };
        
        return cubeTypes[UnityEngine.Random.Range(0, cubeTypes.Length)];
    }

    private bool IsCubeType(CellItemType type) {
        return type == CellItemType.RedCube || 
               type == CellItemType.GreenCube || 
               type == CellItemType.BlueCube || 
               type == CellItemType.YellowCube;
    }

    private bool IsWithinGrid(int x, int y) {
        return x >= 0 && x < gridWidth && y >= 0 && y < gridHeight;
    }

    private void ScaleSpriteToFit(GameObject item, Sprite sprite, float targetSize) {
        if (sprite == null) return;
        
        float spriteWidth = sprite.bounds.size.x;
        float scale = targetSize / spriteWidth;

        item.transform.localScale = new Vector3(scale, scale, 1f);
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