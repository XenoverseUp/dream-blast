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
