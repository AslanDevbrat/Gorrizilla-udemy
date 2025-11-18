using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkiEx_Shooter : MonoBehaviour
{
    public Dropdown m_patternDropdown;
    public GameObject m_target;
    public float m_speed;
    public float m_lifespan;
    public float m_distance;
    public float m_heightScale;
    private Skinetic.HapticEffect m_hapticEffect;
    private GameObject m_projectile;
    private LineRenderer m_line;
    private Skinetic.SkineticDevice m_device;
    private float m_time;
    private bool m_shoot;
    private float m_dispersionAngle;

    public float VerticalRotation
    {
        get
        {
            return Vector3.SignedAngle(m_target.transform.forward, this.transform.position - m_target.transform.position, m_target.transform.up);
        }

        set
        {
            Vector3 temp = m_distance * (Quaternion.Euler(0, value, 0) * m_target.transform.forward);
            this.transform.position = new Vector3(temp.x, this.transform.position.y, temp.z);
        }
    }

    public float VerticalTranslation
    {
        get
        {
            return this.transform.position.y;
        }

        set
        {
            this.transform.position = new Vector3(this.transform.position.x, value, this.transform.position.z);
        }
    }

    public float Dispersion { get => m_dispersionAngle; set => m_dispersionAngle = value; }
    public float GlobalBoost { get => (float)m_device.GetGlobalIntensityBoost(); set => m_device.SetGlobalIntensityBoost((int)value); }



    public void Shoot()
    {
        m_shoot = true;
    }


    void PatternDropdownValueChanged(Dropdown change)
    {
        m_hapticEffect.TargetPattern = m_device.LoadedPatterns[change.value];
    }

    // Start is called before the first frame update
    void Start()
    {
        //set components reference
        m_line = GetComponent<LineRenderer>();
        m_device = m_target.GetComponent<Skinetic.SkineticDevice>();
        m_hapticEffect = GetComponent<Skinetic.HapticEffect>();
        m_projectile = transform.GetChild(0).gameObject;

        //set pattern dropdown menu
        m_patternDropdown.ClearOptions();
        List<string> listName = new List<string>();
        for (int i = 0; i < m_device.LoadedPatterns.Count; i++)
        {
            listName.Add(m_device.LoadedPatterns[i].Name);
        }
        m_patternDropdown.AddOptions(listName);
        m_patternDropdown.onValueChanged.AddListener(delegate { PatternDropdownValueChanged(m_patternDropdown); });

        //set hapticEffect references
        m_hapticEffect.TargetDevice = m_device;
        m_hapticEffect.TargetPattern = m_device.LoadedPatterns[0];
        
        /*//GUI
        m_line.startWidth = 0.01f;
        m_line.endWidth = 0.01f;
        m_line.startColor = Color.red;
        m_line.endColor = Color.red;*/
    }

    // Update is called once per frame
    void Update()
    {
        /*m_line.SetPosition(0, this.transform.position);
        m_line.SetPosition(1, m_target.transform.position);*/
        m_time += Time.deltaTime;
        if (m_time > 1 || m_shoot)
        {
            m_time = 0;
            m_shoot = false;
            transform.rotation = Quaternion.LookRotation(m_target.transform.position - this.transform.position) * Quaternion.Euler(0, 0, Random.Range(-180f, 180f)) * Quaternion.Euler(m_dispersionAngle, 0, 0);
            GameObject go = Instantiate(m_projectile, this.transform.position, Quaternion.identity);
            go.GetComponent<SkiEx_SpatializedCollision>().Target = m_target;
            go.GetComponent<SkiEx_SpatializedCollision>().HapticEffect = m_hapticEffect;
            go.GetComponent<SkiEx_SpatializedCollision>().Shooter = transform;
            go.GetComponent<SkiEx_SpatializedCollision>().HeightScale = m_heightScale;
            go.GetComponent<Rigidbody>().velocity = m_speed * this.transform.forward;
            Destroy(go, m_lifespan);
        }
    }
}
