using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiEx_EffectAccumulation : MonoBehaviour
{
    public GameObject m_target;
    public float m_speed;
    public float m_lifespan;
    public float m_distance;
    public float m_heightScale;
    public GameObject[] m_shooters;
    private Skinetic.HapticEffect m_hapticEffect;
    private GameObject m_projectile;
    private LineRenderer m_line;
    private Skinetic.SkineticDevice m_device;
    private bool m_shooting;
    private int m_nbShots = 5;
    private float m_meanShotInterval = 0.3f;
    private float m_dispersionAngle = 5.0f;
    private float m_accumulationWindow = 2.0f;
    private int m_maxAccumulation = 2;


    public float GlobalBoost { get => (float)m_device.GetGlobalIntensityBoost(); set => m_device.SetGlobalIntensityBoost((int)value); }

    public float AccumulationWindow
    {
        get
        {
            return m_accumulationWindow;
        }

        set
        {
            m_accumulationWindow = value;
            OverrideAccumulationWindow();
        }
    }

    public float MaxEffectsAccumulation
    {
        get
        {
            return m_maxAccumulation;
        }

        set
        {
            m_maxAccumulation = (int)value;
            OverrideAccumulationWindow();
        }
    }

    public float NumberOfShots { get => m_nbShots; set => m_nbShots = (int)value; }

    public float MeanShotInterval { get => m_meanShotInterval; set => m_meanShotInterval = value; }

    public float Dispersion { get => m_dispersionAngle; set => m_dispersionAngle = value; }

    public void Shoot()
    {if(!m_shooting)
        {
            m_shooting = true;
            StartCoroutine(StartingShooting());
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //set components reference
        m_line = GetComponent<LineRenderer>();
        m_device = m_target.GetComponent<Skinetic.SkineticDevice>();
        m_hapticEffect = GetComponent<Skinetic.HapticEffect>();
        m_projectile = transform.GetChild(0).gameObject;

        //set hapticEffect references
        m_hapticEffect.TargetDevice = m_device;
        m_hapticEffect.TargetPattern = m_device.LoadedPatterns[0];

        //set Accumulation 
        m_device.SetAccumulationWindowToPattern(m_device.LoadedPatterns[0], m_device.LoadedPatterns[1], 0.1f, 5);
        

        //equalize distance of shooters
        for(int i = 0; i < m_shooters.Length; i++)
        {
            m_shooters[i].transform.position = m_distance * (m_shooters[i].transform.position - m_target.transform.position).normalized;
        }

        //GUI
        m_line.startWidth = 0.01f;
        m_line.endWidth = 0.01f;
        m_line.startColor = Color.red;
        m_line.endColor = Color.red;
    }

        private void OverrideAccumulationWindow()
    {
        m_device.SetAccumulationWindowToPattern(m_device.LoadedPatterns[0], m_device.LoadedPatterns[1], m_accumulationWindow, m_maxAccumulation);
    }

    private IEnumerator StartingShooting()
    {
        Debug.Log("StartCoroutine");
        float[] shotInterval = new float[m_nbShots - 1];
        int[] shooterOrder = new int[m_nbShots];

        for(int i = 0; i < m_nbShots - 1; i++)
        {
            shooterOrder[i] = Random.Range(0, 3);
            shotInterval[i] = 0.1f + Random.Range(0, 2 * m_meanShotInterval);
            Debug.Log(shotInterval[i]);
        }
        shooterOrder[m_nbShots - 1] = Random.Range(0, 3);

        for(int i = 0; i < m_nbShots; i++)
        {
            m_shooters[shooterOrder[i]].transform.rotation = Quaternion.LookRotation(m_target.transform.position - m_shooters[shooterOrder[i]].transform.position) * Quaternion.Euler(0, 0, Random.Range(-180f, 180f)) * Quaternion.Euler(m_dispersionAngle, 0, 0);
            GameObject go = Instantiate(m_projectile, m_shooters[shooterOrder[i]].transform.position, Quaternion.identity);
            go.GetComponent<SkiEx_SpatializedCollision>().HapticEffect = m_hapticEffect;
            go.GetComponent<SkiEx_SpatializedCollision>().Shooter = m_shooters[shooterOrder[i]].transform;
            go.GetComponent<SkiEx_SpatializedCollision>().HeightScale = m_heightScale;
            go.GetComponent<SkiEx_SpatializedCollision>().Target = m_target;
            go.GetComponent<Rigidbody>().velocity = m_speed * m_shooters[shooterOrder[i]].transform.forward;
            Destroy(go, m_lifespan);
            if (i == m_nbShots - 1)
                continue;
            yield return new WaitForSeconds(shotInterval[i]);
        }
        m_shooting = false;
        Debug.Log("StopCoroutine");
        yield return null;
    }

}
