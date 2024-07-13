using System.Collections;
using Fusion;
using PhotonFusionTutorial.Scripts.Network;
using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Weapon
{
    public class WeaponHandler : NetworkBehaviour
    {
        public ParticleSystem fireParticleSystem;
        
        [Networked(OnChanged = nameof(OnFireChanged))]
        public bool IsFiring { get; set; }

        private float lastTimeFired;

        public override void FixedUpdateNetwork()
        {
            if (GetInput(out NetworkInputData networkInputData))
            {
                if (networkInputData.isFirePressed)
                    Fire(networkInputData.aimForwardVector);
            }
        }

        private void Fire(Vector3 aimForward)
        {
            if (Time.time - lastTimeFired < 0.15f)
                return;

            StartCoroutine(FireEffect());
            lastTimeFired = Time.time;
        }

        private IEnumerator FireEffect()
        {
             IsFiring = true;
             
             fireParticleSystem.Play();

             yield return new WaitForSeconds(0.09f);
             
             IsFiring = false;
        }
        
        private static void OnFireChanged(Changed<WeaponHandler> changed)
        {
            Debug.Log($"{Time.time} OnFireChanged {changed.Behaviour.IsFiring}");
            
            var isFiringCurrent = changed.Behaviour.IsFiring;
            
            changed.LoadOld();
            
            var isFiringOld = changed.Behaviour.IsFiring;

            if (isFiringCurrent && !isFiringOld)
                changed.Behaviour.OnFireRemote();
        }

        private void OnFireRemote()
        {
            if (!Object.HasInputAuthority)
                fireParticleSystem.Play();
        }
    }
}
