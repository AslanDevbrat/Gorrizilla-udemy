using UnityEngine;
using SocketIOUnity.Runtime;
using SocketIOUnity.Transport;

public sealed class SocketIOManager : MonoBehaviour
{
    public static SocketIOManager Instance { get; private set; }

    public SocketIOClient Socket { get; private set; }

    [SerializeField] private string serverUrl = "http://wan.local:3000";

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Socket = new SocketIOClient(TransportFactoryHelper.CreateDefault());
    }

    private void Start()
    {
        Socket.OnConnected += () =>
        {
            Debug.Log("Socket.IO connected");
        };

        Socket.On("fire", OnFire);
        Socket.On("modeChange", OnModeChange);

        Socket.Connect(serverUrl);
    }

    [System.Serializable]
    private class ModeChangePayload { public string mode; }
    [System.Serializable]
    private class HapticZone { public int index; public int intensity; }

    [System.Serializable]
    private class FirePayload
    {
        public string mode;
        public string signalIndex;
        public HapticZone upperFront;
        public HapticZone lowerFront;
        public HapticZone upperBack;
        public HapticZone lowerBack;
    }

    private void OnModeChange(string message)
    {
        Debug.Log($"[Mode Change] Server: {message}");
        var payload = JsonUtility.FromJson<ModeChangePayload>(message);
        if (payload == null) return;

        GorillaZilla.GameManager.Mode = payload.mode;

        if (payload.mode != "PQ")
        {
            var haptics = GorillaZilla.PlayerHaptics.Instance;
            if (haptics != null) haptics.LoadNoPQPattern();
        }

        if (payload.mode == "PQ")
        {
            foreach (var bullet in FindObjectsByType<Bullet>(FindObjectsSortMode.None))
                Destroy(bullet.gameObject);
        }
    }

    private void OnFire(string message)
    {
        Debug.Log($"[Fire] Server: {message}");
        var payload = JsonUtility.FromJson<FirePayload>(message);
        if (payload == null) return;

        if (GorillaZilla.GameManager.Mode == "PQ")
        {
            GorillaZilla.GameManager.TriggerPQFire();

            var haptics = GorillaZilla.PlayerHaptics.Instance;
            if (haptics != null)
            {
                int sig = int.TryParse(payload.signalIndex, out int s) ? s : 21;
                int[] indices     = { payload.upperFront.index,     payload.lowerFront.index,     payload.upperBack.index,     payload.lowerBack.index };
                int[] intensities = { payload.upperFront.intensity, payload.lowerFront.intensity, payload.upperBack.intensity, payload.lowerBack.intensity };
                haptics.RegisterPQFirePayload(indices, intensities, sig);
            }
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Socket?.Shutdown();
            Instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        Socket?.Shutdown();
    }
}