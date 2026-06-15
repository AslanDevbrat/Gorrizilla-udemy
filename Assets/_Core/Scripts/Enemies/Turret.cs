using System.Collections;
using System.Collections.Generic;
using GorillaZilla;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Turret Settings")]
    public float lookSpeed = 1;
    public Transform turretHead;
    public float fireRate;
    public float fireTimer;
    public float bulletSpeed = 5f;
    public GameObject bulletPrefab;
    public LayerMask laserLayer;
    public bool canAttack = true;

    [Header("Aiming Randomization")]
    [Tooltip("Lowest point to aim (e.g., -0.6 for Stomach)")]
    public float minHeightOffset = -0.6f;

    [Tooltip("Highest point to aim (e.g., -0.1 for Neck/Head)")]
    public float maxHeightOffset = -0.1f;

    [Tooltip("How fast the aim drifts up and down. Higher = faster wobble.")]
    public float aimWobbleSpeed = 1.0f;

    // Internal variable to keep the random seed unique for this specific turret
    private float randomSeed;

    private void Awake()
    {
        fireTimer = fireRate;
        // Create a random starting point so all turrets don't move in sync
        randomSeed = Random.Range(0f, 100f);
    }

    public string pqZone = "";

    void FixedUpdate()
    {
        LookAtPlayer();

        if (GameManager.Mode == "PQ") return;

        // Raycast using the current aim direction
        if (canAttack && Physics.Raycast(turretHead.position, turretHead.forward, out RaycastHit hitInfo, Mathf.Infinity, laserLayer, QueryTriggerInteraction.Collide))
        {
            if (hitInfo.collider.GetComponentInParent<Player>() || hitInfo.collider.CompareTag("Player") || hitInfo.collider.CompareTag("Head"))
            {
                if (fireTimer >= fireRate)
                {
                    FireBullet();
                    fireTimer = 0;
                }
                fireTimer += Time.fixedDeltaTime;
            }
        }
    }

    public void FirePQBullet(Vector3 targetPos, float travelTime)
    {
        Vector3 dir = (targetPos - turretHead.position).normalized;
        float dist = Vector3.Distance(turretHead.position, targetPos);
        float speed = dist / travelTime;

        Vector3 spawnPos = turretHead.position + dir * 0.1f;
        var bulletGO = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir), transform);
        var rb = bulletGO.GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearVelocity = dir * speed;
        Destroy(bulletGO, travelTime + 0.5f);
    }

    void LookAtPlayer()
    {
        Transform playerHead = Camera.main.transform;

        // 1. Get Head Position
        Vector3 targetPos = playerHead.position;

        // 2. Calculate Smooth Random Offset (Perlin Noise)
        // Time.time makes it move over time. randomSeed ensures unique movement.
        float noiseValue = Mathf.PerlinNoise(Time.time * aimWobbleSpeed, randomSeed);

        // Map the noise (0 to 1) to our Min and Max range
        float currentHeightOffset = Mathf.Lerp(minHeightOffset, maxHeightOffset, noiseValue);

        // 3. Apply the randomized offset
        targetPos.y += currentHeightOffset;

        // 4. Rotate towards the new Target Position
        Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position, Vector3.up);
        Quaternion curRotation = turretHead.rotation;

        turretHead.rotation = Quaternion.Slerp(curRotation, targetRotation, Time.fixedDeltaTime * lookSpeed);
    }

    void FireBullet()
    {
        Vector3 spawnPoint = turretHead.position + turretHead.forward * .1f;
        var bulletGO = Instantiate(bulletPrefab, spawnPoint, turretHead.rotation, transform);
        bulletGO.GetComponent<Rigidbody>().AddForce(turretHead.forward * bulletSpeed);
        Destroy(bulletGO, 10f);
    }
}