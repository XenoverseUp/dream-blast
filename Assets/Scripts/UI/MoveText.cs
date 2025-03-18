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


    public void OnAction(int moveCount, Dictionary<string, int> item) {
        if (textComponent != null) {
            textComponent.SetText(moveCount.ToString());
        }
    }
}