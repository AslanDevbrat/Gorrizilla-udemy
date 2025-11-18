using AOT;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Skinetic
{
    public class SkineticAndroid : ISkinetic
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void CallbackDelegate(SkineticDevice.ConnectionState status, int error, UInt32 serialNumber, IntPtr userData);

        private CallbackDelegate m_callbackDelegate = ClassCallback;
        private SkineticDevice.ConnectionCallbackDelegate m_unityDelegate;
        private GCHandle m_handle;
        private IntPtr m_structPtr;

        private bool m_init = false;

        private static AndroidJavaClass enumOutputTypeJavaClass;
        private static AndroidJavaClass enumDeviceTypeJavaClass;
        private static AndroidJavaClass unityPlayer;

        private AndroidJavaObject m_activity;
        private AndroidJavaObject m_context;
        private AndroidJavaObject m_skineticObj;

        [AOT.MonoPInvokeCallback(typeof(CallbackDelegate))]
        static public void ClassCallback(SkineticDevice.ConnectionState status, int error, UInt32 serialNumber, IntPtr userData)
        {
            GCHandle obj = GCHandle.FromIntPtr(userData);
            ((SkineticAndroid)obj.Target).InstanceCallback(status, error, serialNumber);
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
            AndroidJavaClass skineticSDKJavaClass = new AndroidJavaClass("com.actronika.skineticsdk.SkineticSDK");
            return skineticSDKJavaClass.CallStatic<String>("serialNumberToString", (Int64)serialNumber);
        }
		
        /// <inheritdoc/>
        public void InitInstance()
        {
            if (m_init)
                return;
            Debug.Log("Get java variables");
            enumOutputTypeJavaClass = new AndroidJavaClass("com.actronika.skineticsdk.SkineticSDK$OUTPUT_TYPE");
            enumDeviceTypeJavaClass = new AndroidJavaClass("com.actronika.skineticsdk.SkineticSDK$DEVICE_TYPE");
            unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

            m_activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            m_context = m_activity.Call<AndroidJavaObject>("getApplicationContext");
            m_skineticObj = new AndroidJavaObject("com.actronika.skineticsdk.SkineticSDK", m_context, m_activity);

            Debug.Log("Allocate delegate handle");
            m_handle = GCHandle.Alloc(this, GCHandleType.Normal);
            Debug.Log("Allocate delegate handle done");

            SkineticDevice.EffectProperties test = new SkineticDevice.EffectProperties();
            m_structPtr = Marshal.AllocHGlobal(Marshal.SizeOf(test));
        }

        /// <inheritdoc/>
        public void DeinitInstance()
        {
            if (!m_init)
                return;
            m_activity.Dispose();
            m_context.Dispose();
            m_skineticObj.Dispose();

            m_handle.Free();
            Marshal.FreeHGlobal(m_structPtr);
        }

        /// <inheritdoc/>
        public int ScanDevices(SkineticDevice.OutputType output)
        {
            AndroidJavaObject enumObj = enumOutputTypeJavaClass.GetStatic<AndroidJavaObject>(output.ToString());
            return m_skineticObj.Call<int>("scanDevices", enumObj);
        }

        /// <inheritdoc/>
        public int ScanStatus()
        {
            return m_skineticObj.Call<int>("scanStatus");
        }

        /// <inheritdoc/>
        public List<SkineticDevice.DeviceInfo> GetScannedDevices()
        {
            AndroidJavaObject jlistDevices = m_skineticObj.Call<AndroidJavaObject>("getScannedDevices");
            int count = jlistDevices.Call<int>("size");
            List<SkineticDevice.DeviceInfo> listDevices = new List<SkineticDevice.DeviceInfo>();

            for (int i = 0; i < count; i++)
            {
                SkineticDevice.DeviceInfo device = new SkineticDevice.DeviceInfo();
                AndroidJavaObject jdeviceInfo = jlistDevices.Call<AndroidJavaObject>("get", i);

                device.serialNumber = (UInt32)jdeviceInfo.Get<long>("serialNumber");

                AndroidJavaObject enumObj = jdeviceInfo.Get<AndroidJavaObject>("outputType");
                device.outputType = (SkineticDevice.OutputType)enumObj.Call<int>("getValue");

                enumObj = jdeviceInfo.Get<AndroidJavaObject>("deviceType");
                device.deviceType = (SkineticDevice.DeviceType)enumObj.Call<int>("getValue");

                device.deviceVersion = jdeviceInfo.Get<String>("deviceVersion");
                listDevices.Add(device);
            }
            return listDevices;
        }

        /// <inheritdoc/>
        public int Connect(SkineticDevice.OutputType output, System.UInt32 serialNumber)
        {
            AndroidJavaObject enumObj = enumOutputTypeJavaClass.GetStatic<AndroidJavaObject>(output.ToString());       
            return m_skineticObj.Call<int>("connect", enumObj, (Int64)serialNumber);
        }

        /// <inheritdoc/>
        public int Disconnect()
        {
            return m_skineticObj.Call<int>("disconnect");
        }

        /// <inheritdoc/>
        public SkineticDevice.ConnectionState ConnectionStatus()
        {
            AndroidJavaObject enumObj = m_skineticObj.Call<AndroidJavaObject>("connectionStatus");
            return (SkineticDevice.ConnectionState)enumObj.Call<int>("getValue");
        }

        /// <inheritdoc/>
        public int SetConnectionCallback(SkineticDevice.ConnectionCallbackDelegate callback)
        {
            int ret = m_skineticObj.Call<int>("setConnectionCallback", (Int64)Marshal.GetFunctionPointerForDelegate(m_callbackDelegate), (Int64)GCHandle.ToIntPtr(m_handle));
            if (ret == 0)
                m_unityDelegate = callback;
            return ret;
        }

        /// <inheritdoc/>
        public String GetSDKVersion()
        {
            return m_skineticObj.Call<String>("getSDKVersion");
        }


        /// <inheritdoc/>
        public String GetDeviceVersion()
        {
            return m_skineticObj.Call<String>("getDeviceVersion");
        }

        /// <inheritdoc/>
        public System.UInt32 GetDeviceSerialNumber()
        {
            return BitConverter.ToUInt32(BitConverter.GetBytes(m_skineticObj.Call<long>("getDeviceSerialNumber")), 0);
        }

        /// <inheritdoc/>
        public String GetDeviceSerialNumberAsString()
        {
            return m_skineticObj.Call<String>("getDeviceSerialNumberAsString");
        }

        /// <inheritdoc/>
        public SkineticDevice.DeviceType GetDeviceType()
        {
            AndroidJavaObject enumObj = m_skineticObj.Call<AndroidJavaObject>("getDeviceType");
            return (SkineticDevice.DeviceType)enumObj.Call<int>("getValue");
        }

        /// <inheritdoc/>
        public int GetGlobalIntensityBoost()
        {
            return m_skineticObj.Call<int>("getGlobalIntensityBoost");
        }

        /// <inheritdoc/>
        public int SetGlobalIntensityBoost(int globalBoost)
        {
            return m_skineticObj.Call<int>("setGlobalIntensityBoost", globalBoost);
        }

        /// <inheritdoc/>
        public int LoadPatternFromJSON(String json)
        {
            return m_skineticObj.Call<int>("loadPatternFromJSON", json);
        }

        /// <inheritdoc/>
        public int UnloadPattern(int patternID)
        {
            return m_skineticObj.Call<int>("unloadPattern", patternID);
        }

        /// <inheritdoc/>
        public int GetPatternIntensityBoost(int patternID)
        {
            return m_skineticObj.Call<int>("getPatternIntensityBoost", patternID);
        }

        /// <inheritdoc/>
        public int SetAccumulationWindowToPattern(int mainPatternID, int fallbackPatternID, float timeWindow, int maxAccumulation)
        {
            return m_skineticObj.Call<int>("setAccumulationWindowToPattern", mainPatternID, fallbackPatternID, timeWindow, maxAccumulation);
        }

        /// <inheritdoc/>
        public int EraseAccumulationWindowToPattern(int mainPatternID)
        {
            return m_skineticObj.Call<int>("eraseAccumulationWindowToPattern", mainPatternID);
        }

        /// <inheritdoc/>
        public int PlayEffect(int patternID, SkineticDevice.EffectProperties effectProperies)
        {
            Marshal.StructureToPtr(effectProperies, m_structPtr, false);
            return m_skineticObj.Call<int>("playEffectByPtr", patternID, (Int64)m_structPtr);
        }

        /// <inheritdoc/>
        public int StopEffect(int effectID, float time)
        {
            return m_skineticObj.Call<int>("stopEffect", effectID, time);
        }

        /// <inheritdoc/>
        public HapticEffect.State GetEffectState(int effectID)
        {
            AndroidJavaObject enumObj = m_skineticObj.Call<AndroidJavaObject>("effectState", effectID);
            return (HapticEffect.State)enumObj.Call<int>("getValue");
        }

        /// <inheritdoc/>
        public int PauseAll()
        {
            return m_skineticObj.Call<int>("pauseAll");
        }

        /// <inheritdoc/>
        public int ResumeAll()
        {
            return m_skineticObj.Call<int>("resumeAll");
        }

        /// <inheritdoc/>
        public int StopAll()
        {
            return m_skineticObj.Call<int>("stopAll");
        }
    }
}