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
    
    private bool isActivated = false;

    public void Activate() {
        if (isActivated) return;

        isActivated = true;
        Destroy(gameObject, destroyDelay);

        if (explosionPrefab != null) {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            
            StartCoroutine(SpawnExplosions(rocketPartA, GetDirectionVector(true)));
            StartCoroutine(SpawnExplosions(rocketPartB, GetDirectionVector(false)));
        }
    }
    
    private Vector3 GetDirectionVector(bool firstPart) {
        return rocketDirection switch {
            RocketDirection.Horizontal => firstPart ? Vector3.left : Vector3.right,
            RocketDirection.Vertical => firstPart ? Vector3.up : Vector3.down,
            _ => Vector3.zero,
        };
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
        if (isActivated) MoveRocketParts();
        
    }
    
    private void MoveRocketParts() {
        Move(GetDirectionVector(true), rocketPartA);
        Move(GetDirectionVector(false), rocketPartB);
    }

    private void Move(Vector3 direction, GameObject rocketPart) {
        if (rocketPart == null) return;

        Vector3 movement = direction * rocketPartSpeed * Time.deltaTime;
        rocketPart.transform.Translate(movement, Space.World);
        
        float distance = Vector3.Distance(rocketPart.transform.position, transform.position);
        if (distance >= maxDistance) {
            Destroy(rocketPart);
        }
    }
    
    public void SetDirection(RocketDirection direction) {
        rocketDirection = direction;
    }
}