using UnityEngine;

public class TorsoStabilizer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("0 = Eye Level. Negative values move it down to the Chest/Torso.")]
    public float heightOffset = -0.3f;

    [Tooltip("Keeps the capsule upright even if you look down/up.")]
    public bool lockVerticalRotation = true;

    // We use LateUpdate to apply changes AFTER the VR tracking has moved the object
    void LateUpdate()
    {
        if (lockVerticalRotation)
        {
            // 1. Force the Rotation to be Upright (Kill Pitch and Roll)
            // We preserve the Y-axis (Yaw) so the vest still turns with you.
            Vector3 currentEuler = transform.eulerAngles;
            transform.rotation = Quaternion.Euler(0f, currentEuler.y, 0f);
        }

        // 2. Apply Height Offset
        // We modify the local Y position.
        // Since this object is likely a child of the Player/Tracking space,
        // 0 is the head height, and we offset from there.
        Vector3 localPos = transform.localPosition;
        transform.localPosition = new Vector3(localPos.x, localPos.y - heightOffset, localPos.z);
    }
}