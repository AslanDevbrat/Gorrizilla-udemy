using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Skinetic
{   
    public class SkineticWrapping : ISkinetic
    {
        [System.Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
        private struct CDeviceInfo
        {
            /** Available Output connection mode.*/
            public SkineticDevice.OutputType outputType;
            /** Device Serial Number.*/
            public System.UInt32 serialNumber;
            /** Device Type.*/
            public SkineticDevice.DeviceType deviceType;
            /** Device Version.*/
            [MarshalAs(UnmanagedType.LPStr)] public string deviceVersion;
            /** Pointer to next device.*/
            public IntPtr next;
        }

        [System.Serializable, System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 8)]
        private struct CAudioSettings
        {
            /** Name of the targeted audio device, default value "default" uses the OS default audio device.
             * If using a specific audioPreset other than eAudioDevice, the parameter will be ignored.*/
            [MarshalAs(UnmanagedType.LPStr)] public string deviceName;
            /** Name of the targeted API, default value "any_API" uses any available API.*/
            [MarshalAs(UnmanagedType.LPStr)] public string audioAPI;
            /** Sample rate of the audio stream. If using a specific audioPreset, the parameter will be ignored.*/
            public System.UInt32 sampleRate;
            /** Size of a chunk of data sent over the audio stream.*/
            public System.UInt32 bufferSize;
            /** Number of channels to use while streaming to the haptic output. If using a specific audio
             * preset, the parameter will be ignored. Setting -1 will use the number of actuator of the layout, or a portion of it.*/
            public int nbStreamChannel;
            /** Desired latency in seconds, default value "-1" sets it automatically. The value is rounded
             * to the closest available latency value from the audio API.*/
            public float suggestedLatency;

            public CAudioSettings(Experimental.AudioStreamConfiguration.AudioSettings audioSettings)
            {
                this.deviceName = audioSettings.deviceName;
                this.audioAPI = audioSettings.audioAPI;
                this.sampleRate = (System.UInt32)audioSettings.sampleRate;
                this.bufferSize = (System.UInt32)audioSettings.bufferSize;
                this.nbStreamChannel = audioSettings.nbStreamChannel;
                this.suggestedLatency = audioSettings.suggestedLatency;
            }
        }

        private const string DLLNAME = "SkineticSDK";

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void CallbackDelegate(SkineticDevice.ConnectionState status, int error, UInt32 serialNumber, IntPtr userData);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_serialNumberToString")]
        private static extern IntPtr Ski_serialNumberToString(UInt32 serialNumber);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_createSDKInstance")]
        private static extern int Ski_createSDKInstance([MarshalAs(UnmanagedType.LPStr)] string logFileName);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_freeSDKInstance")]
        private static extern void Ski_freeSDKInstance(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_scanDevices")]
        private static extern int Ski_scanDevices(int sdk_ID, SkineticDevice.OutputType outputType);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_scanStatus")]
        private static extern int Ski_scanStatus(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_getFirstScannedDevice")]
        private static extern IntPtr Ski_getFirstScannedDevice(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_connectDevice")]
        private static extern int Ski_connectDevice(int sdk_ID, SkineticDevice.OutputType outputType, UInt32 serialNumber);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_disconnectDevice")]
        private static extern int Ski_disconnectDevice(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_connectionStatus")]
        private static extern SkineticDevice.ConnectionState Ski_connectionStatus(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_setConnectionCallback")]
        private static extern int Ski_setConnectionCallback(int sdk_ID, IntPtr callback, IntPtr userData);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_getSDKVersion")]
        private static extern IntPtr Ski_getSDKVersion(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_getSkineticSerialNumber")]
        private static extern UInt32 Ski_getSkineticSerialNumber(int sdk_ID);
        
        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_getSkineticSerialNumberAsString")]
        private static extern IntPtr Ski_getSkineticSerialNumberAsString(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_getSkineticVersion")]
        private static extern IntPtr Ski_getSkineticVersion(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_getSkineticType")]
        private static extern SkineticDevice.DeviceType Ski_getSkineticType(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_getGlobalIntensityBoost")]
        private static extern int Ski_getGlobalIntensityBoost(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_setGlobalIntensityBoost")]
        private static extern int Ski_setGlobalIntensityBoost(int sdk_ID, int globalBoost);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_loadPatternFromJSON")]
        private static extern int Ski_loadPatternFromJSON(int sdk_ID, [MarshalAs(UnmanagedType.LPStr)] string json);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_unloadPattern")]
        private static extern int Ski_unloadPattern(int sdk_ID, int patternID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_getPatternIntensityBoost")]
        private static extern int Ski_getPatternIntensityBoost(int sdk_ID, int patternID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_setAccumulationWindowToPattern")]
        private static extern int Ski_setAccumulationWindowToPattern(int sdk_ID, int mainPatternID, int fallbackPatternID, float timeWindow, int maxAccumulation);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_eraseAccumulationWindowToPattern")]
        private static extern int Ski_eraseAccumulationWindowToPattern(int sdk_ID, int mainPatternID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_playEffect")]
        private static extern int Ski_playEffect(int sdk_ID, int patternID, SkineticDevice.EffectProperties effectProperties);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_stopEffect")]
        private static extern int Ski_stopEffect(int sdk_ID, int effectID, float time);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_effectState")]
        private static extern HapticEffect.State Ski_effectState(int sdk_ID, int effectI);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_pauseAll")]
        private static extern int Ski_pauseAll(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_resumeAll")]
        private static extern int Ski_resumeAll(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_stopAll")]
        private static extern int Ski_stopAll(int sdk_ID);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_exp_connectAudio")]
        private static extern int Ski_exp_connectAudio(int sdk_ID, Experimental.AudioStreamConfiguration.AudioPreset audioPreset, CAudioSettings audioSettings);
        
        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_exp_getOutputDevicesNames", CallingConvention = CallingConvention.Cdecl)]
        private static extern int Ski_exp_getOutputDevicesNames(ref IntPtr devicesNames, ref int nbDevices);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_exp_getOutputDeviceAPIs")]
        public static extern int Ski_exp_getOutputDeviceAPIs([MarshalAs(UnmanagedType.LPStr)] string outputName, ref IntPtr apis, ref int nbApis);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_exp_getOutputDeviceInfo")]
        public static extern int Ski_exp_getOutputDeviceInfo([MarshalAs(UnmanagedType.LPStr)] string outputName, [MarshalAs(UnmanagedType.LPStr)] string apiName, ref int max_channels, ref float default_low_latency, ref float default_high_latency);

        [global::System.Runtime.InteropServices.DllImport(DLLNAME, EntryPoint = "ski_exp_getSupportedStandardSampleRates")]
        public static extern int Ski_exp_getSupportedStandardSampleRates([MarshalAs(UnmanagedType.LPStr)] string outputName, [MarshalAs(UnmanagedType.LPStr)] string api, ref IntPtr sampleRates, ref int nbSampleRates);




        private int m_instanceID = -1;
        private CallbackDelegate m_callbackDelegate = ClassCallback;
        private SkineticDevice.ConnectionCallbackDelegate m_unityDelegate;
        private GCHandle m_handle;

        [AOT.MonoPInvokeCallback(typeof(CallbackDelegate))]
        static public void ClassCallback(SkineticDevice.ConnectionState status, int error, UInt32 serialNumber, IntPtr userData)
        {
            GCHandle obj = GCHandle.FromIntPtr(userData);
            ((SkineticWrapping)obj.Target).InstanceCallback(status, error, serialNumber);
        }

        private void InstanceCallback(SkineticDevice.ConnectionState status, int error, UInt32 serialNumber)
        {
            m_unityDelegate(status, error, serialNumber);
        }

		/// <summary>
        /// Convert Skinetic serial number to a formatted string.
        /// </summary>
        /// <param name="serialNumber">serial number to convert</param>
        /// <returns>string representation of the serial number</returns>
        public static string SerialNumberToString(System.UInt32 serialNumber)
        {
            IntPtr ptr = Ski_serialNumberToString(serialNumber);
            // assume returned string is utf-8 encoded
            return Marshal.PtrToStringAnsi(ptr);
        }

        /// <inheritdoc/>
        public void InitInstance()
        {
            if (m_instanceID != -1)
                return;
            m_instanceID = Ski_createSDKInstance("");
            m_handle = GCHandle.Alloc(this, GCHandleType.Normal);
        }

        /// <inheritdoc/>
        public void DeinitInstance()
        {
            if (m_instanceID == -1)
                return;
            Ski_freeSDKInstance(m_instanceID);
            m_instanceID = -1;
            m_handle.Free();
        }

        /// <inheritdoc/>
        public int ScanDevices(SkineticDevice.OutputType output)
        {
            return Ski_scanDevices(m_instanceID, output);
        }

        /// <inheritdoc/>
        public int ScanStatus()
        {
            return Ski_scanStatus(m_instanceID);
        }

        /// <inheritdoc/>
        public List<SkineticDevice.DeviceInfo> GetScannedDevices()
        {
            List<SkineticDevice.DeviceInfo> listDevices = new List<SkineticDevice.DeviceInfo>();
            IntPtr res = Ski_getFirstScannedDevice(m_instanceID);
            if(res == IntPtr.Zero)
            {
                return listDevices;
            }
            CDeviceInfo cdevice = Marshal.PtrToStructure<CDeviceInfo>(Ski_getFirstScannedDevice(m_instanceID));
            Skinetic.SkineticDevice.DeviceInfo device = new SkineticDevice.DeviceInfo();

            device.deviceType = cdevice.deviceType;
            device.deviceVersion = cdevice.deviceVersion;
            device.serialNumber = cdevice.serialNumber;
            device.outputType = cdevice.outputType;
            listDevices.Add(device);
            while (cdevice.next != IntPtr.Zero)
            {
                cdevice = Marshal.PtrToStructure<CDeviceInfo>(cdevice.next);
                device = new SkineticDevice.DeviceInfo();

                device.deviceType = cdevice.deviceType;
                device.deviceVersion = cdevice.deviceVersion;
                device.serialNumber = cdevice.serialNumber;
                device.outputType = cdevice.outputType;
                listDevices.Add(device);
            }
            return listDevices;
        }

        /// <inheritdoc/>
        public int Connect(SkineticDevice.OutputType output, System.UInt32 serialNumber)
        {
            return Ski_connectDevice(m_instanceID, output, serialNumber);
        }

        /// <inheritdoc/>
        public int Disconnect()
        {
            return Ski_disconnectDevice(m_instanceID);
        }

        /// <inheritdoc/>
        public SkineticDevice.ConnectionState ConnectionStatus()
        {
            return Ski_connectionStatus(m_instanceID);
        }

        /// <inheritdoc/>
        public int SetConnectionCallback(SkineticDevice.ConnectionCallbackDelegate callback)
        {
            int ret = Ski_setConnectionCallback(m_instanceID, Marshal.GetFunctionPointerForDelegate(m_callbackDelegate), GCHandle.ToIntPtr(m_handle));
            if (ret == 0)
                m_unityDelegate = callback;
            return ret;
        }

        /// <inheritdoc/>
        public String GetSDKVersion()
        {
            IntPtr ptr = Ski_getSDKVersion(m_instanceID);
            // assume returned string is utf-8 encoded
            return Marshal.PtrToStringAnsi(ptr);
        }

        /// <inheritdoc/>
        public String GetDeviceVersion()
        {
            IntPtr ptr = Ski_getSkineticVersion(m_instanceID);
            // assume returned string is utf-8 encoded
            return Marshal.PtrToStringAnsi(ptr);
        }

        /// <inheritdoc/>
        public System.UInt32 GetDeviceSerialNumber()
        {
            return Ski_getSkineticSerialNumber(m_instanceID);
        }

        /// <inheritdoc/>
        public String GetDeviceSerialNumberAsString()
        {
            IntPtr ptr = Ski_getSkineticSerialNumberAsString(m_instanceID);
            // assume returned string is utf-8 encoded
            return Marshal.PtrToStringAnsi(ptr);
        }

        /// <inheritdoc/>
        public SkineticDevice.DeviceType GetDeviceType()
        {
            return Ski_getSkineticType(m_instanceID);
        }

        /// <inheritdoc/>
        public int GetGlobalIntensityBoost()
        {
            return Ski_getGlobalIntensityBoost(m_instanceID);
        }

        /// <inheritdoc/>
        public int SetGlobalIntensityBoost(int globalBoost)
        {
            return Ski_setGlobalIntensityBoost(m_instanceID, globalBoost);
        }

        /// <inheritdoc/>
        public int LoadPatternFromJSON(String json)
        {
            return Ski_loadPatternFromJSON(m_instanceID, json);
        }

        /// <inheritdoc/>
        public int UnloadPattern(int patternID)
        {
            return Ski_unloadPattern(m_instanceID, patternID);
        }

        /// <inheritdoc/>
        public int GetPatternIntensityBoost(int patternID)
        {
            return Ski_getPatternIntensityBoost(m_instanceID, patternID);
        }

        /// <inheritdoc/>
        public int SetAccumulationWindowToPattern(int mainPatternID, int fallbackPatternID, float timeWindow, int maxAccumulation)
        {
            return Ski_setAccumulationWindowToPattern(m_instanceID, mainPatternID, fallbackPatternID, timeWindow, maxAccumulation);
        }

        /// <inheritdoc/>
        public int EraseAccumulationWindowToPattern(int mainPatternID)
        {
            return Ski_eraseAccumulationWindowToPattern(m_instanceID, mainPatternID);
        }

        /// <inheritdoc/>
        public int PlayEffect(int patternID, SkineticDevice.EffectProperties effectProperties)
        {
            return Ski_playEffect(m_instanceID, patternID, effectProperties);
        }

        /// <inheritdoc/>
        public int StopEffect(int effectID, float time)
        {
            return Ski_stopEffect(m_instanceID, effectID, time);
        }

        /// <inheritdoc/>
        public HapticEffect.State GetEffectState(int effectID)
        {
            return Ski_effectState(m_instanceID, effectID);
        }

        /// <inheritdoc/>
        public int PauseAll()
        {
            return Ski_pauseAll(m_instanceID);
        }

        /// <inheritdoc/>
        public int ResumeAll()
        {
            return Ski_resumeAll(m_instanceID);
        }

        /// <inheritdoc/>
        public int StopAll()
        {
            return Ski_stopAll(m_instanceID);
        }

        /// <summary>
        /// Initialize an asynchronous connection to an audio device using the
        /// provided settings.
        /// </summary>
        /// <remarks>
        /// If audioPreset is set to anything else other than
        /// AudioPreset::E_CUSTOMDEVICE, the provided settings are ignored and
        /// the ones corresponding to the preset are used instead.
        /// </remarks>
        /// <param name="audioPreset">preset of audio device.</param>
        /// <param name="audioSettings">stream settings</param>
        /// <returns>0 on success, an error otherwise.</returns>
        public int Exp_connectAudio(Experimental.AudioStreamConfiguration.AudioPreset audioPreset, Experimental.AudioStreamConfiguration.AudioSettings audioSettings)
        {
            CAudioSettings cAudioSettings = new CAudioSettings(audioSettings);
            return Ski_exp_connectAudio(m_instanceID, audioPreset, cAudioSettings);
        }

        /// <summary>
        /// Get names of available audio output devices.
        /// </summary>
        /// <remarks>
        /// If no device is available, the array will contain "noDevice".
        /// This allows to select which device to use if initializing the 
        /// context with eAudioDevice.
        /// </remarks>
        /// <returns>array of device names</returns>
        public static string[] Exp_GetOutputDevicesNames()
        {
            string[] output = new string[0];
            int nbOutput = 0;
            IntPtr arrPtr = IntPtr.Zero;
            int ret = Ski_exp_getOutputDevicesNames(ref arrPtr, ref nbOutput);
            if (ret < 0)
                Debug.Log(ret);
            if (arrPtr == IntPtr.Zero)
            {
                Debug.LogError("Invalid IntPtr: binaries might be corrupted, please reinstall package.");
                return output;
            }
            IntPtr[] managedArray = new IntPtr[nbOutput];
            Marshal.Copy(arrPtr, managedArray, 0, nbOutput);
            output = new string[nbOutput];
            for (int i = 0; i < nbOutput; i++)
            {
                output[i] = Marshal.PtrToStringAnsi(managedArray[i]);
            }
            return output;
        }

        /// <summary>
        /// Get available APIs for a given output device identified by name.
        /// </summary>
        /// <remarks>
        /// If no API is available, the array will contain "noAPI".
        /// </remarks>
        /// <param name="outputName">name of the output</param>
        /// <returns>array of api names</returns>
        public static string[] Exp_GetOutputDeviceAPIs(string outputName)
        {
            string[] apis = {"any_API"};
            int nbApis = 0;
            IntPtr arrPtr = IntPtr.Zero;
            int ret = Ski_exp_getOutputDeviceAPIs(outputName, ref arrPtr, ref nbApis);
            if (ret < 0)
                Debug.Log(ret);
            if (arrPtr == IntPtr.Zero)
            {
                Debug.LogError("Invalid IntPtr: binaries might be corrupted, please reinstall package.");
                return apis;
            }
            if(nbApis == 0)
            {
                apis[0] = "no_API";
                return apis;
            }
            IntPtr[] managedArray = new IntPtr[nbApis];
            Marshal.Copy(arrPtr, managedArray, 0, nbApis);
            apis = new string[nbApis + 1];
            apis[0] = "any_API";
            for (int i = 0; i < nbApis; i++)
            {
                apis[i+1] = Marshal.PtrToStringAnsi(managedArray[i]);
            }
            return apis;
        }

        /// <summary>
        /// Get settings extremum values of the output device identified by 
        /// name and API.
        /// </summary>
        /// <param name="outputName">name of the output</param>
        /// <param name="apiName">name of the API</param>
        /// <param name="maxChannels">max number of channel</param>
        /// <param name="defaultLowLatency">minimum latency of the output device</param>
        /// <param name="defaultHighLatency">maximum latency of the output device</param>
        /// <returns>0 if the values have been set successfully, an Error otherwise</returns>
        public static int Exp_GetOutputDeviceInfo(string outputName, string apiName, ref int maxChannels, ref float defaultLowLatency, ref float defaultHighLatency)
        {
            return Ski_exp_getOutputDeviceInfo(outputName, apiName, ref maxChannels, ref defaultLowLatency, ref defaultHighLatency);
        }

        /// <summary>
        /// Get all supported standard sample rates of the output device 
        /// identified by name and API.
        /// </summary>
        /// <remarks>
        /// If the outputName or the API are not valid, the 
        /// array is filled with all standard sample rates.
        /// </remarks>
        /// <param name="outputName">name of the output</param>
        /// <param name="apiName">name of the API</param>
        /// <returns>array of available framerates</returns>
        public static int[] Exp_GetSupportedStandardSampleRates(string outputName, string apiName)
        {
            int[] framerates = new int[0];
            int nbFramerates = 0;
            IntPtr arrPtr = IntPtr.Zero;
            int ret = Ski_exp_getSupportedStandardSampleRates(outputName, apiName, ref arrPtr, ref nbFramerates);
            if (arrPtr == IntPtr.Zero)
            {
                Debug.LogError("Invalid IntPtr: binaries might be corrupted, please reinstall package.");
                return framerates;
            }
            framerates = new int[nbFramerates];
            Marshal.Copy(arrPtr, framerates, 0, nbFramerates);
            return framerates;
        }
    }
}

