using System;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour {

    [SerializeField] private Sprite boardSprite; 
    
    private int gridWidth, gridHeight;
    private RectTransform rect;
    private readonly float padding = 0.12f; // Padding percentage
    private GameObject boardPanel;

    void Start() {
        this.gridWidth = LevelManager.Instance.GetLevelData().GridWidth;
        this.gridHeight = LevelManager.Instance.GetLevelData().GridHeight;

        this.rect = GetComponent<RectTransform>();
        
        float gridSize = CalculateOptimalGridSize();
        CreateBoardPanel(gridSize);
    }

    private float CalculateOptimalGridSize() {
        float availableWidth = rect.rect.width * (1.0f - padding);
        float availableHeight = rect.rect.height * (1.0f - padding);
        
        float gridSizeFromWidth = availableWidth / gridWidth;
        float gridSizeFromHeight = availableHeight / gridHeight;
        
        return Mathf.Min(gridSizeFromWidth, gridSizeFromHeight);
    }

    private void CreateBoardPanel(float gridSize) {
        boardPanel = new GameObject("Board");
        boardPanel.transform.SetParent(this.transform, false);
        
        RectTransform boardRect = boardPanel.AddComponent<RectTransform>();
        
        float boardWidth = gridSize * gridWidth;
        float boardHeight = gridSize * gridHeight;
        
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
    }
}