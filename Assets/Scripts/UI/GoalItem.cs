using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;


public class GoalItem : MonoBehaviour, IActionListener {
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    
    [Header("Configuration")]
    [SerializeField] public CellItemType obstacleType = CellItemType.Box;
    [SerializeField] public Sprite iconSprite;
    
    void Awake() {
        if (countText == null) countText = GetComponentInChildren<TMP_Text>();
        if (iconImage == null) iconImage = GetComponentInChildren<Image>();

        Debug.Assert(countText != null && iconImage != null, "Couldn't find component references in GoalItem.");

        UpdateIcon();
    }

    void OnValidate() {
        UpdateIcon();
    }

    void Start() {
        LevelManager.Instance.Subscribe(this);
    }
    
    void OnDestroy() { 
        LevelManager.Instance.Unsubscribe(this); 
    }

    public void Initialize(CellItemType type, Sprite sprite) {
        obstacleType = type;
        iconSprite = sprite;
        UpdateIcon();
    }
    
    private void UpdateIcon() {
        if (iconImage != null && iconSprite != null) {
            iconImage.sprite = iconSprite;
        }
    }


    public void OnAction(int moveCount, Dictionary<CellItemType, int> obstacles) {
        countText?.SetText(obstacles.GetValueOrDefault(obstacleType, 0).ToString());
    }
}