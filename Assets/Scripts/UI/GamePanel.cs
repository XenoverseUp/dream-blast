using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour {

    [SerializeField] private Sprite boardSprite;
    [SerializeField] private Sprite redCubeSprite;
    [SerializeField] private Sprite greenCubeSprite;
    [SerializeField] private Sprite blueCubeSprite;
    [SerializeField] private Sprite yellowCubeSprite;
    [SerializeField] private Sprite horizontalRocketSprite;
    [SerializeField] private Sprite verticalRocketSprite;
    [SerializeField] private Sprite boxSprite;
    [SerializeField] private Sprite stoneSprite;
    [SerializeField] private Sprite vaseSprite;
    [SerializeField] private Sprite damagedVaseSprite;
    [SerializeField] private float padding = 0.12f; // Padding percentage

    private int gridWidth, gridHeight;
    private RectTransform rect;
    private GameObject boardPanel;

    public void Start() {
        this.gridWidth = LevelManager.Instance.GetLevelData().GridWidth;
        this.gridHeight = LevelManager.Instance.GetLevelData().GridHeight;

        this.rect = GetComponent<RectTransform>();
        
        float cellSize = CalculateOptimalGridSize();
        CreateBoard(cellSize);
    }

    private float CalculateOptimalGridSize() {
        float availableWidth = rect.rect.width * (1.0f - padding);
        float availableHeight = rect.rect.height * (1.0f - padding);
        
        float cellSizeFromWidth = availableWidth / gridWidth;
        float cellSizeFromHeight = availableHeight / gridHeight;
        
        return Mathf.Min(cellSizeFromWidth, cellSizeFromHeight);
    }

    private void CreateBoard(float cellSize) {
        boardPanel = new GameObject("Board");
        boardPanel.transform.SetParent(this.transform, false);
        
        RectTransform boardRect = boardPanel.AddComponent<RectTransform>();
        
        float boardWidth = cellSize * gridWidth;
        float boardHeight = cellSize * gridHeight;
        
        boardRect.sizeDelta = new Vector2(boardWidth, boardHeight);
        
        boardRect.anchorMin = new Vector2(0.5f, 0.5f);
        boardRect.anchorMax = new Vector2(0.5f, 0.5f);
        boardRect.pivot = new Vector2(0.5f, 0.5f);
        boardRect.anchoredPosition = Vector2.zero;
        
        Image boardImage = boardPanel.AddComponent<Image>();
        
        if (boardSprite != null) {
            boardImage.sprite = boardSprite;
            boardImage.type = Image.Type.Sliced; 
            boardImage.raycastTarget = true; 
        } else {
            boardImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); 
        }

        Board boardScript = boardPanel.AddComponent<Board>();
        
        boardScript.SetSprites(
            redCubeSprite,
            greenCubeSprite,
            blueCubeSprite,
            yellowCubeSprite,
            horizontalRocketSprite,
            verticalRocketSprite,
            boxSprite,
            stoneSprite,
            vaseSprite,
            damagedVaseSprite
        );

        boardScript.Initialize(cellSize);
    }
    
}