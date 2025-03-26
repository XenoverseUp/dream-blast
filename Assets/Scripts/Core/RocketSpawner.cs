using UnityEngine;
using System.Collections;

public class RocketSpawner : MonoBehaviour {
    [SerializeField] private GameObject horizontalRocketPrefab;
    [SerializeField] private GameObject verticalRocketPrefab;
    [SerializeField] private GameObject explosionPrefab;
    
    [SerializeField] private Transform rocketParent;
    
    public static RocketSpawner Instance { get; private set; }
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
        
        if (rocketParent == null)
            rocketParent = transform;
    }
    
    public void SpawnRocket(Vector3 position, RocketEffect.RocketDirection direction) {
        if (direction == RocketEffect.RocketDirection.Horizontal) {
            SpawnHorizontalRocket(position);
        } else {
            SpawnVerticalRocket(position);
        }
        
        if (explosionPrefab != null) {
            Instantiate(explosionPrefab, position, Quaternion.identity);
        }
    }
    
    public void SpawnHorizontalRocket(Vector3 position) {
        if (horizontalRocketPrefab != null) {
            GameObject rocket = Instantiate(horizontalRocketPrefab, position, Quaternion.identity, rocketParent);
            
            RocketEffect rocketEffect = rocket.GetComponent<RocketEffect>();
            if (rocketEffect != null) {
                rocketEffect.SetDirection(RocketEffect.RocketDirection.Horizontal);
                rocketEffect.Activate();
            }
        }
    }
    
    public void SpawnVerticalRocket(Vector3 position) {
        if (verticalRocketPrefab != null) {
            GameObject rocket = Instantiate(verticalRocketPrefab, position, Quaternion.identity, rocketParent);
            
            RocketEffect rocketEffect = rocket.GetComponent<RocketEffect>();
            if (rocketEffect != null) {
                rocketEffect.SetDirection(RocketEffect.RocketDirection.Vertical);
                rocketEffect.Activate();
            }
        }
    }
    
    private void OnDestroy() {
        if (EventManager.Instance != null) {
            EventManager.Instance.OnRocketActivated -= SpawnRocket;
        }
    }
}