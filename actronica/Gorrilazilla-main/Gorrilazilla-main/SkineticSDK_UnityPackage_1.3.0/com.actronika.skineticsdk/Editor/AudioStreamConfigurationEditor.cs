using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Skinetic;
using System;
using System.Linq;

[CustomEditor(typeof(Skinetic.Experimental.AudioStreamConfiguration))]
public class AudioStreamConfigurationEditor : Editor
{
    private Skinetic.Experimental.AudioStreamConfiguration m_targetAudioSettings;
    private int AudioDeviceIndex;
    private int IndexAPI;
    private int IndexFrameRate;
    private int MAXChannels;
    private float DefaultLowLatency;
    private float DefaultHighLatency;
    private int[] AvailableFrameRates;
    private string[] AvailableFrameRatesAsString;
    private string[] AudioDeviceNames;
    private string[] ApisNames;
    private bool m_isEditable;

    private Skinetic.Experimental.AudioStreamConfiguration.AudioSettings m_tempAudioSettings;
    private Skinetic.Experimental.AudioStreamConfiguration.AudioPreset m_tempPreset;


    public void DisplayAudioSettings(Skinetic.Experimental.AudioStreamConfiguration settings)
    {
        EditorGUIUtility.labelWidth = 110f;
        EditorGUIUtility.fieldWidth = 60f;
        EditorGUILayout.Space(5f, false);

        if (!m_isEditable)
            DisplayCurrentAudioSettings(m_targetAudioSettings);
        else
        {
            EditAudioSettings();
        }
    }

