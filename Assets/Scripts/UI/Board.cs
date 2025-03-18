using System.Collections.Generic;
using UnityEngine;

public class Cell {
    private int x;
    private int y;
    private CellItem item;
    
    public Cell(int x, int y) {
        this.x = x;
        this.y = y;
        this.item = null;
    }
    
    public bool IsEmpty() {
        return item == null;
    }
    
    public CellItem GetItem() { return item; }
    public void SetItem(CellItem item) { this.item = item; }
    public void Clear() { this.item = null; }
}

public enum CellItemType {
    Empty,
    RedCube,
    GreenCube,
    BlueCube,
    YellowCube,
    HorizontalRocket,
    VerticalRocket,
    Box,
    Stone,
    Vase
}

public class Board : MonoBehaviour {
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
    
    private int gridWidth;
    private int gridHeight;
    private float cellSize;
    private Cell[,] grid;
    private RectTransform rectTransform;
    
    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }
    
    public void SetSprites(
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
    
    public void Initialize(float cellSize) {
        this.cellSize = cellSize;

        this.gridWidth = LevelManager.Instance.GetLevelData().GridWidth;
        this.gridHeight = LevelManager.Instance.GetLevelData().GridHeight;
        
        InitializeGrid();
        PopulateGrid(LevelManager.Instance.GetLevelData().Grid);
    }
    
    private void InitializeGrid() {
        grid = new Cell[this.gridWidth, this.gridHeight];

        for (int x = 0; x < gridWidth; x++) 
            for (int y = 0; y < gridHeight; y++) 
                grid[x, y] = new Cell(x, y);
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
        Vector3 position = GetWorldPosition(x, y);
        
        GameObject item = new GameObject("Item_" + type.ToString());
        
        RectTransform itemRect = item.AddComponent<RectTransform>();
        itemRect.anchoredPosition = new Vector2(position.x, position.y);
        itemRect.sizeDelta = new Vector2(cellSize * 0.9f, cellSize * 0.9f);
        
        item.transform.SetParent(transform, false);
        
        SpriteRenderer spriteRenderer = item.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = y + 1;
        
        ScaleSpriteToFit(item, sprite, cellSize);
        
        CellItem itemComponent = item.AddComponent<CellItem>();
        itemComponent.Initialize(type, x, y, sprite);
        
        if (type == CellItemType.Vase) {
            itemComponent.SetDamagedSprite(damagedVaseSprite);
        }
        
        BoxCollider2D collider = item.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(cellSize, cellSize);
        grid[x, y].SetItem(itemComponent);
    }
    
    private void ScaleSpriteToFit(GameObject item, Sprite sprite, float targetSize) {
        if (sprite == null) return;
        
        float spriteWidth = sprite.bounds.size.x;
        float scale = targetSize / spriteWidth;

        item.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private Vector2 GetWorldPosition(int x, int y) {
        
        return new Vector2(
            (x * cellSize) + (cellSize / 2) - (gridWidth * cellSize / 2),  // x
            (y * cellSize) + (cellSize / 2) - (gridHeight * cellSize / 2)  // y
        );
    }
    
    public bool TryBlast(int x, int y) {
        return false;
    }

    private void OnDestroy() {
        for (int x = 0; x < gridWidth; x++) 
            for (int y = 0; y < gridHeight; y++) 
                if (!grid[x, y].IsEmpty()) Destroy(grid[x, y].GetItem().gameObject);
    }
}