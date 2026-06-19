using System.IO;
using UnityEngine;
using Skinetic;
using Newtonsoft.Json.Linq;
using System.Reflection;


namespace GorillaZilla
{
    public class PlayerHaptics : MonoBehaviour
    {
        public static PlayerHaptics Instance { get; private set; }

        [Header("Skinetic Settings")]
        [SerializeField] private SkineticDevice skineticDevice;
        [SerializeField] private HapticEffect hitEffect;

        [Header("Calibration")]
        [SerializeField] private float heightScale = 1.0f;

        [SerializeField] private PatternAsset patternAsset;
        [SerializeField] private PatternAsset noPQPatternAsset;

        private int[] _pqIndices;
        private int[] _pqIntensities;
        private int _pqSignalIndex;
        private bool _pqPending;
        private PatternAsset _runtimePattern;

        private void Start()
        {
            Instance = this;
            if (hitEffect != null && skineticDevice != null)
            {
                hitEffect.TargetDevice = skineticDevice;
                hitEffect.StrategyOnPlay = HapticEffect.PlayStrategy.E_FORCE;
            }
        }

        public void RegisterPQFirePayload(int[] indices, int[] intensities, int signalIndex)
        {
            _pqIndices = indices;
            _pqIntensities = intensities;
            _pqSignalIndex = signalIndex;
            _pqPending = true;
        }

        public void OnPQBulletHit()
        {
            if (!_pqPending) return;
            _pqPending = false;
            PlayEffect(_pqIndices, _pqIntensities, _pqSignalIndex);
        }

        public bool HasLastEffect => _pqIndices != null;

        public void LoadNoPQPattern()
        {
            if (noPQPatternAsset == null || skineticDevice == null || hitEffect == null) return;
            hitEffect.StopEffect(0f);
            skineticDevice.LoadPattern(noPQPatternAsset);
            hitEffect.TargetPattern = noPQPatternAsset;
        }

        public void ArmLastEffect()
        {
            if (_pqIndices == null) return;
            _pqPending = true;
        }

        public void PlayLastEffect()
        {
            if (_pqIndices == null) return;
            PlayEffect(_pqIndices, _pqIntensities, _pqSignalIndex);
        }

        // CHANGED: Now takes 'Collision' instead of 'Transform'
        public void PlayImpactHaptic(Collision collision)
        {
            Debug.Log("PlayImpactHaptic");
            if (hitEffect == null || skineticDevice == null || collision.contactCount == 0) return;
            Debug.Log("PlayImpactHaptic cleared");
            // 1. Get Exact Contact Point
            // GetContact(0) gives the first point where physics meshes touched.
            Vector3 contactCenter = collision.GetContact(0).point;

            // 2. Determine "Shooter" Position 
            // We assume the impact came from the direction the bullet was traveling
            // -collision.relativeVelocity is often used, but -transform.forward of the bullet is safer for projectiles.
            Vector3 incomingDirection = -collision.transform.forward;
            Vector3 pseudoShooterPos = contactCenter + incomingDirection;

            // 3. Perform Spatial Calculations
            float heightTranslation = (contactCenter.y - transform.position.y) / heightScale;

            Vector3 impactVector = pseudoShooterPos - contactCenter;

            // Yaw (Heading)
            Vector3 projectedImpact = Vector3.ProjectOnPlane(impactVector, transform.up).normalized;
            float headingRotation = Vector3.SignedAngle(transform.forward, projectedImpact, transform.up);

            // Pitch (Tilting)
            float tiltingRotation = Vector3.SignedAngle(
                Vector3.ProjectOnPlane(impactVector, transform.up),
                impactVector,
                Vector3.Cross(impactVector, transform.up)
            );

            // 4. Play Effect
            hitEffect.HeightTranslation = heightTranslation;
            hitEffect.HeadingRotation = headingRotation;
            hitEffect.TiltingRotation = tiltingRotation;
            hitEffect.PlayEffect();
        }

