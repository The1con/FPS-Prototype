using FirstGearGames.FPSLand.Characters.Weapons;
using FirstGearGames.FPSLand.Clients;
using FirstGearGames.FPSLand.Managers.Gameplay;
using FirstGearGames.FPSLand.Network;
using FirstGearGames.Managers.Global;
using FishNet.Managing.Timing;
using GameKit.CameraShakers;
using GameKit.Utilities;
using GameKit.Utilities.ObjectPooling;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstGearGames.FPSLand.Weapons
{

    public class Firearm : Weapon
    {
        #region Types.
        /// <summary>
        /// Data used to configure recoil.
        /// </summary>
        [System.Serializable]
        private class RecoilConfiguration
        {
            /// <summary>
            /// 
            /// </summary>
            [Tooltip("Maximum multiplier which can be applied to recoil with continuous fire.")]
            [SerializeField]
            private float _maximumRecoilMultiplier = 4f;
            /// <summary>
            /// Maximum multiplier which can be applied to recoil with continuous fire.
            /// </summary>
            public float MaximumRecoilMultiplier { get { return _maximumRecoilMultiplier; } }
            /// <summary>
            /// 
            /// </summary>
            [Tooltip("How much to multiply recoil after firing.")]
            [SerializeField]
            private float _fireKickRate = 4f;
            /// <summary>
            /// How much to multiply recoil after firing.
            /// </summary>
            public float FireKickRate { get { return _fireKickRate; } }
            /// <summary>
            /// 
            /// </summary>
            [Tooltip("How much to settle accumulated recoil over a second. This reduces the recoil multiplier.")]
            [SerializeField]
            private float _settleRate = 4f;
            /// <summary>
            /// How much to settle accumulated recoil over a second. This reduces the recoil multiplier.
            /// </summary>
            public float SettleRate { get { return _settleRate; } }
        }
        #endregion

        #region Serialized.
        [Header("Firearm")]
        /// <summary>
        /// How often a tracer may draw.
        /// </summary>
        [Tooltip("How often a tracer may draw.")]
        [SerializeField]
        private float _tracerInterval = 0.025f;
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("How much ammunition a clip can hold.")]
        [SerializeField]
        private int _clipSize = 30;
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Maximum reserve ammunition this weapon can hold.")]
        [SerializeField]
        private int _maximumReverseAmmunition = 90;
        /// <summary>
        /// How long it takes to reload.
        /// </summary>
        [Tooltip("How long it takes to reload.")]
        [SerializeField]
        private float _reloadDuration = 3f;
        /// <summary>
        /// Muzzle flash for this weapon.
        /// </summary>
        [Tooltip("Muzzle flash for this weapon.")]
        [SerializeField]
        private GameObject _muzzleFlashPrefab;
        /// <summary>
        /// Prefab to spawn as a tracer.
        /// </summary>
        [Tooltip("Prefab to spawn as a tracer.")]
        [SerializeField]
        private Tracer _tracerPrefab;
        /// <summary>
        /// Audio to play when a bullet passes by.
        /// </summary>
        [Tooltip("Audio to play when a bullet passes by.")]
        [SerializeField]
        private GameObject _bulletWhizAudioPrefab;
        /// <summary>
        /// Audio to play when reloading.
        /// </summary>
        [Tooltip("Audio to play when reloading.")]
        [SerializeField]
        private GameObject _reloadAudioPrefab;
        /// <summary>
        /// ShakeData used to show recoil on the model.
        /// </summary>
        [Tooltip("ShakeData used to show recoil on the model.")]
        [SerializeField]
        private ShakeData _modelRecoilShake;
        /// <summary>
        /// ShakeData used to show recoil on the camera.
        /// </summary>
        [Tooltip("ShakeData used to show recoil on the camera.")]
        [SerializeField]
        private ShakeData _cameraRecoilShake;
        /// <summary>
        /// Configuration for recoil.
        /// </summary>
        [Tooltip("Configuration for recoil.")]
        [SerializeField]
        private RecoilConfiguration _recoilConfiguration = new RecoilConfiguration();
        #endregion

        #region Private.
        /// <summary>
        /// Last time a tracer was spawned.
        /// </summary>
        private float _nextTracerTime = 0f;
        /// <summary>
        /// Current multiplier for recoil on this weapon.
        /// </summary>
        private float _currentRecoilMultiplier = 1f;
        /// <summary>
        /// Last time the weapon was fired.
        /// </summary>
        private float _lastFireTime = 0f;
        /// <summary>
        /// Current amount of ammunition in the clip.
        /// </summary>
        private int _currentClipAmmunition;
        /// <summary>
        /// Current amount of ammunition remaining in reserve.
        /// </summary>
        private int _currentReserveAmmunition;
        /// <summary>
        /// Coroutine for reload.
        /// </summary>
        private Coroutine _reloadCoroutine = null;
        /// <summary>
        /// Current audio being played for a reload.
        /// </summary>
        private GameObject _currentReloadAudio = null;
        #endregion

        private void Awake()
        {
            Reset();
        }

        /// <summary>
        /// Resets the weapon as though it's not been used.
        /// </summary>
        private void Reset()
        {
            _currentRecoilMultiplier = 1f;
            _currentClipAmmunition = _clipSize;
            _currentReserveAmmunition = _maximumReverseAmmunition;
        }

        /// <summary>
        /// Called when this weapon fires.
        /// </summary>
        public override void Fire(PreciseTick pt, Vector3 position, Vector3 direction, NetworkRoles networkRoles)
        {
            base.Fire(pt, position, direction, networkRoles);

            RemoveFromCurrentClip(1);

            bool serverOnly = (networkRoles == NetworkRoles.Server);
            bool owner = networkRoles.Contains(NetworkRoles.Owner);
            /* Special effects. Don't show if server only. */
            if (!serverOnly)
            {
                Transform exitPoint = (owner) ? base.FirstPersonModel.ExitPoint : base.ThirdPersonModel.ExitPoint;
                //Muzzle flash.
                if (_muzzleFlashPrefab != null)
                {
                    GameObject r = ObjectPool.Retrieve(_muzzleFlashPrefab, exitPoint.position, exitPoint.rotation);
                    r.transform.SetParent(exitPoint);

                    //Change layer on muzzle flash.
                    int layerValue = (owner) ?
                        Layers.LayerMaskToLayerNumber(GlobalManager.LayerManager.NoClipLayer) :
                        Layers.LayerMaskToLayerNumber(GlobalManager.LayerManager.DefaultLayer)
                        ;

                    List<Transform> children = new List<Transform>();
                    Transforms.GetComponentsInChildren<Transform>(r.transform, children, true);
                    for (int i = 0; i < children.Count; i++)
                        children[i].gameObject.layer = layerValue;
                }
                //Tracer.
                if (_tracerPrefab != null && _tracerInterval > 0f && Time.time > _nextTracerTime)
                {
                    //Only fire tracers if they will move a decent distance before hitting something.
                    if (!Physics.Linecast(exitPoint.position, exitPoint.position + (direction * 5f), (GlobalManager.LayerManager.DefaultLayer | GlobalManager.LayerManager.HitboxLayer)))
                    {
                        //Set to spawn a few units out from shooter.
                        Vector3 spawn = exitPoint.position + (direction * 1.5f);
                        _nextTracerTime = Time.time + _tracerInterval;
                        Tracer tracer = ObjectPool.Retrieve<Tracer>(_tracerPrefab.gameObject, spawn, exitPoint.rotation);
                        tracer.Initialize(direction, 300f);
                    }
                }

                //Add onto the recoil multiplier.
                _currentRecoilMultiplier = Mathf.Clamp(_currentRecoilMultiplier + _recoilConfiguration.FireKickRate, 1f, _recoilConfiguration.MaximumRecoilMultiplier);
                _lastFireTime = Time.time;
                CheckPlayBulletPassBy(position, direction, owner);
            }
        }

        /// <summary>
        /// Checks to play the bullet pass by audio.
        /// </summary>
        private void CheckPlayBulletPassBy(Vector3 position, Vector3 direction, bool owner)
        {
            //Don't play bullet pass by on own bullets.
            if (owner)
                return;
            //No bullet pass by audio.
            if (_bulletWhizAudioPrefab == null)
                return;
            /* These checks are gross but it alleviates
            * creating a chain of references. */
            //Check to make sure local player exist.
            if (ClientInstance.Instance == null || ClientInstance.Instance.PlayerSpawner == null || ClientInstance.Instance.PlayerSpawner.SpawnedCharacterData.NetworkObject == null)
                return;
            //Local player is dead.
            if (ClientInstance.Instance.PlayerSpawner.SpawnedCharacterData.Health.CurrentHealth <= 0)
                return;

            Transform localCharacter = ClientInstance.Instance.PlayerSpawner.SpawnedCharacterData.NetworkObject.transform;

            //Approximation of player height.
            float playerHeight = 1.7f;
            //Set approximate location of local players head.
            Vector3 headPosition = localCharacter.transform.position + new Vector3(0f, playerHeight, 0f);
            //Get the distance between head and where shot comes from.
            float distance = Vector3.Distance(headPosition, position);
            //Travel distance in fire direction.
            Vector3 endPosition = position + (direction * distance);
            //Determine how close end position is from head.
            float nearDistance = Vector3.Distance(endPosition, headPosition);

            //Maximum distance before whiz won't be played.
            float maximumWhizDistance = 1.5f;
            if (nearDistance <= maximumWhizDistance)
                OfflineGameplayDependencies.AudioManager.PlayAtPoint(_bulletWhizAudioPrefab, endPosition);
        }

        /// <summary>
        /// Removes a specified amount of ammunition from the current clip.
        /// </summary>
        /// <param name="value"></param>
        protected override void RemoveFromCurrentClip(int value)
        {
            _currentClipAmmunition = Mathf.Max(0, _currentClipAmmunition - value);
            base.RemoveFromCurrentClip(value);
        }
        /// <summary>
        /// Removes a specified amount of ammunition from total ammunition.
        /// </summary>
        /// <param name="value"></param>
        protected override void RemoveFromReserveAmmunition(int value)
        {
            _currentReserveAmmunition = Mathf.Max(0, _currentReserveAmmunition - value);
            base.RemoveFromReserveAmmunition(value);
        }
        /// <summary>
        /// Called when this weapon hits using a cast.
        /// </summary>
        /// <param name="hit"></param>
        public override void RayImpact(RaycastHit hit, bool serverOnly)
        {
            base.RayImpact(hit, serverOnly);

            GameObject prefab = (GlobalManager.LayerManager.InLayerMask(hit.collider.gameObject, GlobalManager.LayerManager.HitboxLayer))
                ? CharacterImpactPrefab : TerrainImpactPrefab;
            if (prefab != null && !serverOnly)
                ObjectPool.Retrieve(prefab, hit.point, Quaternion.LookRotation(hit.normal));
        }

        /// <summary>
        /// Sets the weapon's equipped state.
        /// </summary>
        /// <param name="equipped"></param>
        /// <param name="owner">True if owner.</param>
        /// <param name="reset">True to reset this weapon.</param>
        public override void SetEquipped(bool equipped, NetworkRoles networkRoles)
        {
            base.SetEquipped(equipped, networkRoles);
            if (!equipped)
                StopReload();
        }


        /// <summary>
        /// Removes weapon from inventory.
        /// </summary>
        public override void RemoveFromInventory()
        {
            base.RemoveFromInventory();
            //Reset weapon.
            Reset();
        }

        /// <summary>
        /// Called when a reload is interrupted.
        /// </summary>
        private void StopReload()
        {
            if (_reloadCoroutine != null)
            {
                StopCoroutine(_reloadCoroutine);
                _reloadCoroutine = null;
                //If audio is playing.
                if (_currentReloadAudio != null && _currentReloadAudio.activeInHierarchy)
                    ObjectPool.Store(_currentReloadAudio);
            }
        }

        /// <summary>
        /// Reloads this weapon using settings.
        /// </summary>
        /// <returns></returns>
        private IEnumerator __Reload(NetworkRoles networkRoles, bool instant)
        {
            bool serverOnly = (networkRoles == NetworkRoles.Server);
            bool owner = networkRoles.Contains(NetworkRoles.Owner);

            //If not server only then play audio.
            if (!serverOnly && _reloadAudioPrefab != null)
            {
                //If owner play first person.
                if (owner)
                {
                    _currentReloadAudio = OfflineGameplayDependencies.AudioManager.PlayFirstPerson(_reloadAudioPrefab);
                }
                //If not owner attach to topmost transform.
                else
                {
                    _currentReloadAudio = OfflineGameplayDependencies.AudioManager.PlayAtPoint(_reloadAudioPrefab, transform.root.position);
                    _currentReloadAudio.transform.SetParent(transform.root);
                }
            }

            /*Only perform the rest of reload beyond audio/vfx if server. */
            if (networkRoles.Contains(NetworkRoles.Server))
            {
                //Wait reload duration if not instant.
                if (!instant)
                    yield return new WaitForSeconds(ReturnReloadDuration());

                //Get amount to restore.
                int requiredAmmunition = ReturnClipSize() - ReturnClipRemaining();
                int ammunitionToRestore = Mathf.Min(requiredAmmunition, ReturnReserveAmmunitionRemaining());
                //Add to clip and remove from reserve.
                _currentClipAmmunition += ammunitionToRestore;
                _currentReserveAmmunition -= ammunitionToRestore;
                //Make base calls.
                base.AddToCurrentClip(ammunitionToRestore);
                base.RemoveFromReserveAmmunition(ammunitionToRestore);

                base.ServerReloadConditionsComplete();
            }

            _reloadCoroutine = null;
        }

        /// <summary>
        /// Performs a reload.
        /// </summary>
        public override void Reload(NetworkRoles networkRoles, bool instant)
        {
            base.Reload(networkRoles, instant);

            StopReload();
            _reloadCoroutine = StartCoroutine(__Reload(networkRoles, instant));
        }

        /// <summary>
        /// Received on the owner after server has confirmed a reload finished.
        /// </summary>
        public override void ConfirmReload()
        {
            //Get amount to restore.
            int requiredAmmunition = ReturnClipSize() - ReturnClipRemaining();
            int ammunitionToRestore = Mathf.Min(requiredAmmunition, ReturnReserveAmmunitionRemaining());
            //Add to clip and remove from reserve.
            _currentClipAmmunition += ammunitionToRestore;
            _currentReserveAmmunition -= ammunitionToRestore;
            //Make base calls.
            base.AddToCurrentClip(ammunitionToRestore);
            base.RemoveFromReserveAmmunition(ammunitionToRestore);

            base.ConfirmReload();
        }

        /// <summary>
        /// Returns the multiplier to use with the next camera recoil. Should be called before firing.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public override float ReturnCameraRecoilMultiplier() 
        {
            float recoilReduction = (Time.time - _lastFireTime) * _recoilConfiguration.SettleRate;
            _currentRecoilMultiplier = Mathf.Clamp(_currentRecoilMultiplier - recoilReduction, 1f, _recoilConfiguration.MaximumRecoilMultiplier);
            return _currentRecoilMultiplier; 
        }

        /// <summary>
        /// Returns the ShakeData to use for model recoil.
        /// </summary>
        /// <returns></returns>
        public override ShakeData ReturnModelRecoilShake() { return _modelRecoilShake; }
        /// <summary>
        /// Returns the ShakeData to use for camera recoil.
        /// </summary>
        /// <returns></returns>
        public override ShakeData ReturnCameraRecoilShake() { return _cameraRecoilShake; }

        /// <summary>
        /// Returns if this weapon is empty of all ammunition.
        /// </summary>
        /// <returns></returns>
        public override bool IsAmmunitionEmpty()
        {
            return (_currentReserveAmmunition <= 0 && _currentClipAmmunition <= 0);
        }

        /// <summary>
        /// Returns if this weapon's current clip is empty of ammunition.
        /// </summary>
        /// <returns></returns>
        public override bool IsClipEmpty()
        {
            return (_currentClipAmmunition <= 0);
        }

        /// <summary>
        /// Returns how much ammunition the clip can old.
        /// </summary>
        public override int ReturnClipSize()
        {
            return _clipSize;
        }

        /// <summary>
        /// Returns the amount of ammunition the reserve can hold.
        /// </summary>
        public override int ReturnReserveSize()
        {
            return _maximumReverseAmmunition;
        }

        /// <summary>
        /// Returns the ammunition left in the clip.
        /// </summary>
        /// <returns></returns>
        public override int ReturnClipRemaining()
        {
            return _currentClipAmmunition;
        }

        /// <summary>
        /// Returns the total ammunition left in the weapon.
        /// </summary>
        /// <returns></returns>
        public override int ReturnReserveAmmunitionRemaining()
        {
            return _currentReserveAmmunition;
        }

        /// <summary>
        /// Returns how long it takes to reload.
        /// </summary>
        /// <returns></returns>
        public override float ReturnReloadDuration()
        {
            return _reloadDuration;
        }

        /// <summary>
        /// Adds to the reserve ammunition.
        /// </summary>
        /// <param name="value"></param>
        public override void AddToCurrentReserve(int value)
        {
            _currentReserveAmmunition = Mathf.Min(_currentReserveAmmunition + value, _maximumReverseAmmunition);
            base.AddToCurrentReserve(value);
        }
    }


}