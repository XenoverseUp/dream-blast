using UnityEngine;
using System.Collections.Generic;

public class BlockFactory : MonoBehaviour {
    private Dictionary<CellItemType, Sprite> cubeSprites = new Dictionary<CellItemType, Sprite>();
    private Dictionary<CellItemType, Sprite> rocketStateSprites = new Dictionary<CellItemType, Sprite>();
    private Dictionary<CellItemType, Sprite> crackSprites = new Dictionary<CellItemType, Sprite>();
    private Dictionary<CellItemType, Sprite> obstacleSprites = new Dictionary<CellItemType, Sprite>();
    private Dictionary<CellItemType, Sprite> rocketSprites = new Dictionary<CellItemType, Sprite>();
    
    private Sprite damagedVaseSprite;
    private GameObject particleSystemPrefab;
    private Board board;
    private float cellSize;

    public void Initialize(Board board, float cellSize) {
        this.board = board;
        this.cellSize = cellSize;
    }

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

    public void SetRocketSprites(Sprite horizontalRocket, Sprite verticalRocket) {
        rocketSprites[CellItemType.HorizontalRocket] = horizontalRocket;
        rocketSprites[CellItemType.VerticalRocket] = verticalRocket;
    }
    
    public void SetParticleSystemPrefab(GameObject prefab) {
        this.particleSystemPrefab = prefab;
    }

    public CellItem CreateRandomCube(int x, int y, bool isNewCube = false) {
        CellItemType[] cubeTypes = new CellItemType[] {
            CellItemType.RedCube,
            CellItemType.GreenCube,
            CellItemType.BlueCube,
            CellItemType.YellowCube
        };
        
        CellItemType randomType = cubeTypes[Random.Range(0, cubeTypes.Length)];
        return CreateItem(x, y, randomType, isNewCube);
    }

    public CellItem CreateItemFromTypeString(int x, int y, string typeString) {
        CellItemType type = ItemTypeParserManager.Instance.ParseType(typeString);
        
        if (type == CellItemType.Empty) 
            return null; // Empty cell
        
        return CreateItem(x, y, type);
    }

    public CellItem CreateItem(int x, int y, CellItemType type, bool isNewCube = false) {
        if (type == CellItemType.Empty) return null;

        Sprite sprite = GetSpriteForType(type);
        Sprite crackSprite = null;
        Sprite rocketStateSprite = null;
        
        if (ItemTypeParserManager.Instance.IsCube(type)) {
            crackSprite = crackSprites[type];
            rocketStateSprite = rocketStateSprites[type];
        }
        
        Vector3 position = isNewCube ? board.GetWorldPosition(x, board.GridHeight) : board.GetLocalPosition(x, y);
        
        GameObject item = new GameObject($"Block_{type}");
        
        RectTransform itemRect = item.AddComponent<RectTransform>();
        if (!isNewCube) {
            itemRect.anchoredPosition = new Vector2(position.x, position.y);
        }
        itemRect.sizeDelta = new Vector2(cellSize, cellSize);
        
        item.transform.SetParent(board.transform, false);
        if (isNewCube) {
            item.transform.position = position;
        }
        
        SpriteRenderer spriteRenderer = item.AddComponent<SpriteRenderer>();
        spriteRenderer.sortingOrder = y + 1;
        
        ScaleSpriteToFit(
            item, 
            sprite, 
            cellSize,
            type == CellItemType.VerticalRocket
        );
        
        CellItem itemComponent = item.AddComponent<CellItem>();
        itemComponent.SetSprites(sprite, crackSprite);
        itemComponent.SetParticleSystemPrefab(particleSystemPrefab);
        
        if (type == CellItemType.Vase) {
            itemComponent.SetDamagedSprite(damagedVaseSprite);
        }
        
        itemComponent.Initialize(type, x, y);

        if (ItemTypeParserManager.Instance.IsCube(type)) {
            itemComponent.SetRocketStateSprite(rocketStateSprite);
        }
              
        return itemComponent;
    }

    private Sprite GetSpriteForType(CellItemType type) {
        return type switch {
            CellItemType.RedCube or CellItemType.GreenCube or CellItemType.BlueCube or CellItemType.YellowCube => cubeSprites[type],
            CellItemType.HorizontalRocket => rocketSprites[CellItemType.HorizontalRocket],
            CellItemType.VerticalRocket => rocketSprites[CellItemType.VerticalRocket],
            CellItemType.Box or CellItemType.Stone or CellItemType.Vase => obstacleSprites[type],
            _ => null,
        };
    }

    public Sprite GetSpriteForRocketType(CellItemType rocketType) {
        if (rocketType == CellItemType.HorizontalRocket || rocketType == CellItemType.VerticalRocket) {
            return rocketSprites[rocketType];
        }
        return null;
    }

    private void ScaleSpriteToFit(GameObject item, Sprite sprite, float targetSize, bool shouldScaleForHeight = false) {
        if (sprite == null) return;
        
        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;
        float scale = targetSize / (shouldScaleForHeight ? spriteHeight : spriteWidth);

        item.transform.localScale = new Vector3(scale, scale, 1f);
    }
}