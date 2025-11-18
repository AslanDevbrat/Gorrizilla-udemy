using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Skinetic.Experimental
{
    /// <summary>
    /// Experimental component to configure and open an audio stream. 
    /// Feature is not available on Android.
    /// </summary>
    [AddComponentMenu("Skinetic/Experimental/Audio Settings")]
    public class AudioStreamConfiguration : MonoBehaviour
    {
        /// <summary>
        /// Preset of audio devices.
        /// </summary>
        /// <remarks> 
        /// E_CUSTOMDEVICE is to be used for a custom configuration.
        /// </summary>
        public enum AudioPreset
        {
            /// <summary>Audio stream with a custom configuration.</summary>
            E_CUSTOMDEVICE = 0,
            /// <summary>Autoconfiguration of the audioStream for the Skinetic device.</summary>
            E_SKINETIC = 1,
            /// <summary>Autoconfiguration of the audioStream for the HSD mk.I device.</summary>
            E_HSDMKI = 2,
            /// <summary>Autoconfiguration of the audioStream for the HSD mk.II device.</summary>
            E_HSDMKII = 3,
            /// <summary>Autoconfiguration of the audioStream for the HSD 0 device.</summary>
            E_HSD0 = 4,
        }

        /// <summary>Struct containing settings for an audio connection.</summary>
        [System.Serializable]
        public struct AudioSettings
        {
            /// <summary>Name of the targeted audio device, default value
            /// "default" uses the OS default audio device. If using a specific
            /// AudioSettings other than eCustomDevice, the parameter will be
            /// ignored.</summary>
            public string deviceName;
            /// <summary>Name of the targeted API. Default value "any_API" uses
            /// any available API which match the configuration, if any.
            /// If using a specific AudioSettings other than E_CUSTOMDEVICE,
            /// the parameter will be ignored.</summary>
            public string audioAPI;
            /// <summary>Sample rate of the audio stream. If using a specific
            /// AudioSettings, the parameter will be ignored.</summary>
            public int sampleRate;
            /// <summary>Size (strictly positive) of a chunk of data sent over 
            /// the audio stream. This parameter MUST be set independently
            /// of the used AudioSettings.</summary>
            public int bufferSize;
            /// <summary>Number of channels (strictly positive) to use while 
            /// streaming to the haptic output. If using a specific AudioSettings, 
            /// the parameter will be ignored. Setting -1 will use the 
            /// number of actuator of the layout, or a portion of it.</summary>
            public int nbStreamChannel;
            /// <summary>Desired latency in seconds. The value is rounded to the
            /// closest available latency value from the audio API. If using a
            /// specific AudioSettings other than eCustomDevice, the parameter
            /// will be ignored.</summary>
            public float suggestedLatency;

            public AudioSettings(string deviceName, string audioAPI, int sampleRate, int bufferSize, int nbStreamChannel, float suggestedLatency)
            {
                this.deviceName = deviceName;
                this.audioAPI = audioAPI;
                this.sampleRate = sampleRate;
                this.bufferSize = bufferSize;
                this.nbStreamChannel = nbStreamChannel;
                this.suggestedLatency = suggestedLatency;
            }
        }        

        [SerializeField]
        private AudioSettings m_audioSettings = new AudioSettings("noDevice", "noAPI", 48000, 128, 2, 0);
        [SerializeField]
        private AudioPreset m_audioPreset = AudioPreset.E_SKINETIC;

        
        /// <summary>
        /// Selected preset of audio device.
        /// </summary>
        public AudioPreset DeviceAudioPreset { get => m_audioPreset; set => m_audioPreset = value; }

        /// <summary>
        /// Name of the targeted audio device.
        /// </summary>
        /// <remarks>
        /// If using a specific audioPreset other than eAudioDevice, the parameter will be ignored.
        /// </remarks>
        public string DeviceName { get => m_audioSettings.deviceName; set => m_audioSettings.deviceName = value; }

        /// <summary>
        /// Name of the targeted API.
        /// </summary>
        /// <remarks>
        /// Default value "any_API" uses any available API which match the configuration, if any.
        /// </remarks>
        public string AudioAPI { get => m_audioSettings.audioAPI; set => m_audioSettings.audioAPI = value; }

        /// <summary>
        /// Sample rate of the audio stream.
        /// </summary>
        /// <remarks>
        /// If using a specific audioPreset, the parameter will be ignored.
        /// </remarks>
        public int SampleRate { get => (int)m_audioSettings.sampleRate; set => m_audioSettings.sampleRate = Mathf.Max(0, value); }

        /// <summary>
        /// Size of a chunk of data sent over the audio stream.
        /// </summary>
        public int BufferSize { get => (int)m_audioSettings.bufferSize; set => m_audioSettings.bufferSize = Mathf.Max(0, value); }

        /// <summary>
        /// Number of channels to use while streaming to the haptic output. 
        /// </summary>
        /// <remarks>
        /// If using a specific audio preset, the parameter will be ignored.Setting -1 will use the number of actuator of the layout, or a portion of it.
        /// </remarks>
        public int NbStreamChannel { get => m_audioSettings.nbStreamChannel; set => m_audioSettings.nbStreamChannel = Mathf.Max(-1, value); }

        /// <summary>
        /// Desired latency in seconds. 
        /// </summary>
        /// <remarks>
        /// The value is rounded to the closest available latency value from the audio API.
        /// </remarks>
        public float SuggestedLatency { get => m_audioSettings.suggestedLatency; set => m_audioSettings.suggestedLatency = Mathf.Max(0, value); }

        /// <summary>
        /// Get names of available audio output devices.
        /// </summary>
        /// <remarks>
        /// If no device is available, the array will contain "noDevice".
        /// This allows to select which device to use if initializing the 
        /// context with eAudioDevice.
        /// Not Available on Android.
        /// </remarks>
        /// <returns>array of device names</returns>
        static public string[] GetOutputDevicesNames()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new string[0];
#else
            return SkineticWrapping.Exp_GetOutputDevicesNames();
#endif
        }


        /// <summary>
        /// Get available APIs for a given output device identified by name.
        /// </summary>
        /// <remarks>
        /// If no API is available, the array will contain "noAPI". 
        /// Not Available on Android.
        /// </remarks>
        /// <param name="outputName">name of the output</param>
        /// <returns>array of api names</returns>
        static public string[] GetOutputDeviceAPIs(string outputName)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new string[0];
#else
            return SkineticWrapping.Exp_GetOutputDeviceAPIs(outputName);
#endif
        }

        /// <summary>
        /// Get settings extremum values of the output device identified by 
        /// name and API.
        /// </summary>
        /// <remarks>
        /// Not Available on Android.
        /// </remarks>
        /// <param name="outputName">name of the output</param>
        /// <param name="apiName">name of the API</param>
        /// <param name="maxChannels">max number of channel</param>
        /// <param name="defaultLowLatency">minimum latency of the output device</param>
        /// <param name="defaultHighLatency">maximum latency of the output device</param>
        /// <returns>0 if the values have been set successfully, an Error otherwise</returns>
        static public int GetOutputDeviceInfo(string outputName, string apiName, ref int maxChannels, ref float defaultLowLatency, ref float defaultHighLatency)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            maxChannels = 0;
            defaultLowLatency = 0;
            defaultHighLatency = 0;
            return 0;
