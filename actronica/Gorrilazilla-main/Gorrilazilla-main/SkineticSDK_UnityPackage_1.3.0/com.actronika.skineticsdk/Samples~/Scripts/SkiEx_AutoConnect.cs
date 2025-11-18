using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkiEx_AutoConnect : MonoBehaviour
{
    public Skinetic.SkineticDevice m_device;
    public Text m_text;
    private string m_connectString = "";

    void OnEnable()
    {
        if (m_device == null)
            Debug.Log("SkineticDevice not set.");

        if (m_text == null)
            Debug.Log("Text not set.");

        StartCoroutine(UpdateText());
        m_device.SetConnectionCallback(ConnectionCallback);
        m_device.Connect(Skinetic.SkineticDevice.OutputType.E_AUTODETECT, 0);
    }

    void OnDisable()
    {
        Skinetic.SkineticDevice.ConnectionState state;
        Debug.Log("Asynch disconnection of the Skinetic device.");
        m_device.Disconnect();
        state = m_device.ConnectionStatus();
        while (state != Skinetic.SkineticDevice.ConnectionState.E_DISCONNECTED)
        { state = m_device.ConnectionStatus(); }
        StopAllCoroutines();
        Debug.Log("Device disconnected successfully.");
    }

    private IEnumerator UpdateText()
    {
        while(true)
        {
            if(m_connectString != "")
            {
                m_text.text = m_connectString;
                m_connectString = "";
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void ConnectionCallback(Skinetic.SkineticDevice.ConnectionState state, int error, System.UInt32 serialNumber)
    {
        if (error < 0)
        {
            m_connectString = "Connection error: " + Skinetic.SkineticDevice.getSDKError(error);
        }
        else if (state == Skinetic.SkineticDevice.ConnectionState.E_CONNECTED)
        {
            m_connectString = "Device Connected: " + Skinetic.SkineticDevice.SerialNumberToString(serialNumber);
        }
        else if (state == Skinetic.SkineticDevice.ConnectionState.E_DISCONNECTED)
        {
            m_connectString = "Device Disconnected";
        }
    }
}
