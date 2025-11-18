using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiEx_BasicSingleEffect : MonoBehaviour
{
    public Skinetic.SkineticDevice m_device;
    public Skinetic.HapticEffect m_hapticEffect;
    // Start is called before the first frame update
    void OnEnable()
    {
        if (m_device == null)
            Debug.Log("SkineticDevice not set.");
        if (m_hapticEffect == null)
            Debug.Log("Haptic effect not set.");
        
        //get haptic effect pattern and load it to the device
        m_device.LoadPattern(m_hapticEffect.TargetPattern);
    }

    public void BtnPlay()
    {
        m_device.PlayEffect(m_hapticEffect);
    }

    public void BtnStop()
    {
        m_device.StopEffect(m_hapticEffect, 0);
    }
}
