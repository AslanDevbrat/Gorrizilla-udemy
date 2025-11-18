using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Skinetic;

namespace GorillaZilla
{
    public class PlayerHaptics : MonoBehaviour
    {
        [Header("Skinetic Settings")]
        [SerializeField] private SkineticDevice skineticDevice;
        [SerializeField] private HapticEffect hitEffect;

        [Header("Calibration")]
        [SerializeField] private float heightScale = 1.0f;

        private void Start()
        {
            if (hitEffect != null && skineticDevice != null)
            {
                hitEffect.TargetDevice = skineticDevice;
            }
        }

        // CHANGED: Now takes 'Collision' instead of 'Transform'
        public void PlayImpactHaptic(Collision collision)
        {
            Debug.Log("PlayImpactHaptic");
            if (hitEffect == null || skineticDevice == null || collision.contactCount == 0) return;
            Debug.Log("PlayImpactHaptic cleared");
            // 1. Get Exact Contact Point
            // GetContact(0) gives the first point where physics meshes touched.
            Vector3 contactCenter = collision.GetContact(0).point;

            // 2. Determine "Shooter" Position 
            // We assume the impact came from the direction the bullet was traveling
            // -collision.relativeVelocity is often used, but -transform.forward of the bullet is safer for projectiles.
            Vector3 incomingDirection = -collision.transform.forward;
            Vector3 pseudoShooterPos = contactCenter + incomingDirection;

            // 3. Perform Spatial Calculations
            float heightTranslation = (contactCenter.y - transform.position.y) / heightScale;

            Vector3 impactVector = pseudoShooterPos - contactCenter;

            // Yaw (Heading)
            Vector3 projectedImpact = Vector3.ProjectOnPlane(impactVector, transform.up).normalized;
            float headingRotation = Vector3.SignedAngle(transform.forward, projectedImpact, transform.up);

            // Pitch (Tilting)
            float tiltingRotation = Vector3.SignedAngle(
                Vector3.ProjectOnPlane(impactVector, transform.up),
                impactVector,
                Vector3.Cross(impactVector, transform.up)
            );

            // 4. Play Effect
            hitEffect.HeightTranslation = heightTranslation;
            hitEffect.HeadingRotation = headingRotation;
            hitEffect.TiltingRotation = tiltingRotation;
            hitEffect.PlayEffect();
        }
    }
}