    private void DisplayCurrentAudioSettings(Skinetic.Experimental.AudioStreamConfiguration settings)
    {
        EditorGUIUtility.labelWidth = 200f;
        EditorGUIUtility.fieldWidth = 60f;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(new GUIContent("AudioPreset :", PropertiesUtils.GetPropertyTooltip("AudioPreset")), GUILayout.MaxWidth(130));
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField(new GUIContent(m_targetAudioSettings.DeviceAudioPreset.ToString(), PropertiesUtils.GetPropertyTooltip(m_targetAudioSettings.DeviceAudioPreset.ToString())));
        EditorGUILayout.EndHorizontal();

        if (m_tempPreset == Skinetic.Experimental.AudioStreamConfiguration.AudioPreset.E_CUSTOMDEVICE)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("DeviceName :", PropertiesUtils.GetPropertyTooltip("DeviceName")), GUILayout.MaxWidth(130));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent(m_targetAudioSettings.DeviceName.ToString()));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("AudioAPI :", PropertiesUtils.GetPropertyTooltip("AudioAPI")), GUILayout.MaxWidth(130));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent(m_targetAudioSettings.AudioAPI.ToString()));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("NbStreamChannel :", PropertiesUtils.GetPropertyTooltip("NbStreamChannel")), GUILayout.MaxWidth(130));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent(m_targetAudioSettings.NbStreamChannel.ToString() + " in [1; ∞[ or auto {-1}"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("BufferSize : ", PropertiesUtils.GetPropertyTooltip("BufferSize")), GUILayout.MaxWidth(130));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent(m_targetAudioSettings.BufferSize.ToString()));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("SampleRate :", PropertiesUtils.GetPropertyTooltip("SampleRate")), GUILayout.MaxWidth(130));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent(m_targetAudioSettings.SampleRate.ToString() + " Hz"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("SuggestedLatency :", PropertiesUtils.GetPropertyTooltip("SuggestedLatency")), GUILayout.MaxWidth(130));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent(m_targetAudioSettings.SuggestedLatency.ToString() + " s"));
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("BufferSize : ", PropertiesUtils.GetPropertyTooltip("BufferSize")), GUILayout.MaxWidth(130));
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(new GUIContent(m_targetAudioSettings.BufferSize.ToString()));
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(7f, false);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Edit"))
        {
            m_tempPreset = m_targetAudioSettings.DeviceAudioPreset;
            m_tempAudioSettings = new Skinetic.Experimental.AudioStreamConfiguration.AudioSettings(m_targetAudioSettings.DeviceName,
                                                                                                   m_targetAudioSettings.AudioAPI,
                                                                                                   m_targetAudioSettings.SampleRate,
                                                                                                   m_targetAudioSettings.BufferSize,
                                                                                                   m_targetAudioSettings.NbStreamChannel,
                                                                                                   m_targetAudioSettings.SuggestedLatency);
            ReinitCachedData();
            m_isEditable = true;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void EditAudioSettings()
    {
        m_tempPreset = (Skinetic.Experimental.AudioStreamConfiguration.AudioPreset)EditorGUILayout.EnumPopup(new GUIContent("AudioPreset", PropertiesUtils.GetPropertyTooltip("AudioPreset")), m_tempPreset);
        EditorGUILayout.HelpBox(PropertiesUtils.GetPropertyTooltip(m_tempPreset.ToString()), MessageType.Info);

        if (m_tempPreset == Skinetic.Experimental.AudioStreamConfiguration.AudioPreset.E_CUSTOMDEVICE)
        {
            AudioDeviceNames = Skinetic.Experimental.AudioStreamConfiguration.GetOutputDevicesNames();
            AudioDeviceIndex = Array.IndexOf(AudioDeviceNames, m_tempAudioSettings.deviceName);
            if (AudioDeviceIndex < 0)
            {
                AudioDeviceIndex = 0;
                m_tempAudioSettings.deviceName = AudioDeviceNames[0];
            }

            int dropdownSelectedIndex = EditorGUILayout.Popup(new GUIContent("DeviceName", PropertiesUtils.GetPropertyTooltip("DeviceName")), AudioDeviceIndex, AudioDeviceNames);

            if (dropdownSelectedIndex != AudioDeviceIndex)
            {
                AudioDeviceIndex = dropdownSelectedIndex;
                m_tempAudioSettings.deviceName = AudioDeviceNames[dropdownSelectedIndex];
                IndexAPI = 0;
                ApisNames = Skinetic.Experimental.AudioStreamConfiguration.GetOutputDeviceAPIs(m_tempAudioSettings.deviceName);
                m_tempAudioSettings.audioAPI = ApisNames[IndexAPI];
            }

            IndexAPI = Array.IndexOf(ApisNames, m_tempAudioSettings.audioAPI);
            if (IndexAPI < 0)
            {
                IndexAPI = 0;
                m_tempAudioSettings.audioAPI = ApisNames[0];
            }

            dropdownSelectedIndex = EditorGUILayout.Popup(new GUIContent("AudioAPI", PropertiesUtils.GetPropertyTooltip("AudioAPI")), IndexAPI, ApisNames);
            if (dropdownSelectedIndex != IndexAPI)
            {
                IndexAPI = dropdownSelectedIndex;
                m_tempAudioSettings.audioAPI = ApisNames[IndexAPI];
                if (IndexAPI >= 0)
                {
                    IndexFrameRate = 0;
                    Skinetic.Experimental.AudioStreamConfiguration.GetOutputDeviceInfo(m_tempAudioSettings.deviceName, m_tempAudioSettings.audioAPI, ref MAXChannels, ref DefaultLowLatency, ref DefaultHighLatency);

                    m_tempAudioSettings.nbStreamChannel = -1;
                    m_tempAudioSettings.suggestedLatency = DefaultLowLatency;
                    AvailableFrameRates = Skinetic.Experimental.AudioStreamConfiguration.GetSupportedStandardSampleRates(m_tempAudioSettings.deviceName, m_tempAudioSettings.audioAPI);
                    AvailableFrameRatesAsString = AvailableFrameRates.Select(i => i.ToString()).ToArray();
                    m_tempAudioSettings.sampleRate = AvailableFrameRates[0];
                }
            }

            if (IndexAPI == 0)
            {
                EditorGUILayout.BeginHorizontal();
                m_tempAudioSettings.nbStreamChannel = EditorGUILayout.IntField(new GUIContent("NbStreamChannel", PropertiesUtils.GetPropertyTooltip("NbStreamChannel")), 
                    Mathf.Max(m_tempAudioSettings.nbStreamChannel, -1));
                if (m_tempAudioSettings.nbStreamChannel == 0) m_tempAudioSettings.nbStreamChannel = -1;
                EditorGUILayout.LabelField("[1; ∞[ or auto {-1}");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                m_tempAudioSettings.bufferSize = EditorGUILayout.IntField(new GUIContent("BufferSize", PropertiesUtils.GetPropertyTooltip("BufferSize")), 
                    Mathf.Max(m_tempAudioSettings.bufferSize, 0));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                IndexFrameRate = Array.IndexOf(AvailableFrameRates, m_tempAudioSettings.sampleRate);
                if (IndexFrameRate < 0)
                {
                    IndexFrameRate = 0;
                    m_tempAudioSettings.sampleRate = AvailableFrameRates[0];
                }
                EditorGUILayout.BeginHorizontal();
                int idx = EditorGUILayout.Popup(new GUIContent("SampleRate", PropertiesUtils.GetPropertyTooltip("SampleRate")), IndexFrameRate, AvailableFrameRatesAsString);
                EditorGUILayout.LabelField("Hz");
                EditorGUILayout.EndHorizontal();
                if (IndexFrameRate != idx)
                {
                    IndexFrameRate = idx;
                    m_tempAudioSettings.sampleRate = AvailableFrameRates[IndexFrameRate];
                }

                EditorGUILayout.BeginHorizontal();
                m_tempAudioSettings.suggestedLatency = EditorGUILayout.FloatField(new GUIContent("SuggestedLatency", PropertiesUtils.GetPropertyTooltip("SuggestedLatency")),
                    Mathf.Max(0, m_tempAudioSettings.suggestedLatency));
                EditorGUILayout.LabelField("[0; ∞[ s");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                m_tempAudioSettings.nbStreamChannel = EditorGUILayout.IntField(new GUIContent("NbStreamChannel", PropertiesUtils.GetPropertyTooltip("NbStreamChannel")), 
                    Mathf.Min(MAXChannels, Mathf.Max(m_tempAudioSettings.nbStreamChannel, -1)));
                if (m_tempAudioSettings.nbStreamChannel == 0) m_tempAudioSettings.nbStreamChannel = -1;
                EditorGUILayout.LabelField("[1; " + MAXChannels + "], or auto {-1}");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                m_tempAudioSettings.bufferSize = EditorGUILayout.IntField(new GUIContent("BufferSize", PropertiesUtils.GetPropertyTooltip("BufferSize")), 
                    Mathf.Max(m_tempAudioSettings.bufferSize, 0));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                IndexFrameRate = Array.IndexOf(AvailableFrameRates, m_tempAudioSettings.sampleRate);
                if (IndexFrameRate < 0)
                {
                    IndexFrameRate = 0;
                    m_tempAudioSettings.sampleRate = AvailableFrameRates[0];
                }
                EditorGUILayout.BeginHorizontal();
                int idx = EditorGUILayout.Popup(new GUIContent("SampleRate", PropertiesUtils.GetPropertyTooltip("SampleRate")), IndexFrameRate, AvailableFrameRatesAsString);
                EditorGUILayout.LabelField("Hz");
                EditorGUILayout.EndHorizontal();
                if (IndexFrameRate != idx)
                {
                    IndexFrameRate = idx;
                    m_tempAudioSettings.sampleRate = AvailableFrameRates[IndexFrameRate];
                }

                EditorGUILayout.BeginHorizontal();
                m_tempAudioSettings.suggestedLatency = EditorGUILayout.FloatField(new GUIContent("SuggestedLatency", PropertiesUtils.GetPropertyTooltip("SuggestedLatency")), 
                    Mathf.Min(DefaultHighLatency,Mathf.Max(DefaultLowLatency, m_tempAudioSettings.suggestedLatency)));
                EditorGUILayout.LabelField("[" + DefaultLowLatency + "; " + DefaultHighLatency + "] s");
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            m_tempAudioSettings.bufferSize = EditorGUILayout.IntField(new GUIContent("BufferSize", PropertiesUtils.GetPropertyTooltip("BufferSize")),
                Mathf.Max(m_tempAudioSettings.bufferSize, 0));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(7f, false);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Cancel"))
        {
            m_isEditable = false;
        }
        if (GUILayout.Button("Save"))
        {
            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(m_targetAudioSettings, "Experimental Audio Settings" + m_targetAudioSettings.gameObject);

            m_targetAudioSettings.DeviceAudioPreset = m_tempPreset;
            m_targetAudioSettings.DeviceName = m_tempAudioSettings.deviceName;
            m_targetAudioSettings.AudioAPI = m_tempAudioSettings.audioAPI;
            m_targetAudioSettings.NbStreamChannel = m_tempAudioSettings.nbStreamChannel;
            m_targetAudioSettings.BufferSize = (int)m_tempAudioSettings.bufferSize;
            m_targetAudioSettings.SampleRate = (int)m_tempAudioSettings.sampleRate;
            m_targetAudioSettings.SuggestedLatency = m_tempAudioSettings.suggestedLatency;

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_targetAudioSettings);
            }
            m_isEditable = false;

        }
        EditorGUILayout.EndHorizontal();
    }

    private void ReinitCachedData()
    {
        AudioDeviceNames = Skinetic.Experimental.AudioStreamConfiguration.GetOutputDevicesNames();
        AudioDeviceIndex = Array.IndexOf(AudioDeviceNames, m_tempAudioSettings.deviceName);
        if (AudioDeviceIndex < 0)
        {
            AudioDeviceIndex = 0;
            m_tempAudioSettings.deviceName = AudioDeviceNames[0];
        }

        ApisNames = Skinetic.Experimental.AudioStreamConfiguration.GetOutputDeviceAPIs(m_tempAudioSettings.deviceName);
        IndexAPI = Array.IndexOf(ApisNames, m_tempAudioSettings.audioAPI);
        if (IndexAPI < 0)
            IndexAPI = 0;
        m_tempAudioSettings.audioAPI = ApisNames[IndexAPI];

        Skinetic.Experimental.AudioStreamConfiguration.GetOutputDeviceInfo(m_tempAudioSettings.deviceName, m_tempAudioSettings.audioAPI, ref MAXChannels, ref DefaultLowLatency, ref DefaultHighLatency);
            
        m_tempAudioSettings.suggestedLatency = Mathf.Min(DefaultHighLatency, Mathf.Max(DefaultLowLatency, m_tempAudioSettings.suggestedLatency));
        m_tempAudioSettings.nbStreamChannel = Mathf.Min(MAXChannels, Mathf.Max(-1, m_tempAudioSettings.nbStreamChannel));
        if (m_tempAudioSettings.nbStreamChannel == 0) m_tempAudioSettings.nbStreamChannel = -1;

        AvailableFrameRates = Skinetic.Experimental.AudioStreamConfiguration.GetSupportedStandardSampleRates(m_tempAudioSettings.deviceName, m_tempAudioSettings.audioAPI);
        AvailableFrameRatesAsString = AvailableFrameRates.Select(i => i.ToString()).ToArray();
        IndexFrameRate = Array.IndexOf(AvailableFrameRates, m_tempAudioSettings.sampleRate);

        if (IndexFrameRate < 0)
            IndexFrameRate = 0;
        m_tempAudioSettings.sampleRate = AvailableFrameRates[IndexFrameRate];
    }

    private void OnEnable()
    {
        m_isEditable = false;
        m_targetAudioSettings = (Skinetic.Experimental.AudioStreamConfiguration)target;
        m_tempPreset = m_targetAudioSettings.DeviceAudioPreset;
        m_tempAudioSettings = new Skinetic.Experimental.AudioStreamConfiguration.AudioSettings(m_targetAudioSettings.DeviceName,
                                                                                               m_targetAudioSettings.AudioAPI,
                                                                                               m_targetAudioSettings.SampleRate,
                                                                                               m_targetAudioSettings.BufferSize,
                                                                                               m_targetAudioSettings.NbStreamChannel,
                                                                                               m_targetAudioSettings.SuggestedLatency);
        ReinitCachedData();
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DisplayAudioSettings(m_targetAudioSettings);
        serializedObject.ApplyModifiedProperties();
    }

    private void OnDisable()
    {
        m_isEditable = false;
    }
}
