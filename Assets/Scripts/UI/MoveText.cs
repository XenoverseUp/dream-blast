using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoveText : MonoBehaviour, IActionListener {
    private TMP_Text textComponent;
    
    void Start() {
        textComponent = GetComponent<TMP_Text>();
        LevelManager.Instance.Subscribe(this);
    }
    
    void OnDestroy() {
        LevelManager.Instance.Unsubscribe(this);
    }

    public void OnAction(int moveCount, Dictionary<CellItemType, int> obstacles) {
        if (textComponent == null) return;
        if (textComponent.text == moveCount.ToString()) return;
        if (moveCount < 0) return;

        textComponent.SetText(moveCount.ToString());
        AnimationManager.Instance.PlayUpdateText(textComponent.gameObject);
    }
}