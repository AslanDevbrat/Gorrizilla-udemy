using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkiEx_DeviceScan : MonoBehaviour
{
    public Skinetic.SkineticDevice m_device;
    public Dropdown m_deviceDropdown;
    public Text m_deviceInfoText;
    public Text m_connectionText;

    public List<Skinetic.SkineticDevice.DeviceInfo> m_scanDevices;

    private bool m_coroutineActive = false;
    private int m_selectedIndex = -1;
    void OnEnable()
    {
        if (m_device == null)
            Debug.Log("SkineticDevice not set.");
        if (m_deviceDropdown == null)
            Debug.Log("Dropdown not set.");
        if (m_deviceInfoText == null)
            Debug.Log("Text field not set.");
        if (m_connectionText == null)
            Debug.Log("Text field not set.");

        m_deviceDropdown.ClearOptions();
        m_deviceDropdown.AddOptions(new List<string>{"no device"});
        m_deviceDropdown.onValueChanged.AddListener(delegate { DeviceSelection(m_deviceDropdown); });
    }

    void OnDisable()
    {
        Debug.Log("Asynch disconnection of the Skinetic device.");
        m_device.Disconnect();
        while (m_device.ConnectionStatus() != Skinetic.SkineticDevice.ConnectionState.E_DISCONNECTED)
        { }
        StopAllCoroutines();
        Debug.Log("Device disconnected successfully.");
    }

    public void BtnStartScan()
    {
        if (m_coroutineActive)
            return;
        StartCoroutine(ScanDevices());
    }

    public void BtnConnect()
    {
        if (m_selectedIndex < 0)
            return;
        if (m_coroutineActive)
            return;
        StartCoroutine(DeviceConnection());
    }

    private void DisplayDeviceInfo(int index)
    {
        string text;
        if (index < 0 || index >= m_scanDevices.Count)
        {
            text = "Device #\n";
            text += "   Output Type     :\n";
            text += "   Serial Number  :\n";
            text += "   Device Type     :\n";
            text += "   Device Version :";
            m_deviceInfoText.text = text;
            return;
        }
        
        text = "Device #" + index + "\n";
        text += "   Output Type     : " + m_scanDevices[index].outputType + "\n";
        text += "   Serial Number  : " + Skinetic.SkineticDevice.SerialNumberToString(m_scanDevices[index].serialNumber) + "\n";
        text += "   Device Type     : " + m_scanDevices[index].deviceType + "\n";
        text += "   Device Version : " + m_scanDevices[index].deviceVersion;
        m_deviceInfoText.text = text;
    }

    private void UpdateDropdown()
    {
        m_deviceDropdown.ClearOptions();
        List<string> listName = new List<string>();
        for (int i = 0; i < m_scanDevices.Count; i++)
        {
            listName.Add("Device #" + i);
        }
        if (listName.Count == 0)
        {
            listName.Add("noDevice");
            m_selectedIndex = -1;
        }
        m_deviceDropdown.AddOptions(listName);
    }

    private void DeviceSelection(Dropdown change)
    {
        if (change.value < m_scanDevices.Count)
        {
            m_selectedIndex = change.value;
        }
        else
            m_selectedIndex = -1;
    }

    private IEnumerator DeviceConnection()
    {
        m_coroutineActive = true;

        int ret = m_device.Connect(m_scanDevices[m_selectedIndex].outputType, m_scanDevices[m_selectedIndex].serialNumber);
        if (ret < 0)
            m_connectionText.text = "Could not connected to selected device: " + Skinetic.SkineticDevice.getSDKError(ret) + " Scan again to refresh the lsit of available device.";
        else
        {
            while (m_device.ConnectionStatus() == Skinetic.SkineticDevice.ConnectionState.E_CONNECTING)
            {
                m_connectionText.text = "Connecting...";
                yield return new WaitForSeconds(0.2f);
            }

            if(m_device.ConnectionStatus() == Skinetic.SkineticDevice.ConnectionState.E_CONNECTED)
            {
                m_connectionText.text = "Device Connected: " + m_device.GetDeviceSerialNumberAsString();
            }
            else if (m_device.ConnectionStatus() == Skinetic.SkineticDevice.ConnectionState.E_DISCONNECTED)
            {
                m_connectionText.text = "Device Disconnected";
            }
            else
            {
                m_connectionText.text = "Undefined State";
            }
        }
        
        m_coroutineActive = false;
        yield return null;
    }

    private IEnumerator ScanDevices()
    {
        m_coroutineActive = true;

        //disconnect device if any as the scan cannot occurs while a device is connected.
        if (m_device.ConnectionStatus() != Skinetic.SkineticDevice.ConnectionState.E_DISCONNECTED)
        {
            m_device.Disconnect();
        }
        while(m_device.ConnectionStatus() != Skinetic.SkineticDevice.ConnectionState.E_DISCONNECTED)
        {
            yield return new WaitForSeconds(0.2f);
        }
        m_connectionText.text = "Device Disconnected";

        //start scanning procedure
        m_device.ScanDevices(Skinetic.SkineticDevice.OutputType.E_AUTODETECT);
        while (m_device.ScanStatus() != 0)
        {
            yield return new WaitForSeconds(0.2f);
        }

        m_scanDevices = m_device.GetScannedDevices();
        UpdateDropdown();
        yield return null;

        if (m_scanDevices.Count > 0)
        {
            m_selectedIndex = 0;
        }
        else
            m_selectedIndex = -1;
        DisplayDeviceInfo(m_selectedIndex);
        m_coroutineActive = false;
        yield return null;
    }
}
