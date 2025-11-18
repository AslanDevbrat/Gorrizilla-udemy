using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkiEx_ParametricPlay : MonoBehaviour
{
    public Skinetic.SkineticDevice m_device;
    public Skinetic.HapticEffect m_hapticEffect;
    public Dropdown m_patternDropdown;
    public Dropdown m_playStrategy;

    private float m_stopFadeout;

    public float StopFadeout { get => m_stopFadeout; set => m_stopFadeout = value; }
    public float GlobalBoost { get => (float)m_device.GetGlobalIntensityBoost(); set => m_device.SetGlobalIntensityBoost((int)value); }

    // Start is called before the first frame update
    void Start()
    {
        m_patternDropdown.ClearOptions();
        List<string> listName = new List<string>();
        for (int i = 0; i < m_device.LoadedPatterns.Count; i++)
        {
            listName.Add(m_device.LoadedPatterns[i].Name);
        }
        m_patternDropdown.AddOptions(listName);

        m_hapticEffect.TargetPattern = m_device.LoadedPatterns[0];
        m_patternDropdown.onValueChanged.AddListener(delegate { PatternDropdownValueChanged(m_patternDropdown); });
        m_playStrategy.onValueChanged.AddListener(delegate { StrategyDropdownValueChanged(m_playStrategy); });
        m_device.SetGlobalIntensityBoost(0);
    }


    public void SetRepeatCountFromSlider(float val)
    {
        m_hapticEffect.RepeatCount = (int)val;
    }

    public void BtnTriggerPlay()
    {
        m_hapticEffect.PlayEffect();
    }

    public void BtnTriggerStop()
    {
        m_hapticEffect.StopEffect(m_stopFadeout);
    }

    void PatternDropdownValueChanged(Dropdown change)
    {
        m_hapticEffect.TargetPattern = m_device.LoadedPatterns[change.value];
    }

    void StrategyDropdownValueChanged(Dropdown change)
    {
        m_hapticEffect.StrategyOnPlay = (Skinetic.HapticEffect.PlayStrategy)change.value;
        m_device.StopEffect(m_hapticEffect, 0);
    }
}
