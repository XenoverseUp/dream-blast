using System.Collections.Generic;
using UnityEngine;


public class Board : MonoBehaviour {
    private GameObject particleSystemPrefab;
    
    private Sprite redCubeSprite;
    private Sprite greenCubeSprite;
    private Sprite blueCubeSprite;
    private Sprite yellowCubeSprite;
    private Sprite horizontalRocketSprite;
    private Sprite verticalRocketSprite;
    private Sprite boxSprite;
    private Sprite stoneSprite;
    private Sprite vaseSprite;
    private Sprite damagedVaseSprite;

    private Sprite blueCrack;
    private Sprite redCrack;
    private Sprite yellowCrack;
    private Sprite greenCrack;
    
    private int gridWidth;
    private int gridHeight;
    private float cellSize;
    private CellItem[,] grid;
    
    public void SetBlockSprites(
        Sprite redCube, 
        Sprite greenCube, 
        Sprite blueCube, 
        Sprite yellowCube,
        Sprite horizontalRocket,
        Sprite verticalRocket,
        Sprite box,
        Sprite stone,
        Sprite vase,
        Sprite damagedVase) {
        
        this.redCubeSprite = redCube;
        this.greenCubeSprite = greenCube;
        this.blueCubeSprite = blueCube;
        this.yellowCubeSprite = yellowCube;
        this.horizontalRocketSprite = horizontalRocket;
        this.verticalRocketSprite = verticalRocket;
        this.boxSprite = box;
        this.stoneSprite = stone;
        this.vaseSprite = vase;
        this.damagedVaseSprite = damagedVase;
    }
    
    public void SetCrackSprites(Sprite redCrack, Sprite greenCrack, Sprite blueCrack, Sprite yellowCrack) {
        this.redCrack = redCrack;
        this.greenCrack = greenCrack;
        this.blueCrack = blueCrack;
        this.yellowCrack = yellowCrack;
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
    
    private void InitializeGrid() {
        grid = new CellItem[this.gridWidth, this.gridHeight];

        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                grid[x, y] = null;
            }
        }
    }

    private void PopulateGrid(List<string> levelGrid) {
        for (int i = 0; i < levelGrid.Count; i++) {
            int x = i % gridWidth;
            int y = Mathf.FloorToInt(i / gridWidth);
            
            string itemType = levelGrid[i];
            SpawnItemFromType(x, y, itemType);
        }
    }

    private void SpawnItemFromType(int x, int y, string itemType) {
        CellItemType type = CellItemType.Empty;
        Sprite sprite = null;
        
        switch (itemType) {
            case "r":
                sprite = redCubeSprite;
                type = CellItemType.RedCube;
                break;
            case "g":
                sprite = greenCubeSprite;
                type = CellItemType.GreenCube;
                break;
            case "b":
                sprite = blueCubeSprite;
                type = CellItemType.BlueCube;
                break;
            case "y":
                sprite = yellowCubeSprite;
                type = CellItemType.YellowCube;
                break;
            case "rand":
                string[] colors = { "r", "g", "b", "y" };
                SpawnItemFromType(x, y, colors[UnityEngine.Random.Range(0, colors.Length)]);
                return;
            case "vro":
                sprite = verticalRocketSprite;
                type = CellItemType.VerticalRocket;
                break;
            case "hro":
                sprite = horizontalRocketSprite;
                type = CellItemType.HorizontalRocket;
                break;
            case "bo":
                sprite = boxSprite;
                type = CellItemType.Box;
                break;
            case "s":
                sprite = stoneSprite;
                type = CellItemType.Stone;
                break;
            case "v":
                sprite = vaseSprite;
                type = CellItemType.Vase;
                break;
            default:
                return; // Empty cell
        }
        
        SpawnItem(x, y, type, sprite);
    }
    
    public void SpawnItem(int x, int y, CellItemType type, Sprite sprite) {
        Vector3 position = this.GetLocalPosition(x, y);
        
        GameObject item = new GameObject("Block_" + type.ToString());
        
        RectTransform itemRect = item.AddComponent<RectTransform>();
        itemRect.anchoredPosition = new Vector2(position.x, position.y);
        itemRect.sizeDelta = new Vector2(cellSize, cellSize);
        
        item.transform.SetParent(transform, false);
        
        SpriteRenderer spriteRenderer = item.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = y + 1;
        
        this.ScaleSpriteToFit(item, sprite, cellSize);
        
        CellItem itemComponent = item.AddComponent<CellItem>();
        itemComponent.Initialize(type, x, y, sprite, particleSystemPrefab, GetCrackSpriteForItemType(type));
        
        if (type == CellItemType.Vase) itemComponent.SetDamagedSprite(damagedVaseSprite);
        
        grid[x, y] = itemComponent;
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
    
    public bool TryBlast(int x, int y) {
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
            
            HashSet<Vector2Int> affectedObstacles = new HashSet<Vector2Int>();
            foreach (CellItem cell in connectedCells) {
                AnimationManager.Instance.PlayDestroyBlock(cell.gameObject)
                    .setOnComplete(() => RemoveItem(cell.X, cell.Y));

                cell.InstantiateParticleSystem();

                CheckAndDamageAdjacentObstacles(cell.X, cell.Y, affectedObstacles);
                
                Vector3 position = GetWorldPosition(cell.X, cell.Y);
            }

            var (box, stone, vase) = GetObstacleCount();
            
            LevelManager.Instance.UpdateObstacleCountMap(box, stone, vase);
            LevelManager.Instance.SpendMove();
            return true;
        } else if (item.IsRocket()) {
            Debug.Log("Rocket clicked, but rocket functionality is disabled");
            return false;
        }
        
        return false;
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
        
        Vector2Int[] directions = new Vector2Int[] {
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, 0)
        };
        
        while (queue.Count > 0) {
            Vector2Int current = queue.Dequeue();
            
            foreach (var dir in directions) {
                int newX = current.x + dir.x;
                int newY = current.y + dir.y;
                
                if (newX >= 0 && newX < gridWidth && newY >= 0 && newY < gridHeight) {
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

    private void CheckAndDamageAdjacentObstacles(int x, int y, HashSet<Vector2Int> processedObstacles) {
        Vector2Int[] directions = new Vector2Int[] {
            new(0, 1),
            new(1, 0),
            new(0, -1),
            new(-1, 0)
        };
        
        foreach (var dir in directions) {
            int newX = x + dir.x;
            int newY = y + dir.y;
            
            if (newX >= 0 && newX < gridWidth && newY >= 0 && newY < gridHeight) {
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
    
    private Sprite GetCrackSpriteForItemType(CellItemType itemType) {
        return itemType switch {
            CellItemType.RedCube => redCrack,
            CellItemType.GreenCube => greenCrack,
            CellItemType.BlueCube => blueCrack,
            CellItemType.YellowCube => yellowCrack,
            _ => blueCrack,
        };
    }

    private (int box, int stone, int vase) GetObstacleCount() {
        int box = 0, stone = 0, vase = 0;


        for (int x = 0; x < gridWidth; x++) {
            for (int y = 0; y < gridHeight; y++) {
                CellItem item = grid[x, y];
                if (item == null || !item.IsObstacle()) continue;

                if (item.GetItemType() == CellItemType.Box) box += 1;
                else if (item.GetItemType() == CellItemType.Stone) stone += 1;
                else if (item.GetItemType() == CellItemType.Vase) vase += 1;
            }
        }

        return (box, stone, vase);
    }

    private void RemoveItem(int x, int y) {
        if (grid[x, y] != null) {
            CellItem item = grid[x, y];
            Destroy(item.gameObject);
            grid[x, y] = null;
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
}