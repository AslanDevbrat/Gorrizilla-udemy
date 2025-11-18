using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiEx_GlobalBoostHandle : MonoBehaviour
{
    public Skinetic.SkineticDevice m_device;


    public float GlobalBoost { get => (float)m_device.GetGlobalIntensityBoost(); set => m_device.SetGlobalIntensityBoost((int)value); }
   
}
