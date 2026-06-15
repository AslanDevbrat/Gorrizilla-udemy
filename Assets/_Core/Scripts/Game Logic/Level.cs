using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaZilla
{
    public class Level : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] RoomManager roomManager;

        [Header("Events")]
        public UnityEvent onWaveSpawned;
        public UnityEvent onLastEnemyDestroyed;

        private Transform buildingsRoot;
        private Transform enemiesRoot;
        private float buildingSpawnDelay = .025f;
        private List<Enemy> spawnedEnemies = new List<Enemy>();
        private List<DestructableBuilding> spawnedBuildings = new List<DestructableBuilding>();

        private void Awake()
        {
            // if (roomManager == null)
            // {
            //     roomManager = GameObject.FindObjectOfType<RoomManager>();
            // }
            buildingsRoot = new GameObject("Buildings").transform;
            buildingsRoot.parent = transform;
            enemiesRoot = new GameObject("Enemies").transform;
            enemiesRoot.parent = transform;
        }
        private void OnEnemyDestroyed(Enemy enemy)
        {
            spawnedEnemies.Remove(enemy);

            print("Enemy destroyed, remaining: " + spawnedEnemies.Count);
            if (spawnedEnemies.Count <= 0)
            {
                onLastEnemyDestroyed.Invoke();
            }
        }

        public IEnumerator SpawnWave(Wave wave)
        {
            spawnedEnemies.Clear();
            spawnedBuildings.Clear();
            GameManager.PQTurrets.Clear();
            // Zones match cornerAngles order: { -30, +30, +150, -150 }
            string[] cornerZones = { "lowerBack", "upperFront", "lowerFront", "upperBack" };
            int numBuildings = wave.numBuildings;
            int numEnemies = wave.numEnemies;
            List<Transform> enemySpawnPoints = new List<Transform>();
            List<Vector3> availableLocations = GetAvailableSpawnLocations();

            // Pick 4 corner locations relative to headset facing direction at spawn time
            Vector3 center = Vector3.zero;
            foreach (var loc in availableLocations) center += loc;
            center /= availableLocations.Count;

            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            camForward.Normalize();
            float playerAngle = Mathf.Atan2(camForward.z, camForward.x) * Mathf.Rad2Deg;
            float[] cornerAngles = { playerAngle - 30f, playerAngle + 30f, playerAngle + 150f, playerAngle - 150f };
            List<Vector3> cornerLocations = new List<Vector3>();
            foreach (float angle in cornerAngles)
            {
                if (availableLocations.Count == 0) break;
                Vector3 loc = FindLocationClosestToAngle(availableLocations, angle, center);
                cornerLocations.Add(loc);
                availableLocations.Remove(loc);
            }

            // Spawn 4 corner buildings first, tinted red for visibility
            List<Transform> cornerTurretSpawnPoints = new List<Transform>();
            foreach (Vector3 spawnPosition in cornerLocations)
            {
                float randomYRot = UnityEngine.Random.Range(0, 4) * 90f;
                Quaternion spawnRotation = Quaternion.Euler(0, randomYRot, 0);
                GameObject go = Instantiate(wave.GetRandomBuilding().prefab, spawnPosition, spawnRotation, buildingsRoot);

                foreach (var rend in go.GetComponentsInChildren<Renderer>())
                    foreach (var mat in rend.materials)
                        mat.color = Color.red;

                SpawnPointList spawnPointList = go.GetComponent<SpawnPointList>();
                if (spawnPointList != null && spawnPointList.spawnPoints.Count > 0)
                {
                    // Reserve first spawn point for the corner turret enemy
                    cornerTurretSpawnPoints.Add(spawnPointList.spawnPoints[0]);
                    // Remaining points go into the regular pool
                    for (int j = 1; j < spawnPointList.spawnPoints.Count; j++)
                        enemySpawnPoints.Add(spawnPointList.spawnPoints[j]);
                }

                DestructableBuilding db = go.GetComponent<DestructableBuilding>();
                if (db != null)
                {
                    db.isDestructable = false;
                    spawnedBuildings.Add(db);
                }
                yield return new WaitForSeconds(buildingSpawnDelay);
            }

            // Spawn one turret enemy per corner building, tinted red
            for (int ci = 0; ci < cornerTurretSpawnPoints.Count; ci++)
            {
                Transform spawnPoint = cornerTurretSpawnPoints[ci];
                if (wave.turretEnemy == null) continue;
                Spawnable turretSpawnable = wave.turretEnemy;
                GameObject go = Instantiate(turretSpawnable.prefab, spawnPoint.position, spawnPoint.rotation, enemiesRoot);

                foreach (var rend in go.GetComponentsInChildren<Renderer>())
                    foreach (var mat in rend.materials)
                        mat.color = Color.red;

                Enemy enemy = go.GetComponent<Enemy>();
                if (enemy != null)
                {
                    spawnedEnemies.Add(enemy);
                    enemy.onDestroy?.AddListener(OnEnemyDestroyed);
                    var turret = enemy.GetComponentInChildren<Turret>();
                    if (turret)
                    {
                        turret.canAttack = false;
                        if (ci < cornerZones.Length)
                        {
                            turret.pqZone = cornerZones[ci];
                            GameManager.PQTurrets[turret.pqZone] = turret;
                        }
                    }
                }
                yield return new WaitForSeconds(buildingSpawnDelay);
            }

            //Spawn remaining Buildings (corner buildings already counted against the total, +1 for power-up)
            int remainingBuildings = Mathf.Max(0, numBuildings - cornerLocations.Count) + 1;
            for (int i = 0; i < remainingBuildings; i++)
            {
                if (i > availableLocations.Count - 1)
                {
                    break;
                }
                //Get random position from grid
                int randomIndex = UnityEngine.Random.Range(0, availableLocations.Count);
                Vector3 spawnPosition = availableLocations[randomIndex];
                availableLocations.RemoveAt(randomIndex);

                //Get Random 90 degree rotation;
                float randomYRot = UnityEngine.Random.Range(0, 4) * 90f;
                Quaternion spawnRotation = Quaternion.Euler(0, randomYRot, 0);
                Spawnable spawnable;
                if (i == 0)
                {
                    //Spawn power-up building
                    spawnable = wave.powerUpBuilding;
                }
                else
                {
                    //Get weighted random building
                    spawnable = wave.GetRandomBuilding();
                }
                GameObject go = Instantiate(spawnable.prefab, spawnPosition, spawnRotation, buildingsRoot);

                //Add possible enemy spawn points
                SpawnPointList buildingSpawnPoints = go.GetComponent<SpawnPointList>();
                if (buildingSpawnPoints != null)
                {
                    enemySpawnPoints.AddRange(buildingSpawnPoints.spawnPoints);
                }

                DestructableBuilding destructableBuilding = go.GetComponent<DestructableBuilding>();
                if (destructableBuilding != null)
                {
                    //Make buildings indestructable during setup process
                    destructableBuilding.isDestructable = false;
                    spawnedBuildings.Add(destructableBuilding);
                }
                yield return new WaitForSeconds(buildingSpawnDelay);
            }
            // yield return new WaitForSeconds(1);

            //Spawn Enemies (corner turrets already counted against the total)
            int remainingEnemies = Mathf.Max(0, numEnemies - cornerTurretSpawnPoints.Count);
            for (int i = 0; i < remainingEnemies; i++)
            {
                //Get random spawn point
                if (enemySpawnPoints.Count == 0) break;

                int randomIndex = UnityEngine.Random.Range(0, enemySpawnPoints.Count);
                Transform spawnPoint = enemySpawnPoints[randomIndex];
                enemySpawnPoints.Remove(spawnPoint);

                //Get weighted random enemy
                Spawnable spawnable = wave.GetRandomEnemy();
                GameObject go = Instantiate(spawnable.prefab, spawnPoint.position, spawnPoint.rotation, enemiesRoot);

                //Set up callbacks
                Enemy enemy = go.GetComponent<Enemy>();
                if (enemy != null)
                {
                    spawnedEnemies.Add(enemy);
                    enemy.onDestroy?.AddListener(OnEnemyDestroyed);
                    var turret = enemy.GetComponentInChildren<Turret>();
                    if (turret)
                    {
                        turret.canAttack = false;
                    }
                }

            }
            onWaveSpawned?.Invoke();
        }
        public void StartLevel()
        {
            foreach (var enemy in spawnedEnemies)
            {
                if (enemy.TryGetComponent<Turret>(out Turret turret))
                    turret.canAttack = true;
            }
            foreach (var building in spawnedBuildings)
            {
                building.isDestructable = true;
            }
        }
        public void ClearLevel()
        {
            spawnedEnemies.Clear();
            buildingsRoot.gameObject.DestroyChildren();
            enemiesRoot.gameObject.DestroyChildren();
        }
        public IEnumerator ClearLevelAnimated(float duration)
        {
            spawnedEnemies.Clear();
            enemiesRoot.gameObject.DestroyChildren();
            var shrinkAnimators = GetComponentsInChildren<GrowShrinkAnimation>();
            foreach (var anim in shrinkAnimators)
            {
                anim.ShrinkAndDestroy(duration);
            }
            yield return new WaitForSeconds(duration);
            buildingsRoot.gameObject.DestroyChildren();
        }

        private Vector3 FindLocationClosestToAngle(List<Vector3> locations, float targetAngle, Vector3 center)
        {
            Vector3 targetDir = new Vector3(
                Mathf.Cos(targetAngle * Mathf.Deg2Rad),
                0f,
                Mathf.Sin(targetAngle * Mathf.Deg2Rad)
            );

            // Normalize distances so angle and distance are on the same scale
            float maxDist = 0f;
            foreach (var loc in locations)
                maxDist = Mathf.Max(maxDist, (loc - center).magnitude);

            const float angleWeight = 0.8f;
            Vector3 best = locations[0];
            float bestScore = float.MinValue;
            foreach (var loc in locations)
            {
                Vector3 dir = loc - center;
                float cosAngle = dir.magnitude > 0.001f ? Vector3.Dot(dir.normalized, targetDir) : 0f;
                float normalizedDist = maxDist > 0.001f ? dir.magnitude / maxDist : 0f;
                float score = angleWeight * cosAngle + (1f - angleWeight) * normalizedDist;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = loc;
                }
            }
            return best;
        }

        private List<Vector3> GetAvailableSpawnLocations()
        {
            List<Vector3> availableLocations = new List<Vector3>();

            // Find all game objects with the tag "PossibleSpawnLocation"
            GameObject[] spawnLocations = GameObject.FindGameObjectsWithTag("PossibleSpawnLocation");

            // Loop through each found game object and add its position to the list
            foreach (GameObject location in spawnLocations)
            {
                availableLocations.Add(location.transform.position);
            }

            return availableLocations;
        }
    }
}
