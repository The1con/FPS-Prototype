using FirstGearGames.FPSLand.Characters.Vitals;
using FirstGearGames.Managers.Global;
using FishNet.Managing.Logging;
using FishNet.Object;
using GameKit.Utilities.ObjectPooling;
using GameKit.Utilities.Types;
using UnityEngine;

namespace FirstGearGames.FPSLand.Weapons
{


    public class FragGrenade : Grenade
    {
        /// <summary>
        /// Prefab to spawn when detonating.
        /// </summary>
        [Tooltip("Prefab to spawn when detonating.")]
        [SerializeField]
        private GameObject _detonatePrefab;
        /// <summary>
        /// Radius of damage.
        /// </summary>
        [Tooltip("Radius of damage.")]
        [SerializeField]
        private float _damageRadius = 5f;
        /// <summary>
        /// Damage which may be dealt based on vicinity of detonation.
        /// </summary>
        [Tooltip("Damage which may be dealt based on vicinity of detonation.")]
        [SerializeField]
        private FloatRange _damageRange = new FloatRange(1, 2);

        protected override void Update()
        {
            base.Update();
        }

        /// <summary>
        /// Detonates the grenade.
        /// </summary>
        [Server(Logging = LoggingType.Off)]
        protected override void Detonate()
        {
            base.Detonate();
            if (base.IsServer)
            {
                //Trace for players 
                Collider[] hits = Physics.OverlapSphere(transform.position, _damageRadius, GlobalManager.LayerManager.CharacterLayer);
                for (int i = 0; i < hits.Length; i++)
                {
                    Health h = hits[i].GetComponent<Health>();
                    if (h != null)
                    {
                        //Get damage based on distance from explosion.
                        float percent = 1f - Mathf.InverseLerp(0f, _damageRadius, Vector3.Distance(transform.position, hits[i].transform.position));
                        int damage = Mathf.CeilToInt(
                            Mathf.Lerp(_damageRange.Minimum, _damageRange.Maximum, percent));

                        h.RemoveHealth(damage);
                    }
                }

                ObserversSpawnDetonatePrefab();

                /* If server only then call destroy now. It will follow in order
                 * so clients will receive it after they get the RPC. 
                 * If client host destroy in the RPC. */
                if (base.IsServerOnly)
                    base.Despawn();
            }
        }

        /// <summary>
        /// Tells clients to spawn the detonate prefab.
        /// </summary>
        [ObserversRpc]
        private void ObserversSpawnDetonatePrefab()
        {
            SpawnDetonatePrefab();
            //If also client host destroy here.
            if (base.IsServer)
                base.Despawn();
        }

        /// <summary>
        /// Spawns the detonate prefab.
        /// </summary>
        [Client(Logging = LoggingType.Off)]
        private void SpawnDetonatePrefab()
        {
            ObjectPool.Retrieve(_detonatePrefab, transform.position, Quaternion.identity);
        }
                
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, _damageRadius);
        }
    }


}

