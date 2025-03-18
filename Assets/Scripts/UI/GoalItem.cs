using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public enum ObstacleType {
    BoxObstacle,
    VaseObstacle,
    StoneObstacle
}

public class GoalItem : MonoBehaviour, IActionListener {
    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    
    [Header("Configuration")]
    [SerializeField] public ObstacleType obstacleType = ObstacleType.BoxObstacle;
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

    public void OnAction(int moveCount, Dictionary<string, int> blocks) {
        countText?.SetText(blocks.GetValueOrDefault(GetObstacleTypeString(), 0).ToString());
    }

    public void Initialize(ObstacleType type, Sprite sprite) {
        obstacleType = type;
        iconSprite = sprite;
        UpdateIcon();
    }
    
    private void UpdateIcon() {
        if (iconImage != null && iconSprite != null) {
            iconImage.sprite = iconSprite;
        }
    }

    private string GetObstacleTypeString() {
        return obstacleType switch {
            ObstacleType.BoxObstacle => "bo",
            ObstacleType.VaseObstacle => "v",
            ObstacleType.StoneObstacle => "s",
            _ => "bo",
        };
    }
}