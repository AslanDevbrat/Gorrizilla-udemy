using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Skinetic
{
    /// <summary>
    /// Component to handle the Haptic Device's global behavior.
    /// </summary>
    [AddComponentMenu("Skinetic/Skinetic Device")]
    public class SkineticDevice : MonoBehaviour
    {
        /// <summary>
        /// Type of connection to Skinetic.
        /// </summary>
        /// <remarks>
        /// E_AUTODETECT will try all available type of connection in the following order:
        /// - Bluetooth
        /// - USB
        /// - WIFI
        /// </remarks>
        public enum OutputType
        {
            /// <summary>Try all available type of connection.</summary>
            E_AUTODETECT = 0,
            /// <summary>Bluetooth connection.</summary>
            E_BLUETOOTH = 1,
            /// <summary>USB connection.</summary>
            E_USB = 2,
            /// <summary>Wifi connection.</summary>
            E_WIFI = 3,
        }

        /// <summary>
        /// Type of Skinetic device.
        /// </summary>
        public enum DeviceType
        {
            /// <summary>Type is Unknown or undefined.</summary>
            E_UNKNOWN = ~0x0,
            /// <summary>Skinetic Vest.</summary>
            E_SKINETICVEST = 0x01100101,         
            /// <summary>HSD mk.II development kit.</summary>
            E_HSDMK2 = 0x0122FFFF
        }

        /// <summary>
        /// Connection state.
        /// </summary>
        public enum ConnectionState
        {
            
            /// <summary> Device connection was broken, trying to reconnect.</summary>
            E_RECONNECTING = 3,
            /// <summary> Device is disconnecting, releasing all resources.</summary>
            E_DISCONNECTING = 2,
            /// <summary> Connection to the device is being established, connection routine is active.</summary>
            E_CONNECTING = 1,
            /// <summary> Device is connected.</summary>
            E_CONNECTED = 0,
            /// <summary> Device is disconnected.</summary>
            E_DISCONNECTED = -1,         
        }

        /// <summary>
        /// Describe the error cause.
        /// </summary>
        /// <param name="error"> value</param>
        /// <returns>Error message.</returns>
        static public string getSDKError(int error)
        {
            switch (error)
            {
                case 0:
                    return "No Error";
                case -1:
                    return "Other";
                case -2:
                    return "Invalid parameter";
                case -3:
                    return "No device connected";
                case -4:
                    return "Output is not supported on this platform";
                case -5:
                    return "Invalid Json";
                case -6:
                    return "Device not reachable";
                case -7:
                    return "A priority command is waiting to be processed";
                case -8:
                    return "No available slot on the board";
                case -9:
                    return "No Skinetic instance created";
                case -10:
                    return "Received an invalid message";
                case -11:
                    return "Process is already running";
                case -12:
                    return "A device is already connected";
                case -13:
                    return "The initialization of the device has been interrupted";
                case -14:
                    return "Play was ignored due to overall trigger strategy";
                case -15:
                    return "PortAudio raised an error";
                case -16:
                    return "An error happened with the socket";
                case -50:
                    return "Error in JNI layer";
                case -100:
                    return "Core Error: Invalid argument";
                case -99:
                    return "Core Error: Invalid spn";
                case -98:
                    return "Core Error: Invalid layout";
                case -97:
                    return "Core Error: ID already allocated";
                case -96:
                    return "Core Error: Invalid sequence ID";
                case -95:
                    return " Core Error: Invalid pattern ID";
                case -94:
                    return "Core Error: Pattern in use";
                case -93:
                    return "Core Error: Sequence already set to play";
                default:
                    return "Unknown error";
            }
        }

        /// <summary>Struct representing device information.</summary>
        public struct DeviceInfo
        {
            /// <summary> Available Output connection mode. </summary>
            public OutputType outputType;
            /// <summary> Device Serial Number. </summary>
            public System.UInt32 serialNumber;
            /// <summary> Device Type. </summary>
            public DeviceType deviceType;
            /// <summary> Device Version. </summary>
            public string deviceVersion;
        }


        /// <summary>
        /// Haptic Structure to describe how effect instances reproduce a pattern with variations.
        /// </summary>
        /// <remarks>
        /// The spatialization properties (height, heading and tilting) allows to apply
        /// the pattern on the haptic device at a different location by
        /// translating/rotating it or performing some inversion/addition.
        /// Notice that combining additions greatly increase the processing time of the
        /// transformation.If the pattern possesses too many shapes and keys, a perceptible delay might be induced.
        /// 
        /// The three transformations are applied in this order: tilting, vertical rotation, vertical translation.
        /// The default position of a pattern is the one obtained when these three parameters are set to zero. 
        /// The actual use of these 3 parameters depends on the default position of the pattern and the targeted interaction:
        /// e.g.; for a piercing shot, a heading between[-180; 180]° can be combined with a tilting between[-90; 90] when 
        /// using a shape-based pattern centered in the middle of the torso; for a environmental effect,
        /// a heading between[-90; 90]° (or[-180; 180]°) can be combined with a tilting between 
        /// [-180; 180]° (resp. [-180; 0]°) when using a pattern with shapes centered on the top, etc.
        /// There are no actual bounds to the angles as not to restrict the usage.
        /// Notice that actuator-based patterns cannot be transformed in this version.
        /// 
        /// The global boost intensity is applied to every effects being rendered as to increase them evenly. However, some
        /// effects are by design stronger than others. Hence, they all have a default boost value in the.spn that is added
        /// to the global boost intensity, and which can be set to compensate the discrepancy of intensity across a set of
        /// patterns. Weaker effects can have a high default boost value while, already strong effects can have a negative
        /// default boost value as to prevent the global boost intensity set by the user to increase the perceived intensity
        /// too much.Note that the resulting boost value is clamp between 0 and 100.
        /// When an instance of an effect is being rendered, the default boost value of the pattern, the one set in the design
        /// process, is used.If the boolean overridePatternBoost is set to true, the passed value effectBoost is used instead
        /// of the default one.
        /// 
        /// Since all effects cannot be rendered simultaneously, the least priority ones are muted until
        /// the more priority ones are stopped of finished rendering. Muted effects are still running,
        /// but not rendered.
        /// 
        /// The priority order is obtain using the priority level: priority increase from 10 to 1. In case
        /// of equality, the number of required simultaneous samples is used to determine which effect has the highest
        /// priority: effects using less simultaneous samples have a higher priority.Again, if the number of required
        /// simultaneous samples is the same, the most recent effect has a higher priority.
        /// </remarks>
        [System.Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
        public struct EffectProperties
        {
            /// <summary>Level of priority [1; 10] (default - 5). In case too 
            /// many effects are playing simultaneously, the effect with lowest
            /// priority(10) will be muted.</summary>
            public int priority;
            /// <summary>Percentage of the base volume between [0; 250]% (default - 100): [0;100[% the pattern attenuated, 
            /// 100% the pattern's base volume is preserved, ]100; 250]% the pattern is amplified. 
            /// Too much amplification may lead to the clipping of the haptic effects, distorting them 
            /// and producing audible artifacts.</summary>
            public float volume;
            /// <summary>Time scale between [0.01; 100] (default - 1): [0.01; 1[ the pattern is slowed down, 1 the pattern 
            /// timing is preserved, ]1; 100] the pattern is accelerated.The resulting speed between 
            /// the haptic effect's and the samples' speed within the pattern cannot exceed these 
            /// bounds. Slowing down or accelerating a sample too much may result in an haptically poor effect.</summary>
            public float speed;
            /// <summary>Number of repetition of the pattern (default - 1) if the maxDuration is not reached. 
            /// If set to 0, the pattern is repeat indefinitely until it is either stopped with stopEffect() 
            /// or reach the maxDuration value.</summary>
            public int repeatCount;
            /// <summary>Pause in second between to repetition of the pattern (default - 0). This value is not
            /// affected by the speed parameter.</summary>
            public float repeatDelay;
            /// <summary>Time in the pattern at which the effect start to play (default - 0). This value need to be 
            /// lower than the maxDuration.It also takes into account the repeatCount and the 
            /// repeatDelay of the pattern.</summary>
            public float playAtTime;
            /// <summary>Maximum duration of the effect (default - 0), it is automatically stopped if the duration 
            /// is reached without any regards for the actual state of the repeatCount.A maxDuration 
            /// of 0 remove the duration limit, making the effect ables to play indefinitely.</summary>
            public float maxDuration;
            /// <summary>Boost intensity level percent [-100; 100] (default - 0) of the effect to use instead of the 
            /// default pattern value if overridePatternBoost is set to true. By using a negative value, can decrease or 
            /// even nullify the global intensity boost set by the user.</summary>
            public int effectBoost;
            /// <summary>By setting this boolean to true (default - false), the effect will use the 
            /// effectBoost value instead of the default pattern value.</summary>
            [MarshalAs(UnmanagedType.I1)]
            public Boolean overridePatternBoost;
            /// <summary>Normalized height [-1; 1] to translate the pattern by (default - 0). A positive value translate 
            /// the pattern upwards.</summary>
            public float height;
            /// <summary>Heading angle (in degree) to rotate the pattern by in the horizontal plan (default - 0). A positive 
            /// value rotates the pattern to the left of the vest.</summary>
            public float heading;
            /// <summary>Tilting angle (in degree) to rotate the pattern by in the sagittal plan (default - 0). A positive 
            /// value rotates the pattern upwards from front to back.</summary>
            public float tilting;
            /// <summary>Invert the direction of the pattern on the front-back axis (default - false). Can be combine with other 
            /// inversion or addition.</summary>
            [MarshalAs(UnmanagedType.I1)]
            public Boolean frontBackInversion;
            /// <summary>Invert the direction of the pattern on the up-down axis (default - false). Can be combine with other 
            /// inversion or addition.</summary>
            [MarshalAs(UnmanagedType.I1)]
            public Boolean upDownInversion;
            /// <summary>Invert the direction of the pattern on the right-left axis (default - false). Can be combine with other 
            /// inversion or addition.</summary>
            [MarshalAs(UnmanagedType.I1)]
            public Boolean rightLeftInversion;
            /// <summary>Perform a front-back addition of the pattern on the front-back axis (default - false). Overrides the 
            /// frontBackInversion.Can be combine with other inversion or addition.</summary>
            [MarshalAs(UnmanagedType.I1)]
            public Boolean frontBackAddition;
            /// <summary>Perform a up-down addition of the pattern on the front-back axis (default - false). Overrides the 
            /// upDownInversion.Can be combine with other inversion or addition.</summary>
            [MarshalAs(UnmanagedType.I1)]
            public Boolean upDownAddition;
            /// <summary>Perform a right-left addition of the pattern on the front-back axis (default - false). Overrides the 
            /// rightLeftInversion.Can be combine with other inversion or addition.</summary>
            [MarshalAs(UnmanagedType.I1)]
            public Boolean rightLeftAddition;

            public EffectProperties(int priority, float volume, float speed, int repeatCount, float repeatDelay, 
                                    float playAtTime, float maxDuration, int effectBoost, bool overridePatternBoost, 
                                    float height, float heading, float tilting,
                                    bool frontBackInversion, bool upDownInversion, bool rightLeftInversion,
                                    bool frontBackAddition, bool upDownAddition, bool rightLeftAddition)
            {
                this.priority = priority;
                this.volume = volume;
                this.speed = speed;
                this.repeatCount = repeatCount;
                this.repeatDelay = repeatDelay;
                this.playAtTime = playAtTime;
                this.maxDuration = maxDuration;
                this.effectBoost = effectBoost;
                this.overridePatternBoost = overridePatternBoost;
                this.height = height;
                this.heading = heading;
                this.tilting = tilting;
                this.frontBackInversion = frontBackInversion;
                this.upDownInversion = upDownInversion;
                this.rightLeftInversion = rightLeftInversion;
                this.frontBackAddition = frontBackAddition;
                this.upDownAddition = upDownAddition;
                this.rightLeftAddition = rightLeftAddition;
            }
        }

        private ISkinetic m_skineticInstance;
        private Dictionary<PatternAsset, int> m_patternsIDs;
        private List<PatternAsset> m_loadedPatternAssets;

        /// @cond For internal usage
        public List<PatternAsset> m_patternToPreload;
        /// @endcond

        /// <summary>
        /// Access list of already loaded pattern.
        /// </summary>
        /// <remarks>
        /// This list cannot be modified, use LoadPattern()/UnloadPattern() instead.
        /// </remarks>
        public IReadOnlyList<PatternAsset> LoadedPatterns { get => m_loadedPatternAssets.AsReadOnly(); }

        private void OnEnable()
        {
            //init m_skineticInstance using define

#if UNITY_ANDROID &&!UNITY_EDITOR
            m_skineticInstance = new SkineticAndroid();
#else
            m_skineticInstance = new SkineticWrapping();
#endif
            if (m_patternsIDs == null)
                m_patternsIDs = new Dictionary<PatternAsset, int>();
            if (m_loadedPatternAssets == null)
                m_loadedPatternAssets = new List<PatternAsset>();

            m_skineticInstance.InitInstance();
            PreLoadPatternList();
        }

        private void OnDisable()
        {
            m_skineticInstance.DeinitInstance();
        }

        /// <summary>
        /// Convert Skinetic serial number to a formatted string.
        /// </summary>
        /// <param name="serialNumber">serial number to convert</param>
        /// <returns>string representation of the serial number</returns>
        static public String SerialNumberToString(System.UInt32 serialNumber)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return SkineticAndroid.SerialNumberToString(serialNumber);
#else
            return SkineticWrapping.SerialNumberToString(serialNumber);
#endif
        }


        /// <summary>
        /// Initialize a scanning routine to find all available Skinetic device.
        /// </summary>
        /// <remarks>
        /// the state of the routine can be obtain from ScanStatus(). Once completed,
        /// the result can be accessed using GetScannedDevices().
        /// </remarks>
        /// <param name="output"></param>
        /// <returns>0 on success, an Error code on failure.</returns>
        public int ScanDevices(SkineticDevice.OutputType output)
        {
            return m_skineticInstance.ScanDevices(output);
        }

        /// <summary>
        /// Check the status of the asynchronous scanning routine.
        /// </summary>
        /// <remarks>
        /// The method returns:
        ///  - '1' if the scan is ongoing.
        ///  - '0' if the scan is completed.
        ///  - a negative error code if the connection failed.
        /// The asynchronous scan routine is terminated on failure.
        /// Once the scan is completed, the result can be obtain by calling GetScannedDevices().
        /// </remarks>
        /// <returns>the current status or an error on failure.</returns>
        public int ScanStatus()
        {
            return m_skineticInstance.ScanStatus();
        }

        /// <summary>
        /// This function returns a list of all DeviceInfo of each Skinetic devices found during the scan
        /// which match the specified output type.
        /// </summary>
        /// <returns>a list of DeviceInfo, empty if no match or an error occurs.</returns>
        public List<SkineticDevice.DeviceInfo> GetScannedDevices()
        {
            return m_skineticInstance.GetScannedDevices();
        }

        /// <summary>
        /// Initialize an asynchronous connection to a Skinetic device using the selected type of connection.
        /// </summary>
        /// <remarks>
        /// The state of the routine can be obtain from ConnectionStatus().
        /// If the serial number is set to '0', the connection will be performed on the first found device.
        /// </remarks>
        /// <param name="output">output type</param>
        /// <param name="serialNumber">serial number of the Skinetic device to connect to</param>
        /// <returns>0 on success, an error otherwise.</returns>
        public int Connect(SkineticDevice.OutputType output, System.UInt32 serialNumber)
        {
            return m_skineticInstance.Connect(output, serialNumber);
        }

        /// <summary>
        /// Disconnect the current Skinetic device.
        /// </summary>
        /// <remarks>
        /// The disconnection is effective once all resources are released.
        /// The state of the routine can be obtain from ConnectionStatus().
        /// </remarks>
        /// <returns>0 on success, an error otherwise.</returns>
        public int Disconnect()
        {
            return m_skineticInstance.Disconnect();
        }

        /// <summary>
        /// Check the current status of the connection.
        /// </summary>
        /// <remarks>
        /// The asynchronous connection routine is terminated on failure.
        /// </remarks>
        /// <returns>the current status of the connection.</returns>
        public SkineticDevice.ConnectionState ConnectionStatus()
        {
            return m_skineticInstance.ConnectionStatus();
        }

        /// <summary>
        /// Delegate of the connection callback.
        /// </summary>
        /// <remarks>
        /// Functions of type ski_ConnectionCallback are implemented by clients.
        /// The callback is fired at the end of the connection routine whether it succeeds
        /// or failed.
        /// It is also fired if a connection issue arise.
        /// </remarks>
        /// <param name="state"> status of the connection</param>
        /// <param name="error"> of error occurring</param>
        /// <param name="serialNumber"> serial number of the device firing the callback</param>
        public delegate void ConnectionCallbackDelegate(ConnectionState state, int error, System.UInt32 serialNumber);

        /// <summary>
        /// Set a callback function fired upon connection changes.
        /// </summary>
        /// <remarks>
        /// Functions of type ski_ConnectionCallback are implemented by clients.
        /// The callback is fired at the end of the connection routine whether it succeeds
        /// or failed.It is also fired if a connection issue arise.
        /// The callback is not fired if none was passed to setConnectionCallback().
        /// The callback is not executed by the main thread. Hence, the actions that can be 
        /// performed by it are restricted by Unity.
        /// </remarks>
        /// <param name="callback">callback client's callback</param>
        /// <returns>0 on success, an Error code on failure.</returns>
        public int SetConnectionCallback(ConnectionCallbackDelegate callback)
        {
            return m_skineticInstance.SetConnectionCallback(callback);
        }


        /// <summary>
        /// Get SDK version as a string.
        /// </summary>
        /// <remarks>
        /// The format of the string is: <pre>major.minor.revision</pre>
        /// </remarks>
        /// <returns>The version string.</returns>
        public string GetSDKVersion()
        {
            return m_skineticInstance.GetSDKVersion();
        }

        /// <summary>
        /// Get the connected device's version as a string.
        /// </summary>
        /// <remarks>
        /// The format of the string is: <pre>major.minor.revision</pre>
        /// </remarks>
        /// <returns>The version string if a Skinetic device is connected, 
        /// an error message otherwise.</returns>
        public string GetDeviceVersion()
        {
            return m_skineticInstance.GetDeviceVersion();
        }

        /// <summary>
        /// Get the connected device's serial number.
        /// </summary>
        /// <returns>The serial number of the connected Skinetic 
        /// device if any, 0xFFFFFFFF otherwise.</returns>
        public System.UInt32 GetDeviceSerialNumber()
        {
            return m_skineticInstance.GetDeviceSerialNumber();
        }

        /// <summary>
        /// Get the connected device's serial number as string.
        /// </summary>
        /// <returns>The serial number as string of the connected Skinetic device 
        /// if any, "noDeviceConnected" otherwise.</returns>
        public string GetDeviceSerialNumberAsString()
        {
            return m_skineticInstance.GetDeviceSerialNumberAsString();
        }

        /// <summary>
        /// Get the connected device's type.
        /// </summary>
        /// <returns>The type of the connected Skinetic device if it is connected,
        /// an ERROR message otherwise.</returns>
        public SkineticDevice.DeviceType GetDeviceType()
        {
            return m_skineticInstance.GetDeviceType();
        }

        /// <summary>
        /// Get the amount of effect's intensity boost.
        /// </summary>
        /// <remarks>
        /// The boost increase the overall intensity of all haptic effects.
        /// However, the higher the boost activation is, the more the haptic effects are degraded.
        /// The global boost is meant to be set by the user as an application setting.
        /// </remarks>
        /// <returns>The percentage of effect's intensity boost, an ERROR otherwise.</returns>
        public int GetGlobalIntensityBoost()
        {
            return m_skineticInstance.GetGlobalIntensityBoost();
        }

        /// <summary>
        /// Set the amount of global intensity boost.
        /// </summary>
        /// <remarks>
        /// The boost increase the overall intensity of all haptic effects.
        /// However, the higher the boost activation is, the more the haptic effects are degraded.
        /// The global boost is meant to be set by the user as an application setting.
        /// </remarks>
        /// <param name="globalBoost">boostPercent percentage of the boost.</param>
        /// <returns>0 on success, an ERROR otherwise.</returns>
        public int SetGlobalIntensityBoost(int globalBoost)
        {
            return m_skineticInstance.SetGlobalIntensityBoost(globalBoost);
        }

        /// <summary>
        /// Load a pattern into the device from a PatternAsset.
        /// </summary>
        /// <param name="pattern">PatternAsset to load.</param>
        /// <returns>True on success, false otherwise.</returns>
        public bool LoadPattern(PatternAsset pattern)
        {
            if (m_patternsIDs.ContainsKey(pattern))
                return true;

            int ret = m_skineticInstance.LoadPatternFromJSON(pattern.Json);
            if(ret < 0)
            {
                Debug.Log(getSDKError(ret));
                return false;
            }
            m_loadedPatternAssets.Add(pattern);
            m_patternsIDs.Add(pattern, ret);
            return true;
        }

        /// <summary>
        /// Unload a pattern into the device from a PatternAsset.
        /// </summary>
        /// <param name="pattern">PatternAsset to unload.</param>
        /// <returns>True if the pattern is successfully found and removed, false otherwise.</returns>
        public bool UnloadPattern(PatternAsset pattern)
        {
            int ret;
            if (!m_patternsIDs.TryGetValue(pattern, out ret))
                return false;
            ret = m_skineticInstance.UnloadPattern(ret);
            if (ret < 0)
            {
                Debug.Log(getSDKError(ret));
                return false;
            }
            return (m_patternsIDs.Remove(pattern) && m_loadedPatternAssets.Remove(pattern));
        }

        /// <summary>
        /// Get the pattern boost value which serves as a default value for the playing effect.
        /// </summary>
        /// <remarks>
        /// The value is ranged in [-100; 100]. If the pattern ID is invalid, zero is still returned.
        /// </remarks>
        public int GetPatternBoost(PatternAsset pattern)
        {
            int ret;
            if (!m_patternsIDs.TryGetValue(pattern, out ret))
                return 0;
            return m_skineticInstance.GetPatternIntensityBoost(ret);
        }

        /// <summary>
        /// Enable the effect accumulation strategy
        /// </summary>
        /// <remarks>
        /// Whenever an effect is triggered on the main pattern,
        /// the fallback one is used instead, if the main is already playing. More details can be found the
        /// additional documentation.
        /// For the maxAccumulation, setting to 0 removes the limit.
        ///
        /// If a new call to this function is done for a specific pattern, the previous association is overridden.
        /// </remarks>
        /// <param name="mainPattern">main pattern of the accumulation.</param>
        /// <param name="fallbackPattern">fallback pattern of the accumulation</param>
        /// <param name="timeWindow">the time window during which the accumulation should happen.</param>
        /// <param name="maxAccumulation">max number of extra accumulated effect instances.</param>
        /// <returns>True if the accumulation is successfully set, false otherwise.</returns>
        public bool SetAccumulationWindowToPattern(PatternAsset mainPattern, PatternAsset fallbackPattern, float timeWindow, int maxAccumulation)
        {
            int ret, mainID, fallbackID;
            if (!m_patternsIDs.TryGetValue(mainPattern, out mainID))
                return false;
            if (!m_patternsIDs.TryGetValue(fallbackPattern, out fallbackID))
                return false;
            ret = m_skineticInstance.SetAccumulationWindowToPattern(mainID, fallbackID, timeWindow, maxAccumulation);
            if (ret < 0)
            {
                Debug.Log(getSDKError(ret));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Disable the effect accumulation strategy on a specific pattern if any set.
        /// </summary>
        /// <param name="mainPattern">Main pattern asset</param>
        /// <returns>0 on success, an ERROR otherwise.</returns>
        public bool EraseAccumulationWindowToPattern(PatternAsset mainPattern)
        {
            int ret;
            if (!m_patternsIDs.TryGetValue(mainPattern, out ret))
                return false;

            ret = m_skineticInstance.EraseAccumulationWindowToPattern(ret);
            if (ret < 0)
            {
                Debug.Log(getSDKError(ret));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Play an haptic effect based on a loaded pattern.
        /// </summary>
        /// <remarks>
        /// The instance index is positive. Each call to playEffect() using the same patternID
        /// generates a new haptic effect instance totally uncorrelated to the previous ones.
        /// The instance is destroyed once it stops playing.
        /// 
        /// The haptic effect instance reproduces the pattern with variations describes in the structure
        /// ski_effect_properties_t.More information on these parameters and how to used them can be found
        /// in the structure's description.
        /// 
        /// If the pattern is unloaded, the haptic effect is not interrupted.
        /// </remarks>
        /// <param name="hapticEffect">reference to the haptic effect instance.</param>
        /// <returns>true on success, false otherwise.</returns>
        public bool PlayEffect(HapticEffect hapticEffect)
        {
            int patternid, ret;
            if (!m_patternsIDs.TryGetValue(hapticEffect.TargetPattern, out patternid))
                return false;

            switch (hapticEffect.StrategyOnPlay)
            {
                case HapticEffect.PlayStrategy.E_DEFAULT:
                    if (GetEffectState(hapticEffect) != HapticEffect.State.E_STOP)
                        return false;
                    break;

                case HapticEffect.PlayStrategy.E_FORCE:
                    StopEffect(hapticEffect, 0);
                    break;

                case HapticEffect.PlayStrategy.E_PULLED:
                    break;

                default:
                    return false;
            }

            ret = m_skineticInstance.PlayEffect(patternid, hapticEffect.Properties);
            if (ret < 0)
            {
                Debug.Log(getSDKError(ret));
                return false;
            }
            hapticEffect.InternalIDs.Add(ret);
            return true;
        }

        /// <summary>
        /// Stop the effect instance.
        /// </summary>
        /// <remarks>
        /// The effect is stop in "time" seconds with a fade out to prevent abrupt transition. If time
        /// is set to 0, no fadeout are applied and the effect is stopped as soon as possible.
        /// </remarks>
        /// <param name="hapticEffect">reference to the haptic effect instance.</param>
        /// <param name="time">duration of the fadeout in second.</param>
        /// <returns>true on success, false otherwise.</returns>
        public bool StopEffect(HapticEffect hapticEffect, float time)
        {
            int ret;
            bool bret = true;
            if (hapticEffect.InternalIDs.Count == 0)
                return false;

            for (int i = 0; i < hapticEffect.InternalIDs.Count; i++)
            {
                if (m_skineticInstance.GetEffectState(hapticEffect.InternalIDs[i]) == HapticEffect.State.E_STOP)
                    continue;
                ret = m_skineticInstance.StopEffect(hapticEffect.InternalIDs[i], time);
                if (ret < 0)
                {
                    bret = false;
                    Debug.Log(getSDKError(ret));
                }
            }
            hapticEffect.InternalIDs.Clear();
            return bret;
        }

        /// <summary>
        ///  Get the current state of an effect.
        /// </summary>
        /// <remarks>
        /// If the haptic effect is invalid, the 'stop' state will be return.
        /// </remarks>
        /// <param name="hapticEffect">reference to the haptic effect instance.</param>
        /// <returns>the current state of the effect.</returns>
        public HapticEffect.State GetEffectState(HapticEffect hapticEffect)
        {
            HapticEffect.State state;
            if (hapticEffect.InternalIDs.Count == 0)
                return HapticEffect.State.E_STOP;
            switch (hapticEffect.StrategyOnPlay)
            {
                case HapticEffect.PlayStrategy.E_DEFAULT:
                    state =  m_skineticInstance.GetEffectState(hapticEffect.InternalIDs[0]);
                    break;

                case HapticEffect.PlayStrategy.E_FORCE:
                    state =  m_skineticInstance.GetEffectState(hapticEffect.InternalIDs[0]);
                    break;

                case HapticEffect.PlayStrategy.E_PULLED:
                    state = HapticEffect.State.E_STOP;
                    for (int i = hapticEffect.InternalIDs.Count - 1; i >= 0; i--)
                    {
                        if (m_skineticInstance.GetEffectState(hapticEffect.InternalIDs[i]) > state)
                            state = m_skineticInstance.GetEffectState(hapticEffect.InternalIDs[i]);
                        if (m_skineticInstance.GetEffectState(hapticEffect.InternalIDs[i]) == HapticEffect.State.E_STOP)
                            hapticEffect.InternalIDs.Remove(i);
                    }
                    break;

                default:
                    state = HapticEffect.State.E_STOP;
                    break;
            }

            if (state == HapticEffect.State.E_STOP)
                hapticEffect.InternalIDs.Clear();
            return state;
        }

        /// <summary>
        /// Pause all haptic effect that are currently playing.
        /// </summary>
        /// <returns>true on success, false otherwise.</returns>
        public bool PauseAll()
        {
            int ret = m_skineticInstance.PauseAll();
            if (ret < 0)
            {
                Debug.Log(getSDKError(ret));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Resume the paused haptic effects.
        /// </summary>
        /// <returns>true on success, false otherwise.</returns>
        public bool ResumeAll()
        {
            int ret = m_skineticInstance.ResumeAll();
            if (ret < 0)
            {
                Debug.Log(getSDKError(ret));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Stop all playing haptic effect.
        /// </summary>
        /// <returns>true on success, false otherwise.</returns>
        public bool StopAll()
        {
            int ret = m_skineticInstance.StopAll();
            if (ret < 0)
            {
                Debug.Log(getSDKError(ret));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Initialize an asynchronous connection to an audio device using the
        /// provided settings.
        /// </summary>
        /// <remarks>
        /// This method allows to handle the settings on your own through scripting.
        /// If audioPreset is set to anything else other than
        /// AudioPreset::E_CUSTOMDEVICE, the provided settings are ignored and
        /// the ones corresponding to the preset are used instead.
        /// Not available for Android.
        /// </summary>
        /// <param name="audioPreset">preset of audio device.</param>
        /// <param name="audioSettings">stream settings.</param>
        /// <returns>0 on success, an error otherwise.</returns>
        public int ExpConnectAudio(Experimental.AudioStreamConfiguration.AudioPreset audioPreset, Experimental.AudioStreamConfiguration.AudioSettings audioSettings)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return -4;
#else
            return ((SkineticWrapping)m_skineticInstance).Exp_connectAudio(audioPreset, audioSettings);
#endif
        }

        private void PreLoadPatternList()
        {
            int ret;
            for (int i = 0; i < m_patternToPreload.Count; i++)
            {
                if (m_patternToPreload[i] == null)
                    continue;
                ret = m_skineticInstance.LoadPatternFromJSON(m_patternToPreload[i].Json);
                if (ret < 0)
                {
                    Debug.Log(getSDKError(ret));
                }
                else
                {
                    m_loadedPatternAssets.Add(m_patternToPreload[i]);
                    m_patternsIDs.Add(m_patternToPreload[i], ret);
                }
            }
        }
    }
}