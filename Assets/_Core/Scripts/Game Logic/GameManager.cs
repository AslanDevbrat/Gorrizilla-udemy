
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

namespace GorillaZilla
{
    [RequireComponent(typeof(Level))]
    public class GameManager : MonoBehaviour
    {
        public static string Mode { get; set; } = "No PQ";

        public static GameManager Instance { get; private set; }

        public static Dictionary<string, Turret> PQTurrets { get; } = new Dictionary<string, Turret>();

        public static void TriggerPQFire()
        {
            if (PQTurrets.Count == 0 || Instance == null) return;

            Transform playerHead = Camera.main.transform;

            float maxDist = 0f;
            float refSpeed = 5f;
            foreach (var kv in PQTurrets)
            {
                if (kv.Value == null) continue;
                float dist = Vector3.Distance(kv.Value.turretHead.position, Instance.GetChestTarget(kv.Key, playerHead));
                if (dist > maxDist) { maxDist = dist; refSpeed = kv.Value.bulletSpeed; }
            }

            float travelTime = maxDist / refSpeed;

            foreach (var kv in PQTurrets)
            {
                if (kv.Value == null) continue;
                kv.Value.FirePQBullet(Instance.GetChestTarget(kv.Key, playerHead), travelTime);
            }
        }

        [Header("PQ Chest Targets")]
        [SerializeField] private float chestUpperY = -0.3f;
        [SerializeField] private float chestLowerY = -0.5f;
        [SerializeField] private float chestDepth = 0.15f;

        private Vector3 GetChestTarget(string zone, Transform playerHead)
        {
            Vector3 bodyCenter = playerHead.position;
            switch (zone)
            {
                case "upperFront":
                case "upperBack":  return bodyCenter + Vector3.up * chestUpperY;
                case "lowerFront":
                case "lowerBack":  return bodyCenter + Vector3.up * chestLowerY;
                default:           return bodyCenter + Vector3.up * ((chestUpperY + chestLowerY) * 0.5f);
            }
        }

        [Header("Dependencies")]
        [SerializeField] Player player;

        [Header("References")]
        [SerializeField] WaveDisplay waveDisplay;
        [SerializeField] AudioSource sfx_BackgroundMusic;
        [SerializeField] AudioSource sfx_WaveEnd;

        [Header("Wave Settings")]
        [SerializeField] float delayBetweenWaves = 1f;
        [SerializeField] Wave waveTemplate;
        private Wave curWave;
        private Level level;
        private int waveNum = 0;

        private bool isGameOver = false;

        private void Awake()
        {
            Instance = this;
            level = GetComponent<Level>();
            level.onWaveSpawned.AddListener(OnWaveSpawned);
            level.onLastEnemyDestroyed.AddListener(OnLastEnemyDestroyed);

            if (player == null) player = GameObject.FindObjectOfType<Player>();
            player.onPlayerHit.AddListener(OnPlayerHit);
        }

        private void OnWaveSpawned()
        {
            // throw new NotImplementedException();
        }
        private void OnLastEnemyDestroyed()
        {
            EndWave();
        }
        public void OnPlayerHit()
        {
            waveNum = 0;
            // GameOver();
        }
        private void Update()
        {
            if (Mode == "PQ" && OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
            {
                var haptics = PlayerHaptics.Instance;
                if (haptics != null && haptics.HasLastEffect)
                {
                    TriggerPQFire();
                    haptics.ArmLastEffect();
                }
            }
        }

        Wave MakeWave(int waveNum)
        {
            Wave wave = Wave.Copy(waveTemplate);
            int numBuildings = Mathf.Min(waveTemplate.numBuildings + waveNum, 20);
            int numEnemies = Mathf.Min(waveTemplate.numEnemies + waveNum, 20);
            wave.numBuildings = numBuildings;
            wave.numEnemies = numEnemies;
            return wave;
        }
        [ContextMenu("Start Game")]
        public void StartGame()
        {
            waveNum = 0;
            StartNextWave();
            player.Revive();

            player.menu.ToggleMenu(false);
        }
        void StartNextWave()
        {
            curWave = MakeWave(waveNum);
            StartCoroutine(WaveSequence(curWave));
        }
        void EndWave()
        {
            sfx_BackgroundMusic.Stop();
            if (!isGameOver)
            {
                waveNum++;
                StartNextWave();
                sfx_WaveEnd.Play();
            }
        }
        IEnumerator WaveSequence(Wave wave)
        {
            player.timeManipulator.enabled = false;


            waveDisplay.ShowMessage("WAVE " + (waveNum + 1));
            isGameOver = false;
            if (waveNum >= 1)
                yield return StartCoroutine(level.ClearLevelAnimated(delayBetweenWaves));




            yield return new WaitForSeconds(delayBetweenWaves);
            if (!PlayerSettings.MuteMusic)
                sfx_BackgroundMusic.Play();

            waveDisplay.Hide();
            yield return StartCoroutine(level.SpawnWave(wave));
            for (int i = 3; i > 0; i--)
            {
                waveDisplay.ShowMessage("" + i);
                yield return new WaitForSeconds(1);
            }
            waveDisplay.ShowMessage("DESTROY!");
            yield return new WaitForSeconds(1);
            waveDisplay.Hide();
            level.StartLevel();

            player.timeManipulator.enabled = true;

        }
        void GameOver()
        {
            level.ClearLevel();
            player.timeManipulator.enabled = false;
            isGameOver = true;
            waveDisplay.ShowMessage("GAME OVER");
            StartCoroutine(GameOverSequence());
        }

        IEnumerator GameOverSequence()
        {
            yield return new WaitForSeconds(delayBetweenWaves);
            waveDisplay.Hide();
            player.menu.ToggleMenu(true);
            player.menu.OpenStartPage();
            player.Revive();
        }
    }
}
