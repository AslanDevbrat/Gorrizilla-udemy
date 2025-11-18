using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkiEx_EffectBoostHandle : MonoBehaviour
{
    public Skinetic.HapticEffect m_hapticEffect;

    public SkiEx_ValueToText m_patternBoostText;
    public Text m_effectName;

    public float EffectBoostOverrideValue { get => (float)m_hapticEffect.EffectBoost; set { m_hapticEffect.EffectBoost = (int)value;} }

    // Start is called before the first frame update
    void Start()
    {
        if(m_hapticEffect.TargetDevice != null && m_hapticEffect.TargetPattern != null)
        {
            m_patternBoostText.UpdateText(m_hapticEffect.TargetDevice.GetPatternBoost(m_hapticEffect.TargetPattern));
            m_effectName.text = "Play\n" + m_hapticEffect.TargetPattern.Name;
        }
        else
        {
            Debug.LogError("Some references are broken.");
        }
    }

    public void PlayEffect()
    {
        m_hapticEffect.PlayEffect();
    }}
