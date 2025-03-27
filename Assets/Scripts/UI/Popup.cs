using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class Popup : MonoBehaviour {
    [Header("References")]
    [SerializeField] private GameObject gamePanel;

    private RectTransform popupBackground;
    private TMP_Text titleText;
    private Button closeButton;
    private Button tryAgainButton;
    
    private Vector2 popupTargetPosition;
    private Vector2 popupStartPosition;
    private Image overlayImage;
    
    private void Awake() {
        overlayImage = GetComponent<Image>();
        
        popupBackground = transform.Find("Background").GetComponent<RectTransform>();
        closeButton = transform.Find("Background/Close").GetComponent<Button>();
        tryAgainButton = transform.Find("Background/PlayAgainButton").GetComponent<Button>();
        titleText = transform.Find("Background/FailedText").GetComponent<TMP_Text>();
        
        popupTargetPosition = popupBackground.anchoredPosition;
        popupStartPosition = popupTargetPosition + new Vector2(0, Screen.height);

        closeButton.onClick.AddListener(Close);
        tryAgainButton.onClick.AddListener(TryAgain);

        DontDestroyOnLoad(gameObject);
    }
    
    private void OnEnable() {
        if (overlayImage != null) {
            Color initialColor = overlayImage.color;
            initialColor.a = 0;
            overlayImage.color = initialColor;
        }
        
        if (popupBackground != null) {
            popupBackground.anchoredPosition = popupStartPosition;
        }

        if (titleText != null) {
            titleText.text = "Level # Failed!".Replace("#", GameManager.Instance.CurrentLevel.ToString());
        }
        
        StartCoroutine(AnimatePopup());
    }
    
    private IEnumerator AnimatePopup() {
        if (closeButton != null) closeButton.interactable = false;
        if (tryAgainButton != null) tryAgainButton.interactable = false;
        
        AnimationManager.Instance.AnimatePopupIn(
            overlayImage, 
            popupBackground, 
            popupTargetPosition, 
            () => {
                if (closeButton != null) closeButton.interactable = true;
                if (tryAgainButton != null) tryAgainButton.interactable = true;
            }
        );
        
        yield return null;
    }

    private IEnumerator AnimateDismiss() {
        if (closeButton != null) closeButton.interactable = false;
        if (tryAgainButton != null) tryAgainButton.interactable = false;
        
        AnimationManager.Instance.AnimatePopupOut(
            overlayImage, 
            popupBackground, 
            popupStartPosition, 
            () => {
                gameObject.SetActive(false);
            }
        );
        
        yield return null;
    }
    
    public void Close() {
        LevelTransition.Instance.SetActiveScene(ActiveScene.MainMenu);
    }

    public void TryAgain() {
        LevelManager.Instance.Reset();
        gamePanel.GetComponent<GamePanel>()?.Reset();
        StartCoroutine(AnimateDismiss());
    }
}