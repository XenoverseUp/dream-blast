using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


public class StartButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler {
    [Header("Button Content")]
    [SerializeField] private string finishText = "Play";
    [SerializeField] private string levelText = "Level #";

    [Header("Button States")]
    [SerializeField] private float pressedBrightness = 0.8f;
    [SerializeField] private float pressedScale = 0.95f;

    [Header("Animation Settings")]
    [SerializeField] private float pressDuration = 0.03f; 
    [SerializeField] private float releaseDuration = 0.1f;

    private Button button;
    private TMP_Text textComponent;
    private Vector3 originalScale;
    private int currentTweenId = -1;
    private bool isPressed = false;
    private Image buttonImage;

    void Start() {
        button = GetComponent<Button>();
        textComponent = GetComponentInChildren<TMP_Text>();
        buttonImage = GetComponent<Image>();

        originalScale = transform.localScale;
        
        button.onClick.AddListener(OnButtonClick);
        button.transition = Selectable.Transition.None;

        if (textComponent == null)
            Debug.LogError($"No TMP_Text found in children of {gameObject.name}.");
        
        if (GameManager.Instance.IsGameCompleted) SetText(finishText);
        else SetText(levelText.Replace("#", GameManager.Instance.CurrentLevel.ToString()));
    }

    public void OnPointerDown(PointerEventData eventData) {
        isPressed = true;
        AnimateToPressed();
    }

    public void OnPointerUp(PointerEventData eventData) {
        if (isPressed) {
            isPressed = false;
            AnimateToNormal();
        }
    }

    public void OnPointerExit(PointerEventData eventData) { 
        if (isPressed) AnimateToNormal();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (isPressed) AnimateToPressed();
    }

    private void AnimateToPressed() {
        if (currentTweenId != -1) {
            LeanTween.cancel(currentTweenId);
            currentTweenId = -1;
        }
        
        currentTweenId = LeanTween.scale(gameObject, originalScale * pressedScale, pressDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .id;

            
        LeanTween.color(buttonImage.rectTransform, new Color(pressedBrightness, pressedBrightness, pressedBrightness), pressDuration);
    }


    private void AnimateToNormal() {
        if (currentTweenId != -1) {
            LeanTween.cancel(currentTweenId);
            currentTweenId = -1;
        }
        
        currentTweenId = LeanTween.scale(gameObject, originalScale, releaseDuration)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => { currentTweenId = -1; })
            .id;

        LeanTween.color(buttonImage.rectTransform, new Color(1.0f, 1.0f, 1.0f), pressDuration);
    }

    private void OnButtonClick() {
        if (!GameManager.Instance.IsGameCompleted)
            LevelTransition.Instance.SetActiveScene(ActiveScene.Level);
    }

    private void SetText(string text) { textComponent.SetText(text); }
}