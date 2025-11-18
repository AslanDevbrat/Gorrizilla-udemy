using UnityEngine;

public class SkineticTriggerHaptics : MonoBehaviour
{
    [Header("Skinetic")]
    public Skinetic.HapticEffect hapticEffect;   // effect to play (pattern + device set here)
    public Transform vestRoot;                   // same as m_target in the sample (vest reference)

    [Tooltip("Scale from world Y to Skinetic height translation (meters). Tune by experiment.")]
    public float heightScale = 1.0f;

    [Tooltip("If true, flips front/back (e.g. if your vest is facing -Z).")]
    public bool invertFrontBack = false;

    /// <summary>
    ///  Call this from TriggerEvent.onTriggerEnter
    /// </summary>
    public void OnTriggerHit(Collider other)
    {
        if (hapticEffect == null || vestRoot == null)
        {
            Debug.LogWarning("SkineticTriggerHaptics: missing hapticEffect or vestRoot.");
            return;
        }

        // 1) Choose a hit point. For trigger volumes we usually approximate with the
        //    closest point on the OTHER collider to the vest root.
        Vector3 hitWorldPos = other.ClosestPoint(vestRoot.position);

        // 2) Bring it into vest-local space
        Vector3 local = vestRoot.InverseTransformPoint(hitWorldPos);

        // local.z > 0  -> in front of vest (if your vest faces +Z)
        if (invertFrontBack)
            local.z = -local.z;

        // 3) Compute heading (around Y axis)
        // Docs: HeadingRotation = angle in degrees in horizontal plane; positive rotates pattern to the LEFT of the vest. :contentReference[oaicite:0]{index=0}
        float headingDeg = -Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg; // minus so +X -> right

        // 4) Compute height (up/down)
        // Docs: HeightTranslation is height in meters to translate pattern vertically. :contentReference[oaicite:1]{index=1}
        float height = local.y * heightScale;

        // 5) Optionally compute tilting (front/back incline); here we keep it simple
        float tiltingDeg = 0f;

        // 6) Apply to Skinetic effect
        hapticEffect.HeightTranslation = height;
        hapticEffect.HeadingRotation = headingDeg;
        hapticEffect.TiltingRotation = tiltingDeg;

        // 7) Fire the haptic
        hapticEffect.PlayEffect();
    }
}
