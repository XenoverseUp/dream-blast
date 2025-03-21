using System.Collections.Generic;

public interface IActionListener {
    public void OnAction(int moveCount, Dictionary<CellItemType, int> item);
}
