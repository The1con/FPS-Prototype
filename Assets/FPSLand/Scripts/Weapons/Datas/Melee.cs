using FirstGearGames.FPSLand.Managers.Gameplay;
using FirstGearGames.FPSLand.Network;
using FirstGearGames.Managers.Global;
using GameKit.Utilities;
using UnityEngine;

namespace FirstGearGames.FPSLand.Weapons
{

    public class Melee : Weapon
    {
        #region Serialized.
        [Header("Melee")]
        /// <summary>
        /// Distance outward to overlap radius.
        /// </summary>
        [Tooltip("Distance outward to overlap radius.")]
        [SerializeField]
        private float _meleeDistance = 0.5f;
        /// <summary>
        /// Radius of melee attack.
        /// </summary>
        [Tooltip("Radius of melee attack.")]
        [SerializeField]
        private float _meleeRadius = 0.25f;
        #endregion

        /// <summary>
        /// Called when this weapon hits using an overlap.
        /// </summary>
        /// <param name="hit"></param>
        public override void OverlapImpact(Vector3 position, Vector3 direction, Collider other, NetworkRoles networkRoles)
        {
            bool serverOnly = (networkRoles == NetworkRoles.Server);
            bool owner = networkRoles.Contains(NetworkRoles.Owner);

            //Only process if effects should be shown.
            if (TerrainImpactAudioPrefab == null || serverOnly)
                return;

            //If not owner try to play at impact.
            if (!owner)
            {
                //Get a rough impact point.
                Ray ray = new Ray(position, direction);
                float distance = ReturnMeleeDistance() + ReturnMeleeRadius();
                RaycastHit hit;
                //If able to hit with the ray use hit info for impact.
                if (Physics.Raycast(ray, out hit, distance, (GlobalManager.LayerManager.DefaultLayer | GlobalManager.LayerManager.CharacterLayer)))
                    OfflineGameplayDependencies.AudioManager.PlayAtPoint(TerrainImpactAudioPrefab, hit.point);
                //No hit, use guestimated position.
                else
                    OfflineGameplayDependencies.AudioManager.PlayAtPoint(TerrainImpactAudioPrefab, position + (direction * distance));
            }
            //If owner play first person.
            else
            {
                OfflineGameplayDependencies.AudioManager.PlayFirstPerson(TerrainImpactAudioPrefab);
            }
        }

        /// <summary>
        /// Returns distance of cast to use for melee attacks.
        /// </summary>
        /// <returns></returns>
        public override float ReturnMeleeDistance()
        {
            return _meleeDistance;
        }
        /// <summary>
        /// Returns radius of cast to use for melee attacks.
        /// </summary>
        /// <returns></returns>
        public override float ReturnMeleeRadius()
        {
            return _meleeRadius;
        }

        /// <summary>
        /// Returns if this weapon is empty of all ammunition.
        /// </summary>
        /// <returns></returns>
        public override bool IsAmmunitionEmpty()
        {
            return false;
        }

        /// <summary>
        /// Returns if this weapon's current clip is empty of ammunition.
        /// </summary>
        /// <returns></returns>
        public override bool IsClipEmpty()
        {
            return false;
        }

        /// <summary>
        /// Returns how much ammunition the clip can old.
        /// </summary>
        public override int ReturnClipSize()
        {
            return 1;
        }

        /// <summary>
        /// Returns the ammunition left in the clip.
        /// </summary>
        /// <returns></returns>
        public override int ReturnClipRemaining()
        {
            return 1;
        }

        /// <summary>
        /// Returns the total ammunition left in the weapon.
        /// </summary>
        /// <returns></returns>
        public override int ReturnReserveAmmunitionRemaining()
        {
            return 0;
        }

    }


}