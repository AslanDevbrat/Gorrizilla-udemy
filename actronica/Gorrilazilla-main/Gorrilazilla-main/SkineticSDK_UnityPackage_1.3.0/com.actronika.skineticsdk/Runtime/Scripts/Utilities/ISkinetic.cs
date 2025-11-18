using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Skinetic
{
    public interface ISkinetic
    {
        /// <summary>
        /// Initialize the Skinetic device instance.
        /// </summary>
        void InitInstance();

        /// <summary>
        /// Deinitialize the Skinetic device instance.
        /// </summary>
        void DeinitInstance();

        /// <summary>
        /// Initialize a scanning routine to find all available Skinetic device.
        /// </summary>
        /// <remarks>
        /// The state of the routine can be obtain from ScanStatus(). Once completed,
        /// the result can be accessed using GetScannedDevices().
        /// </remarks>
        /// <param name="output"></param>
        /// <returns>0 on success, an Error code on failure.</returns>
        int ScanDevices(SkineticDevice.OutputType output);

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
        int ScanStatus();

        /// <summary>
        /// This function returns a list of all DeviceInfo of each Skinetic devices found during the scan
        /// which match the specified output type.
        /// </summary>
        /// <returns>a list of DeviceInfo, empty if no match or an error occurs.</returns>
        List<SkineticDevice.DeviceInfo> GetScannedDevices();

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
        int Connect(SkineticDevice.OutputType output, System.UInt32 serialNumber);

        /// <summary>
        /// Disconnect the current Skinetic device.
        /// </summary>
        /// <remarks>
        /// The disconnection is effective once all resources are released.
        /// The state of the routine can be obtain from ConnectionStatus().
        /// </remarks>
        /// <returns>0 on success, an error otherwise.</returns>
        int Disconnect();

        /// <summary>
        /// Check the current status of the connection.
        /// The asynchronous connection routine is terminated on failure.
        /// </summary>
        /// <returns>the current status of the connection.</returns>
        SkineticDevice.ConnectionState ConnectionStatus();

        /// <summary>
        /// Set a callback function fired upon connection changes.
        /// </summary>
        /// <remarks>
        /// Functions of type ski_ConnectionCallback are implemented by clients.
        /// 
        /// The callback is fired at the end of the connection routine weither it succeed
        /// or failed.It is also fired if a connection issue arise.
        /// The callback is not fired if none was passed to setConnectionCallback().
        /// 
        /// 'userData' is a client supplied pointer which is passed back when the callback
        /// function is called.It could for example, contain a pointer to an class instance
        /// that will process the callback.
        /// </remarks>
        /// <param name="callback">client's callback</param>
        /// <returns>0 on success, an Error code on failure.</returns>
        int SetConnectionCallback(SkineticDevice.ConnectionCallbackDelegate callback);

        /// <summary>
        /// Get SDK version as a string.
        /// </summary>
        /// <remarks>
        /// The format of the string is: <pre>major.minor.revision</pre>
        /// </remarks>
        /// <returns>The version string.</returns>
        string GetSDKVersion();

        /// <summary>
        /// Get the connected device's version as a string.
        /// </summary>
        /// <remarks>
        /// The format of the string is: <pre>major.minor.revision</pre>
        /// </remarks>
        /// <returns>The version string if a Skinetic device is connected, 
        /// an error message otherwise.</returns>
        string GetDeviceVersion();

        /// <summary>
        /// Get the connected device's serial number.
        /// </summary>
        /// <returns>The serial number of the connected Skinetic 
        /// device if any, 0xFFFFFFFF otherwise.</returns>
        System.UInt32 GetDeviceSerialNumber();

        /// <summary>
        /// Get the connected device's serial number as string.
        /// </summary>
        /// <returns>The serial number as string of the connected Skinetic device 
        /// if any, "noDeviceConnected" otherwise.</returns>
        string GetDeviceSerialNumberAsString();

        /// <summary>
        /// Get the connected device's type.
        /// </summary>
        /// <returns>The type of the connected Skinetic device if it is connected,
        /// an ERROR message otherwise.</returns>
        SkineticDevice.DeviceType GetDeviceType();

        /// <summary>
        /// Get the amount of effect's intensity boost.
        /// </summary>
        /// <remarks>
        /// The boost increase the overall intensity of all haptic effects.
        /// However, the higher the boost activation is, the more the haptic effects are degraded.
        /// The global boost is meant to be set by the user as an application setting.
        /// </remarks>
        /// <returns>The percentage of effect's intensity boost, an ERROR otherwise.</returns>
        int GetGlobalIntensityBoost();

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
        int SetGlobalIntensityBoost(int globalBoost);

        /// <summary>
        /// Load a pattern from a valid json into a local haptic asset and return
        /// the corresponding patternID.
        /// </summary>
        /// <remarks>
        /// The patternID is a positive index.
        /// </remarks>
        /// <param name="json">describing the pattern</param>
        /// <returns>Positive patternID on success, an error otherwise.</returns>
        int LoadPatternFromJSON(string json);

        /// <summary>
        /// Unload the pattern from of the corresponding patternID.
        /// </summary>
        /// <param name="patternID">the patternID of the pattern to unload.</param>
        /// <returns>0 on success, an error otherwise.</returns>
        int UnloadPattern(int patternID);

        /// <summary>
        ///Get the pattern boost value which serves as a default value for the playing effect. 
        /// </summary>
        /// <remarks>
        ///The value is ranged in [-100; 100]. 
        ///If the pattern ID is invalid, zero is still returned.
        /// </remarks>
        /// <param name="patternID">the ID of the targeted pattern.</param>
        /// <returns>the pattern intensity boost of the pattern if it exists, 0 otherwise.</returns>
        int GetPatternIntensityBoost(int patternID);

        /// <summary>
        /// Enable the effect accumulation strategy on a targeted pattern.
        /// </summary>
        /// <remarks>
        /// Whenever an effect is triggered on the main pattern,
        /// the fallback one is used instead, if the main is already playing. More details can be found the
        /// additional documentation.
        /// For the maxAccumulation, setting to 0 removes the limit.
        ///
        /// If a new call to this function is done for a specific pattern, the previous association is overridden.
        /// </remarks>
        /// <param name="mainPatternID">the patternID of the main pattern.</param>
        /// <param name="fallbackPatternID">the patternID of the fallback pattern</param>
        /// <param name="timeWindow">the time window during which the accumulation should happen.</param>
        /// <param name="maxAccumulation">max number of extra accumulated effect instances.</param>
        /// <returns>0 on success, an ERROR otherwise.</returns>
        int SetAccumulationWindowToPattern(int mainPatternID, int fallbackPatternID, float timeWindow, int maxAccumulation);

        /// <summary>
        /// Disable the effect accumulation strategy on a specific pattern if any set.
        /// </summary>
        /// <param name="mainPatternID">the patternID of the main pattern.</param>
        /// <returns>0 on success, an ERROR otherwise.</returns>
        int EraseAccumulationWindowToPattern(int mainPatternID);

        /// <summary>
        /// Play an haptic effect based on a loaded pattern and return the effectID of this instance.
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
        /// <param name="patternID">pattern used by the effect instance.</param>
        /// <param name="effectProperties">struct to specialized the effect.</param>
        /// <returns>Positive effectID on success, an error otherwise.</returns>
        int PlayEffect(int patternID, SkineticDevice.EffectProperties effectProperies);

        /// <summary>
        /// Stop the effect instance identified by its effectID.
        /// </summary>
        /// <remarks>
        /// The effect is stop in "time" seconds with a fade out to prevent abrupt transition. If time
        /// is set to 0, no fadeout are applied and the effect is stopped as soon as possible.
        /// Once an effect is stopped, it is instance is destroyed and its effectID invalidated.
        /// </remarks>
        /// <param name="effectID">index identifying the effect.</param>
        /// <param name="time">duration of the fadeout in second.</param>
        /// <returns>0 on success, an error otherwise.</returns>
        int StopEffect(int effectID, float time);

        /// <summary>
        ///  Get the current state of an effect.
        /// </summary>
        /// <remarks>
        /// If the effectID is invalid, the 'stop' state will be return.
        /// </remarks>
        /// <param name="effectID">index identifying the effect.</param>
        /// <returns>the current state of the effect.</returns>
        HapticEffect.State GetEffectState(int effectID);

        /// <summary>
        /// Pause all haptic effect that are currently playing.
        /// </summary>
        /// <returns>0 on success, an error otherwise.</returns>
        int PauseAll();

        /// <summary>
        /// Resume the paused haptic effects.
        /// </summary>
        /// <returns>0 on success, an error otherwise.</returns>
        int ResumeAll();

        /// <summary>
        /// Stop all playing haptic effect.
        /// </summary>
        /// <returns>0 on success, an error otherwise.</returns>
        int StopAll();
    }
}
