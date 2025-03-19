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

    [SerializeField] private GameObject particleSystemPrefab;

    [SerializeField] private float padding = 0.12f; // Padding percentage

    private int gridWidth, gridHeight;
    private RectTransform rect;
    private GameObject outerPanel;  
    private GameObject boardPanel; 

    public void Start() {
        this.gridWidth = LevelManager.Instance.GetLevelData().GridWidth;
        this.gridHeight = LevelManager.Instance.GetLevelData().GridHeight;

        this.rect = GetComponent<RectTransform>();
        
        float cellSize = this.CalculateOptimalGridSize();
        this.CreateBoard(cellSize);
    }

    private float CalculateOptimalGridSize() {
        float availableWidth = rect.rect.width * (1.0f - padding);
        float availableHeight = rect.rect.height * (1.0f - padding);
        
        float cellSizeFromWidth = availableWidth / gridWidth;
        float cellSizeFromHeight = availableHeight / gridHeight;
        
        return Mathf.Min(cellSizeFromWidth, cellSizeFromHeight);
    }

    private void CreateBoard(float cellSize) {
        outerPanel = new GameObject("OuterPanel");
        outerPanel.transform.SetParent(this.transform, false);
        
        RectTransform outerRect = outerPanel.AddComponent<RectTransform>();
        
        float boardWidth = cellSize * gridWidth;
        float boardHeight = cellSize * gridHeight;
        float outerPadding = 15.0f;
        
        outerRect.sizeDelta = new Vector2(boardWidth + outerPadding * 2, boardHeight + outerPadding * 2 + (cellSize * 0.15f));
        
        outerRect.anchorMin = new Vector2(0.5f, 0.5f);
        outerRect.anchorMax = new Vector2(0.5f, 0.5f);
        outerRect.pivot = new Vector2(0.5f, 0.5f);
        outerRect.anchoredPosition = Vector2.zero;
        
        Image outerImage = outerPanel.AddComponent<Image>();
        
        if (boardSprite != null) {
            outerImage.sprite = boardSprite;
            outerImage.type = Image.Type.Sliced; 
            outerImage.raycastTarget = true; 
        } else {
            outerImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); 
        }
        
        boardPanel = new GameObject("BoardPanel");
        boardPanel.transform.SetParent(outerPanel.transform, false);
        
        RectTransform boardRect = boardPanel.AddComponent<RectTransform>();
        
        boardRect.sizeDelta = new Vector2(boardWidth, boardHeight);
        
        boardRect.anchorMin = new Vector2(0.5f, 0.5f);
        boardRect.anchorMax = new Vector2(0.5f, 0.5f);
        boardRect.pivot = new Vector2(0.5f, 0.5f);
        boardRect.anchoredPosition = Vector2.zero;
        
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
        
        // Pass the particle system prefab to the board
        if (particleSystemPrefab != null) {
            boardScript.SetParticleSystemPrefab(particleSystemPrefab);
        }

        boardScript.Initialize(cellSize);
    }
}