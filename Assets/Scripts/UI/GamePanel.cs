using System;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour {
    [SerializeField] private Sprite boardSprite;  

    [Serializable]
    private struct BlockSprites {
        public Sprite redCube;
        public Sprite greenCube;
        public Sprite blueCube;
        public Sprite yellowCube;
    };

    [Serializable]
    private struct BlockRocketStateSprites {
        public Sprite redCubeRocket;
        public Sprite greenCubeRocket;
        public Sprite blueCubeRocket;
        public Sprite yellowCubeRocket;
    };
    
    [Serializable]
    private struct ObstacleSprites {
        public Sprite box;
        public Sprite stone;
        public Sprite vase;
        public Sprite damagedVase;
    };
    
    [Serializable]
    private struct ArtifactSprites {
        public Sprite horizontalRocket;
        public Sprite verticalRocket;
    };

    [Serializable]
    private struct CrackSprites {
        public Sprite redCrack;
        public Sprite greenCrack;
        public Sprite blueCrack;
        public Sprite yellowCrack;
    };

 

    [Header("Sprites")]
    [SerializeField] private BlockSprites blockSprites;
    [SerializeField] private BlockRocketStateSprites rocketStateSprites;
    [SerializeField] private ObstacleSprites obstacleSprites;
    [SerializeField] private ArtifactSprites artifactSprites;
    [SerializeField] private CrackSprites crackSprites;

    [Header("Effects")]
    [SerializeField] private GameObject particleSystemPrefab;

    [Header("Layout")]
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

        
        BlockFactory blockFactory = boardPanel.AddComponent<BlockFactory>();
       
        blockFactory.SetBlockSprites(
            blockSprites.redCube,
            blockSprites.greenCube,
            blockSprites.blueCube,
            blockSprites.yellowCube
        );

        blockFactory.SetRocketStateSprites(
            rocketStateSprites.redCubeRocket,
            rocketStateSprites.greenCubeRocket,
            rocketStateSprites.blueCubeRocket,
            rocketStateSprites.yellowCubeRocket
        );

        blockFactory.SetRocketSprites(
            artifactSprites.horizontalRocket, 
            artifactSprites.verticalRocket
        );

        blockFactory.SetObstacleSprites(
            obstacleSprites.box,
            obstacleSprites.stone,
            obstacleSprites.vase,
            obstacleSprites.damagedVase
        );

        blockFactory.SetCrackSprites(
            crackSprites.redCrack, 
            crackSprites.greenCrack, 
            crackSprites.blueCrack,
            crackSprites.yellowCrack
        );
    
        blockFactory.SetParticleSystemPrefab(particleSystemPrefab);

        boardPanel.AddComponent<Board>().Initialize(cellSize);
    }
}