using UnityEngine;

public class AnimationManager : MonoBehaviour {
    public static AnimationManager Instance { get; private set; }

    [SerializeField] public float blockShakeAmount = 15f;
    [SerializeField] public float blockFallSpeed = 7.5f;

    public delegate void OnCompleteCallback();

    private void Awake() {       
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public LTDescr PlayRocketCreation(GameObject rocketObject, OnCompleteCallback callback = null) {
        if (rocketObject == null) return null;
        
        Vector3 originalScale = rocketObject.transform.localScale;
        
        rocketObject.transform.localScale = Vector3.zero;
        rocketObject.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-20f, 20f));
        
        LTDescr scaleTween = LeanTween.scale(rocketObject, originalScale, 0.25f)
            .setEase(LeanTweenType.easeOutBack)
            .setOvershoot(1.2f)
            .setOnComplete(() => {
                callback?.Invoke();
            });
        
        LeanTween.rotateZ(rocketObject, 0, 0.2f)
            .setEase(LeanTweenType.easeOutQuad);
        
        return scaleTween;
    }

    public LTDescr PlaySwitchToRocketState(GameObject gameObject, OnCompleteCallback callback = null) {
        LeanTween.cancel(gameObject);

        Vector3 originalScale = gameObject.transform.localScale;
        gameObject.transform.localScale = 1.15f * originalScale;

        return LeanTween.scale(gameObject, originalScale, 0.1f)
            .setOnComplete(() => {
                callback?.Invoke();
            });
    }

    public LTDescr PlayUpdateText(GameObject gameObject, OnCompleteCallback callback = null) {
        LeanTween.cancel(gameObject);

        Vector3 originalScale = gameObject.transform.localScale;
        gameObject.transform.localScale = 1.2f * originalScale;

        return LeanTween.scale(gameObject, originalScale, 0.15f)
            .setEaseOutBounce()
            .setOnComplete(() => {
                callback?.Invoke();
            });
    }

    public LTSeq PlayInvalidBlast(GameObject gameObject, OnCompleteCallback callback = null) {
        LeanTween.cancel(gameObject);
        gameObject.transform.rotation = Quaternion.identity;
        
        LTSeq sequence = LeanTween.sequence()
            .append(LeanTween.rotateZ(gameObject, this.blockShakeAmount, 0.05f).setEase(LeanTweenType.easeShake))
            .append(LeanTween.rotateZ(gameObject, -this.blockShakeAmount, 0.1f).setEase(LeanTweenType.easeShake))
            .append(LeanTween.rotateZ(gameObject, this.blockShakeAmount * 0.6f, 0.08f).setEase(LeanTweenType.easeShake))
            .append(LeanTween.rotateZ(gameObject, -this.blockShakeAmount * 0.3f, 0.06f).setEase(LeanTweenType.easeShake))
            .append(LeanTween.rotateLocal(gameObject, Quaternion.identity.eulerAngles, 0.05f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnComplete(() => {
                    gameObject.transform.rotation = Quaternion.identity;
                    callback?.Invoke();
                }));
                
        return sequence;
    }

    public LTDescr PlayDestroyBlock(GameObject block, OnCompleteCallback callback = null) {
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
        ), 0.2f)
        .setEase(LeanTweenType.easeInQuad)
        .setDelay(0.1f)
        .setOnComplete(() => {
            callback?.Invoke();
        });
        
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

    public float AnimateCubeFall(GameObject item, Vector3 startPos, Vector3 targetPos, OnCompleteCallback callback = null) {
        if (item == null) return 0f;

        CellItem itemComponent = item.GetComponent<CellItem>();
        if (itemComponent == null) return 0f;
        
        SpriteRenderer renderer = item.GetComponent<SpriteRenderer>();
        if (renderer != null) 
            renderer.sortingOrder = itemComponent.Y + 1;
        
        float speed = blockFallSpeed;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = distance / speed;
        
        LeanTween.value(item, 0f, 1f, duration)
            .setOnUpdate((float t) => {
                item.transform.position = LerpWithoutClamp(startPos, targetPos, EaseOutBack(t));
            })
            .setOnComplete(() => {
                item.transform.position = targetPos;
                callback?.Invoke();
            });

        return duration;        
    }

    private Vector3 LerpWithoutClamp(Vector3 a, Vector3 b, float t) { 
        return a + (b - a) * t; 
    }
    
    /* Easing Functions */
    public float EaseOutBack(float t) {
        float s = 1.70158f;
        return (t - 1) * (t - 1) * ((s + 1) * (t - 1) + s) + 1;
    }
    
    public float EaseInBack(float t) {
        float s = 1.70158f;
        return t * t * ((s + 1) * t - s);
    }

    public float EaseOutElastic(float t) {
        if (t == 0 || t == 1) return t;
        
        return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - 0.1f) * 5 * Mathf.PI) + 1;
    }
}