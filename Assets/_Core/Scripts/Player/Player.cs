using Oculus.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaZilla
{
    [RequireComponent(typeof(PassthroughLayerController))]
    public class Player : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] OVRPassthroughLayer aliveLayer;
        [SerializeField] OVRPassthroughLayer toxicLayer;
        [SerializeField] OVRPassthroughLayer deadLayer;


        [Header("References")]
        [SerializeField] public TimeManipulator timeManipulator;
        [SerializeField] public PlayerMenu menu;
        [SerializeField] List<TriggerEvent> collidableBodyParts;
        [SerializeField] List<PlayerAbility> playerAbilities;
        [SerializeField] AudioSource deathNoise;
        private PassthroughLayerController passthroughLayerController;

        [Header("Events")]
        public UnityEvent onPlayerHit;

        public PlayerHaptics playerHaptics;

        void Start()
        {
            passthroughLayerController = GetComponent<PassthroughLayerController>();
            //playerHaptics = GetComponent<PlayerHaptics>();
            foreach (var part in collidableBodyParts)
            {
                part.onTriggerEnter.AddListener(OnBodyTriggerCollision);
                part.onCollisionEnter.AddListener(OnBodyCollision);
                Debug.Log("ALl lstener attached");
                
            }
            foreach (var ability in playerAbilities)
            {
                ability.onAbilityDeactivate.AddListener(OnAbilityDeactive);
            }
        }
        private void OnAbilityDeactive()
        {
            print("Re-activating aliveLayer");
            passthroughLayerController.SetActiveLayer(aliveLayer);
        }
        private void OnBodyTriggerCollision(Collider other)
        {
            //Die();
            Debug.Log("OnBodyTriggerCOllison");
            Destroy(other.gameObject);
        }

        private void OnBodyCollision(Collision collision)
        {

            Debug.Log("OnBodyCOllison");
           
            if (playerHaptics != null)
            {
                if (GameManager.Mode == "PQ")
                    playerHaptics.OnPQBulletHit();
                else
                    playerHaptics.PlayImpactHaptic(collision);
            }

            // 2. Destroy the bullet so it doesn't bounce off weirdly
            Destroy(collision.gameObject);

            // 3. Die
            //Die();
        }

        public void Die()
        {
            passthroughLayerController.SetActiveLayer(deadLayer);
            deathNoise.Play();
            onPlayerHit.Invoke();
        }
        public void Revive()
        {
            passthroughLayerController.SetActiveLayer(aliveLayer);
        }
        public void ActivateAbility(string abilityName)
        {
            passthroughLayerController.SetActiveLayer(toxicLayer);
            foreach (var ability in playerAbilities)
            {
                if (ability.abilityName == abilityName)
                {
                    ability.Activate();
                }
                else
                {
                    ability.Deactivate();
                }
            }
        }

    }
}
