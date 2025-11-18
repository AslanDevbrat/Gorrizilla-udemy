using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiEx_BasicUIEffects : MonoBehaviour
{
    public Skinetic.HapticEffect m_frontEffect;
    public Skinetic.HapticEffect m_backEffect;

    void Start()
    {
        if (m_frontEffect == null)
            Debug.Log("Front effect not set.");
        if (m_backEffect == null)
            Debug.Log("Back effect not set.");
    }

    public void BtnTriggerEffect(int index)
    {
        if (index == 0)
            m_frontEffect.PlayEffect();
        else
            m_backEffect.PlayEffect();
    }
}
