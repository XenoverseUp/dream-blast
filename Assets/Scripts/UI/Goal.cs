using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Goal : MonoBehaviour {
    [Header("References")]
    [SerializeField] private GameObject goalItemPrefab;
    
    [Header("Obstacle Sprites")]
    [SerializeField] private Sprite boxObstacleSprite;   
    [SerializeField] private Sprite vaseObstacleSprite;  
    [SerializeField] private Sprite stoneObstacleSprite; 
    
    [Header("Layout Configuration")]
    [SerializeField] private Vector2 baseItemSize = new Vector2(100, 100);
    [SerializeField] private float singleItemScale = 1.0f;
    [SerializeField] private float doubleItemScale = 0.7f;
    [SerializeField] private float tripleItemScale = 0.5f;
    [SerializeField] private float spacing = 10f;
    
    private Dictionary<string, GoalItem> goalItems = new Dictionary<string, GoalItem>();
    private RectTransform containerRect;
    
    void Awake() {
        containerRect = GetComponent<RectTransform>();
        if (containerRect == null) {
            Debug.LogError("Goal script must be attached to a GameObject with RectTransform component!");
        }
    }
    
    void Start() {
        List<string> obstacleTypes = LevelManager.Instance.GetObstacleTypes();
        
        switch (obstacleTypes.Count) {
            case 1:
                CreateSingleItemLayout(obstacleTypes[0]);
                break;
            case 2:
                CreateDoubleItemLayout(obstacleTypes[0], obstacleTypes[1]);
                break;
            case 3:
                CreateTripleItemLayout(obstacleTypes[0], obstacleTypes[1], obstacleTypes[2]);
                break;
            default:
                Debug.LogWarning($"Unexpected number of block types: {obstacleTypes.Count}.");
                break;
        }
    }
    

    private void CreateSingleItemLayout(string blockType) {
        GoalItem goalItem = CreateGoalItem(blockType);
        GameObject itemObj = goalItem.gameObject;
        RectTransform rectTransform = itemObj.GetComponent<RectTransform>();
        
        itemObj.transform.localScale = new Vector3(singleItemScale, singleItemScale, 1f);
        
        rectTransform.anchoredPosition = Vector2.zero;
        
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
    }
    
    private void CreateDoubleItemLayout(string blockType1, string blockType2) {
        HorizontalLayoutGroup horizontalLayout = gameObject.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.spacing = spacing;
        horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
        horizontalLayout.childForceExpandWidth = false;
        horizontalLayout.childForceExpandHeight = false;
        
        GoalItem item1 = CreateGoalItem(blockType1);
        GoalItem item2 = CreateGoalItem(blockType2);
        
        item1.gameObject.transform.localScale = new Vector3(doubleItemScale, doubleItemScale, 1f);
        item2.gameObject.transform.localScale = new Vector3(doubleItemScale, doubleItemScale, 1f);
        
        RectTransform rect1 = item1.GetComponent<RectTransform>();
        RectTransform rect2 = item2.GetComponent<RectTransform>();
        
        rect1.anchorMin = new Vector2(0.5f, 0.5f);
        rect1.anchorMax = new Vector2(0.5f, 0.5f);
        rect1.pivot = new Vector2(0.5f, 0.5f);
        
        rect2.anchorMin = new Vector2(0.5f, 0.5f);
        rect2.anchorMax = new Vector2(0.5f, 0.5f);
        rect2.pivot = new Vector2(0.5f, 0.5f);
    }
    
    private void CreateTripleItemLayout(string blockType1, string blockType2, string blockType3) {
        GameObject topRowObj = new GameObject("TopRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        RectTransform topRowRect = topRowObj.GetComponent<RectTransform>();
        topRowRect.SetParent(transform, false);
        topRowRect.anchorMin = new Vector2(0, 1);
        topRowRect.anchorMax = new Vector2(1, 1);
        topRowRect.pivot = new Vector2(0.5f, 1);
        topRowRect.anchoredPosition = Vector2.zero;
        topRowRect.sizeDelta = new Vector2(0, baseItemSize.y * tripleItemScale);
        
        HorizontalLayoutGroup topRowLayout = topRowObj.GetComponent<HorizontalLayoutGroup>();
        topRowLayout.childAlignment = TextAnchor.MiddleCenter;
        topRowLayout.spacing = spacing;
        topRowLayout.childForceExpandWidth = false;
        topRowLayout.childForceExpandHeight = false;
        
        GameObject bottomRowObj = new GameObject("BottomRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        RectTransform bottomRowRect = bottomRowObj.GetComponent<RectTransform>();
        bottomRowRect.SetParent(transform, false);
        bottomRowRect.anchorMin = new Vector2(0, 0);
        bottomRowRect.anchorMax = new Vector2(1, 0);
        bottomRowRect.pivot = new Vector2(0.5f, 0);
        bottomRowRect.anchoredPosition = Vector2.zero;
        bottomRowRect.sizeDelta = new Vector2(0, baseItemSize.y * tripleItemScale);
        
        HorizontalLayoutGroup bottomRowLayout = bottomRowObj.GetComponent<HorizontalLayoutGroup>();
        bottomRowLayout.childAlignment = TextAnchor.MiddleCenter;
        bottomRowLayout.childForceExpandWidth = false;
        bottomRowLayout.childForceExpandHeight = false;
        
        VerticalLayoutGroup verticalLayout = gameObject.AddComponent<VerticalLayoutGroup>();
        verticalLayout.childAlignment = TextAnchor.MiddleCenter;
        verticalLayout.spacing = spacing;
        verticalLayout.childForceExpandWidth = true;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childControlWidth = true;
        verticalLayout.childControlHeight = true;
        
        GoalItem item1 = CreateGoalItem(blockType1, topRowObj.transform);
        GoalItem item2 = CreateGoalItem(blockType2, topRowObj.transform);
        
        GoalItem item3 = CreateGoalItem(blockType3, bottomRowObj.transform);
        
        item1.gameObject.transform.localScale = new Vector3(tripleItemScale, tripleItemScale, 1f);
        item2.gameObject.transform.localScale = new Vector3(tripleItemScale, tripleItemScale, 1f);
        item3.gameObject.transform.localScale = new Vector3(tripleItemScale, tripleItemScale, 1f);
        
        SetupRectTransform(item1.GetComponent<RectTransform>());
        SetupRectTransform(item2.GetComponent<RectTransform>());
        SetupRectTransform(item3.GetComponent<RectTransform>());
    }
    
    private void SetupRectTransform(RectTransform rect) {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
    }
    
    private GoalItem CreateGoalItem(string blockType, Transform parent = null) {
        if (parent == null) parent = transform;
        
        GameObject itemObj = Instantiate(goalItemPrefab, parent);
        GoalItem goalItem = itemObj.GetComponent<GoalItem>();
        
        ObstacleType obstacleType;
        Sprite sprite;
        
        switch (blockType) {
            case "bo":
                obstacleType = ObstacleType.BoxObstacle;
                sprite = boxObstacleSprite;
                break;
            case "v":
                obstacleType = ObstacleType.VaseObstacle;
                sprite = vaseObstacleSprite;
                break;
            case "s":
                obstacleType = ObstacleType.StoneObstacle;
                sprite = stoneObstacleSprite;
                break;
            default:
                obstacleType = ObstacleType.BoxObstacle;
                sprite = boxObstacleSprite;
                break;
        }
        
        goalItem.Initialize(obstacleType, sprite);
        goalItems[blockType] = goalItem;
        return goalItem;
    }
}