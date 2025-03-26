using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketEffect : MonoBehaviour {
    public enum RocketDirection {
        Horizontal,
        Vertical
    }
    
    [Header("Rocket Configuration")]
    [SerializeField] private RocketDirection rocketDirection = RocketDirection.Horizontal;
    
    [Header("Rocket Parts")]
    [SerializeField] private GameObject rocketPartA; 
    [SerializeField] private GameObject rocketPartB;
    
    [Header("Movement Settings")]
    [SerializeField] private float rocketPartSpeed = 10f;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float destroyDelay = 2f;
    
    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionInterval = 0.1f;
    
    private Vector3 partAInitialPosition;
    private Vector3 partBInitialPosition;
    private bool rocketActivated = false;
    private List<Collider2D> hitColliders = new List<Collider2D>();
    
    private void Awake() {
        SaveInitialPositions();
    }
    
    private void SaveInitialPositions() {
        if (rocketPartA != null) 
            partAInitialPosition = rocketPartA.transform.localPosition;
        
        if (rocketPartB != null)
            partBInitialPosition = rocketPartB.transform.localPosition;
    }
    
    private void Start() {
        rocketActivated = true;
        Destroy(gameObject, destroyDelay);

        
        if (explosionPrefab != null) {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            
            StartCoroutine(SpawnExplosions(rocketPartA, Vector3.left));
            StartCoroutine(SpawnExplosions(rocketPartB, Vector3.right));
        }
    }
    
    private IEnumerator SpawnExplosions(GameObject rocketPart, Vector3 direction) {
        if (rocketPart == null) yield break;
        
        float elapsedTime = 0f;
        Vector3 previousPos = rocketPart.transform.position;
        
        while (rocketPart != null && elapsedTime < destroyDelay) {
            yield return new WaitForSeconds(explosionInterval);
            
            if (rocketPart == null) break;
            
            if (Vector3.Distance(previousPos, rocketPart.transform.position) > 0.5f) {
                if (explosionPrefab != null) {
                    Instantiate(explosionPrefab, rocketPart.transform.position, Quaternion.identity);
                }
                previousPos = rocketPart.transform.position;
            }
            
            elapsedTime += explosionInterval;
        }
    }
    
    private void Update() {
        if (rocketActivated) MoveRocketParts();
    }
    
    private void MoveRocketParts() {
        if (rocketDirection == RocketDirection.Horizontal) {
            Move(Vector3.left, rocketPartA, partAInitialPosition);
            Move(Vector3.right, rocketPartB, partBInitialPosition);
        } else {
            Move(Vector3.up, rocketPartA, partAInitialPosition);
            Move(Vector3.down, rocketPartB, partBInitialPosition);
        }
    }

    private void Move(Vector3 direction, GameObject rocketPart, Vector3 initialPos) {
        if (rocketPart == null) return;

        Vector3 movement = direction * rocketPartSpeed * Time.deltaTime;
        rocketPart.transform.Translate(movement, Space.World);
        
        float distance = Vector3.Distance(rocketPart.transform.position, transform.position);
        if (distance >= maxDistance) Destroy(rocketPart);
    }
    
    public void SetDirection(RocketDirection direction) {
        rocketDirection = direction;
    }
    
    public void AddHitCollider(Collider2D collider) {
        if (!hitColliders.Contains(collider)) {
            hitColliders.Add(collider);
        }
    }
}

