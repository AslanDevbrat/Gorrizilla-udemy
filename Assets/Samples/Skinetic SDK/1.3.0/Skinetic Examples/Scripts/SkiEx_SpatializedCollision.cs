using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiEx_SpatializedCollision : MonoBehaviour
{
    private float m_heightScale;
    private Skinetic.HapticEffect m_hapticEffect;
    private Transform m_shooter;
    private GameObject m_target;
    public Skinetic.HapticEffect HapticEffect { get => m_hapticEffect; set => m_hapticEffect = value; }
    public Transform Shooter { get => m_shooter; set => m_shooter = value; }
    public float HeightScale { get => m_heightScale; set => m_heightScale = value; }

    public GameObject Target { get => m_target; set => m_target = value; }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject != m_target)
            return;
        ContactPoint[] contacts = new ContactPoint[collision.contactCount];
        Vector3 contactCenter = new Vector3();
        collision.GetContacts(contacts);

        for (int i = 0; i < contacts.Length; i++)
        {
            contactCenter += contacts[i].point;
        }
        contactCenter /= contacts.Length;
        m_hapticEffect.TiltingRotation = Vector3.SignedAngle(Vector3.ProjectOnPlane(m_shooter.position - contactCenter, collision.transform.up), m_shooter.position - contactCenter, Vector3.Cross(m_shooter.position - contactCenter, collision.transform.up));
        m_hapticEffect.HeightTranslation = (contactCenter - collision.transform.position).y / m_heightScale;
        m_hapticEffect.HeadingRotation = Vector3.SignedAngle(collision.transform.forward, Vector3.ProjectOnPlane(m_shooter.position - contactCenter, collision.transform.up).normalized, collision.transform.up);
        //Debug.Log(m_hapticEffect.TiltingRotation.ToString() + "//" + m_hapticEffect.HeightTranslation.ToString() + "//" + m_hapticEffect.HeadingRotation.ToString());
        m_hapticEffect.PlayEffect();
    }
}
