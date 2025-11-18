using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkiEx_ValueToText : MonoBehaviour
{
    private Text m_text;
    private string m_header;
    // Start is called before the first frame update
    void Start()
    {
        m_text = GetComponent<Text>();
        m_header = m_text.text;
        if(GetComponentInParent<Slider>() != null)
            m_text.text = m_header + ": " + GetComponentInParent<Slider>().value.ToString();
    }

    public void UpdateText(float val)
    {
        m_text.text = m_header + ": " + val.ToString();
    }
}
