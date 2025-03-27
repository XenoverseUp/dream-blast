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
    private Sprite rocketStateSprite;
    private Sprite damagedSprite;
    private Sprite crackSprite;
    private Board board;
    private GameObject particleSystemPrefab;
    
    public int X => x;
    public int Y => y;
    
    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();        
        collider.size = new Vector2(1f, 1.15f);
        collider.offset = new Vector2(0f, -0.15f);
        collider.isTrigger = true;
    }
    
    private void Start() {
        board = GetComponentInParent<Board>();
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.tag != "Rocket") return;
        
        if (IsObstacle()) {
            DamageObstacle();
            return;
        }
         
        if (IsCube()) {
            DestroyCube();
            return;
        }
        
        if (IsRocket()) {
            Vector3 position = transform.position;
            RocketEffect.RocketDirection direction = type == CellItemType.HorizontalRocket ? 
                RocketEffect.RocketDirection.Horizontal : 
                RocketEffect.RocketDirection.Vertical;
                
            board?.RemoveItem(x, y);
            EventManager.Instance?.TriggerRocketActivated(position, direction);
        }
    }

    private void OnMouseDown() {
        if (!BoardManager.Instance.IsInteractable()) return;
        
        bool isMoveSpent = board?.TryBlast(x, y) ?? false; 

        if (isMoveSpent) LevelManager.Instance.SpendMove();
    }
    
    public void Initialize(CellItemType type, int x, int y) {
        this.type = type;
        this.x = x;
        this.y = y;

        this.health = type == CellItemType.Vase ? 2 : 1;
        
        if (spriteRenderer != null && originalSprite != null) {
            spriteRenderer.sprite = originalSprite;
        }
    }
    
    public void SetSprites(Sprite sprite, Sprite crackSprite) {
        this.originalSprite = sprite;
        this.crackSprite = crackSprite;
        
        if (spriteRenderer != null && sprite != null) {
            spriteRenderer.sprite = sprite;
        }
    }

    public void SetRocketStateSprite(Sprite rocketStateSprite) {
        this.rocketStateSprite = rocketStateSprite;
    }
    
    public void SetDamagedSprite(Sprite damagedSprite) {
        this.damagedSprite = damagedSprite;
    }

    public void SetParticleSystemPrefab(GameObject prefab) {
        this.particleSystemPrefab = prefab;
    }
    
    public void SetPosition(int x, int y) {
        this.x = x;
        this.y = y;
    }
    
    public CellItemType GetItemType() {
        return type;
    }
    
    public bool CanFall() {
        return IsCube() || IsRocket() || type == CellItemType.Vase;
    }

    public bool DestroyCube() {
        if (IsObstacle()) return false;

        AnimationManager.Instance.PlayDestroyBlock(gameObject)
            .setOnComplete(() => board?.RemoveItem(x, y));
            
        InstantiateParticleSystem();
        return true;
    }
    
    public bool DamageObstacle() {
        if (!IsObstacle()) return false;

        health -= 1;
        
        if (health <= 0) {
            AnimationManager.Instance.PlayDestroyBlock(gameObject)
                .setOnComplete(() => board?.RemoveItem(x, y));
            
            InstantiateParticleSystem();
            return true;
        }

        if (type == CellItemType.Vase && damagedSprite != null && spriteRenderer != null) {
            RenderDamagedVaseSprite();
        }
        
        return false;
    }
    
    public void RenderOriginalSprite() {
        if (IsObstacle() || spriteRenderer == null || originalSprite == null || 
            spriteRenderer.sprite == originalSprite) return;
            
        spriteRenderer.sprite = originalSprite;
    }

    public void RenderRocketStateSprite() {
        if (IsObstacle() || spriteRenderer == null || rocketStateSprite == null || 
            spriteRenderer.sprite == rocketStateSprite) return;
            
        spriteRenderer.sprite = rocketStateSprite;
        AnimationManager.Instance.PlaySwitchToRocketState(gameObject);
    }

    private void RenderDamagedVaseSprite() {
        if (spriteRenderer != null && damagedSprite != null) {
            spriteRenderer.sprite = damagedSprite;
        }
    }

    public void InstantiateParticleSystem() {
        if (particleSystemPrefab == null || crackSprite == null) return;
        
        GameObject particleInstance = Instantiate(particleSystemPrefab, transform.position, Quaternion.identity);
        
        ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();

        if (particleSystem != null) {
            ParticleSystemRenderer renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            if (renderer != null && renderer.material != null) {
                renderer.material.mainTexture = crackSprite.texture;
            }
        
            particleSystem.Play();
            
            float totalDuration = particleSystem.main.duration + particleSystem.main.startLifetime.constantMax;
            Destroy(particleInstance, totalDuration);
        } else {
            Debug.LogWarning("Particle system component not found on prefab");
            Destroy(particleInstance, 2f);
        }
    }

    /* Type Checking Helpers */
    public bool IsCube() {
        return ItemTypeParserManager.Instance.IsCube(this.type);
    }
    
    public bool IsRocket() {
        return ItemTypeParserManager.Instance.IsRocket(this.type);
    }
    
    public bool IsObstacle() {
        return ItemTypeParserManager.Instance.IsObstacle(this.type);
    }
}