#else
            return SkineticWrapping.Exp_GetOutputDeviceInfo(outputName, apiName, ref maxChannels, ref defaultLowLatency, ref defaultHighLatency);
#endif
        }

        /// <summary>
        /// Get all supported standard sample rates of the output device 
        /// identified by name and API.
        /// </summary>
        /// <remarks>
        /// If the outputName or the API are not valid, an error is returned and the 
        /// array is filled with all standard sample rates. 
        /// Not Available on Android.
        /// </remarks>
        /// <param name="outputName">name of the output</param>
        /// <param name="apiName">name of the API</param>
        /// <returns>array of available framerates</returns>
        static public int[] GetSupportedStandardSampleRates(string outputName, string apiName)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new int[0];
#else
            return SkineticWrapping.Exp_GetSupportedStandardSampleRates(outputName, apiName);
#endif
        }

        /// <summary>
        /// Initialize an asynchronous connection to an audio device using the
        /// provided settings.
        /// </summary>
        /// <remarks>
        /// This method substitute the connect() methode of the SkineticDevice component.
        /// If audioPreset is set to anything else other than
        /// AudioPreset::E_CUSTOMDEVICE, the provided settings are ignored and
        /// the ones corresponding to the preset are used instead.
        /// Not available for Android.
        /// </summary>
        /// <param name="device">target device to connect through audio.</param>
        /// <returns>0 on success, an error otherwise.</returns>
        public int ConnectAudio(SkineticDevice device)
        {
            return device.ExpConnectAudio(m_audioPreset, m_audioSettings);
        }

    }
}

