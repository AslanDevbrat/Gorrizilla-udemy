using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiEx_Boost : MonoBehaviour
{
    public Skinetic.SkineticDevice m_device;

    public float GlobalBoost { get => (float)m_device.GetGlobalIntensityBoost(); set => m_device.SetGlobalIntensityBoost((int)value); }
    void OnEnable()
    {
        //register patterns to haptic device
        Skinetic.HapticEffect[] hapticEffects = GetComponents<Skinetic.HapticEffect>();
        foreach(Skinetic.HapticEffect effect in hapticEffects)
        {
            m_device.LoadPattern(effect.TargetPattern);
        }
        GlobalBoost = 60;
    }
}
