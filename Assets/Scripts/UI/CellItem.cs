using UnityEngine;

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

public class CellItem : MonoBehaviour {
    private CellItemType type;
    private int x;
    private int y;
    private int health; 
    
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private Sprite damagedSprite;
    private Board board;
    private Sprite crack;
    private GameObject particleSystemPrefab;

    
    public int X { get => x; }
    public int Y { get => y; }
    
    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Start() {
        board = GetComponentInParent<Board>();
        
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        
        collider.size = new Vector2(1f, 1.15f);
        collider.offset = new Vector2(0f, -0.15f);
    }
    
    public void Initialize(CellItemType type, int x, int y, Sprite sprite, GameObject particleSystemPrefab, Sprite crack) {
        this.type = type;
        this.x = x;
        this.y = y;
        this.originalSprite = sprite;
        this.particleSystemPrefab = particleSystemPrefab;
        this.crack = crack;
        this.health = type == CellItemType.Vase ? 2 : 1;
        
        
        if (spriteRenderer != null && sprite != null) {
            spriteRenderer.sprite = sprite;
        }
    }
    
    public void Initialize(CellItemType type, int x, int y, Sprite sprite, GameObject particleSystemPrefab, Sprite crack, Sprite damagedSprite) {
        Initialize(type, x, y, sprite, particleSystemPrefab, crack);
        this.damagedSprite = damagedSprite;
    }
    
    public void SetDamagedSprite(Sprite damagedSprite) {
        this.damagedSprite = damagedSprite;
    }
    
    public void SetPosition(int x, int y) {
        this.x = x;
        this.y = y;
    }
    
    public CellItemType GetItemType() {
        return type;
    }
    
    public bool IsCube() {
        return type == CellItemType.RedCube || 
               type == CellItemType.GreenCube || 
               type == CellItemType.BlueCube || 
               type == CellItemType.YellowCube;
    }
    
    public bool IsRocket() {
        return type == CellItemType.HorizontalRocket || 
               type == CellItemType.VerticalRocket;
    }
    
    public bool IsObstacle() {
        return type == CellItemType.Box || 
               type == CellItemType.Stone || 
               type == CellItemType.Vase;
    }
    
    public bool CanFall() {
        return IsCube() || IsRocket() || type == CellItemType.Vase;
    }
    
    public bool TakeDamage() {
        health -= 1;
        
        if (health <= 0) return true;
        else {
            if (type == CellItemType.Vase && damagedSprite != null && spriteRenderer != null) 
                spriteRenderer.sprite = damagedSprite;
            
            return false;
        }
    }

    public void InstantiateParticleSystem() {
        GameObject particleInstance = Instantiate(particleSystemPrefab, transform.position, Quaternion.identity);
        
        ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();

        if (particleSystem != null) {
            ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.material.mainTexture = crack.texture;
        
            particleSystem.Play();
            
            float totalDuration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
            Destroy(particleInstance, totalDuration);
        } else {
            Debug.LogWarning("Particle system component not found on prefab");
            Destroy(particleInstance, 2f);
        }
        
    }
    
    private void OnMouseDown() {
        if (board != null) board.TryBlast(x, y);
    }
}