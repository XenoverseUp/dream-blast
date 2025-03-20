using UnityEngine;

public class AnimationManager : MonoBehaviour {
    public static AnimationManager Instance { get; private set; }

    [SerializeField] public float blockShakeAmount = 15f;
    [SerializeField] public float blockFallSpeed = 7.5f;

        
    private void Awake() {       
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public LTDescr PlayUpdateText(GameObject gameObject) {
        gameObject.transform.localScale = new(1.2f ,1.2f ,1.2f);
        return LeanTween.scale(gameObject, new(1,1,1), 0.15f).setEaseOutBounce();
    }

    public LTSeq PlayInvalidBlast(GameObject gameObject) {
        LeanTween.cancel(gameObject);
        gameObject.transform.rotation = Quaternion.identity;
        
        Quaternion originalRotation = gameObject.transform.rotation;
        
        return LeanTween.sequence()
            .append(LeanTween.rotateZ(gameObject, this.blockShakeAmount, 0.05f).setEase(LeanTweenType.easeShake))
            .append(LeanTween.rotateZ(gameObject, -this.blockShakeAmount, 0.1f).setEase(LeanTweenType.easeShake))
            .append(LeanTween.rotateZ(gameObject, this.blockShakeAmount * 0.6f, 0.08f).setEase(LeanTweenType.easeShake))
            .append(LeanTween.rotateZ(gameObject, -this.blockShakeAmount * 0.3f, 0.06f).setEase(LeanTweenType.easeShake))
            .append(LeanTween.rotateLocal(gameObject, originalRotation.eulerAngles, 0.05f).setEase(LeanTweenType.easeOutQuad)
            .setOnComplete(() => gameObject.transform.rotation = originalRotation));
    }

    public LTDescr PlayDestroyBlock(GameObject block) {
        Vector3 originalScale = block.transform.localScale;
        Vector3 originalPosition = block.transform.position;
        
        LeanTween.scale(block, new Vector3(
            originalScale.x * 1.2f,
            originalScale.y * 1.2f,
            originalScale.z
        ), 0.1f).setEase(LeanTweenType.easeOutQuad);
        
        LeanTween.rotateZ(block, Random.Range(-15f, 15f), 0.15f);
        
        var fadeTween = LeanTween.scale(block, new Vector3(
            originalScale.x * 0.2f,
            originalScale.y * 0.2f,
            originalScale.z
        ), 0.2f).setEase(LeanTweenType.easeInQuad).setDelay(0.1f);
        
        SpriteRenderer spriteRenderer = block.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            Color originalColor = spriteRenderer.color;
            LeanTween.value(block, (float val) => {
                Color newColor = spriteRenderer.color;
                newColor.a = val;
                spriteRenderer.color = newColor;
            }, originalColor.a, 0f, 0.3f).setDelay(0.1f);
        }
        
        LeanTween.moveY(block, originalPosition.y - 0.5f, 0.3f).setDelay(0.1f);
        
        return fadeTween;
    }
}