        public void PlayEffect(int[] actuatorIndexArray, int[] intensityArray, int signalIndex = 21)
        {
            Debug.Log($"[PlayEffect] called — signalIndex={signalIndex}");

            if (patternAsset == null)
            {
                Debug.LogError("[PlayEffect] PatternAsset not assigned.");
                return;
            }

            if (actuatorIndexArray == null || intensityArray == null || actuatorIndexArray.Length != 4 || intensityArray.Length != 4)
            {
                Debug.LogError($"[PlayEffect] Bad arrays — indices={actuatorIndexArray?.Length} intensities={intensityArray?.Length}");
                return;
            }

            string templateJson = GetPatternJson(patternAsset);
            if (string.IsNullOrEmpty(templateJson))
            {
                Debug.LogError("[PlayEffect] Could not read json from PatternAsset.");
                return;
            }

            JObject root = JObject.Parse(templateJson);

            int[] weightArray = new int[20];
            for (int i = 0; i < 4; i++)
            {
                int actuatorIndex = actuatorIndexArray[i];
                int intensity = Mathf.Clamp(intensityArray[i], 0, 100);
                int weight = MapToSpnWeight(intensity);
                if (actuatorIndex < 0 || actuatorIndex > 19)
                {
                    Debug.LogError($"Invalid actuator index: {actuatorIndex}");
                    return;
                }
                weightArray[actuatorIndex] = weight;
            }

            Debug.Log("Weights = [" + string.Join(", ", weightArray) + "]");

            JArray weightJArray = new JArray();
            for (int i = 0; i < 20; i++) weightJArray.Add(weightArray[i]);
            root["tracks"][0]["samples"][0]["signalIndex"] = signalIndex;
            root["tracks"][0]["samples"][0]["spatKeyframes"][0]["weights"] = weightJArray;
            root["tracks"][0]["samples"][0]["startCrop"] = signalIndex == 163 ? 3.69 : 0;

            string updatedJson = root.ToString();

            if (_runtimePattern != null) Destroy(_runtimePattern);
            _runtimePattern = Instantiate(patternAsset);
            _runtimePattern.name = "Runtime_" + Time.frameCount;

            SetPatternJson(_runtimePattern, updatedJson);

            hitEffect.StopEffect(0f);
            skineticDevice.LoadPattern(_runtimePattern);
            hitEffect.TargetPattern = _runtimePattern;
            Debug.Log("[PlayEffect] calling hitEffect.PlayEffect()");
            hitEffect.PlayEffect();
            Debug.Log("[PlayEffect] done");
        }

        public static int MapToSpnWeight(int value)
        {
            int[] weights = { 0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 5, 5, 5, 5, 5, 6, 6, 6, 7, 7, 7, 8, 8, 8, 9, 9, 10, 10, 10, 11, 11, 12, 13, 13, 14, 14, 15, 16, 17, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 28, 29, 30, 32, 33, 35, 36, 38, 40, 42, 44, 46, 48, 50, 52, 55, 58, 60, 63, 66, 69, 72, 76, 79, 83, 87, 91, 95, 100 };

            value = Mathf.Clamp(value, 0, 100);
            return weights[value];
        }

        private static string GetPatternJson(PatternAsset patternAsset)
        {
            return TryGetStringMember(patternAsset, "Json");
        }

        private static void SetPatternJson(PatternAsset patternAsset, string json)
        {
            TrySetMember(patternAsset, "Json", json);
        }

        private static string TryGetStringMember(object obj, string memberName)
        {
            var t = obj.GetType();
            var prop = t.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null) return prop.GetValue(obj)?.ToString();

            var field = t.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null) return field.GetValue(obj)?.ToString();

            return null;
        }

        private static void TrySetMember(object obj, string memberName, object value)
        {
            var t = obj.GetType();

            var prop = t.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(obj, value);
                return;
            }

            var field = t.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
                field.SetValue(obj, value);
        }
    }
}