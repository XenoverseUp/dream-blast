using System;
using UnityEngine;

public class CellItem : MonoBehaviour {
    private CellItemType type;
    private int x;
    private int y;
    private int health = 1; 
    
    private SpriteRenderer spriteRenderer;
    private Sprite originalSprite;
    private Sprite damagedSprite;
    
    public int X { get => x; }
    public int Y { get => y; }
    
    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void Initialize(CellItemType type, int x, int y, Sprite sprite) {
        this.type = type;
        this.x = x;
        this.y = y;
        this.originalSprite = sprite;
        
        if (type == CellItemType.Vase) {
            health = 2;
        }
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
            if (type == CellItemType.Vase && damagedSprite != null) 
                spriteRenderer.sprite = damagedSprite;
            
            return false;
        }
    }
}