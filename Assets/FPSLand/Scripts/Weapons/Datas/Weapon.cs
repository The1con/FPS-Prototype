using FirstGearGames.FPSLand.Managers.Gameplay;
using FirstGearGames.FPSLand.Network;
using FirstGearGames.Managers.Global;
using FishNet.Managing.Timing;
using GameKit.CameraShakers;
using GameKit.Utilities;
using System;
using UnityEngine;

namespace FirstGearGames.FPSLand.Weapons
{

    public class Weapon : MonoBehaviour
    {
        #region Public.
        /// <summary>
        /// Dispatched when ammunition in the clip changes.
        /// </summary>
        public event Action<int, Weapon> OnClipRemainingChanged;
        /// <summary>
        /// Dispatched when reserve ammunition on the weapon changes.
        /// </summary>
        public event Action<int, Weapon> OnReserveAmmunitionChanged;
        /// <summary>
        /// Dispatched whenever fire must instantiate an object.
        /// </summary>
        public event EventHandler<ThrownInstantiateEventArgs> OnThrownInstantiateRequired;
        /// <summary>
        /// Called on the server when reload conditions complete. Called before ConfirmReload.
        /// </summary>
        public event Action<Weapon> OnServerReloadConditionsComplete;
        /// <summary>
        /// Called on the server after a reload has finished in full on the server.
        /// </summary>
        public event Action<Weapon> OnServerConfirmReload;
        /// <summary>
        /// True if this weapon is in inventory.
        /// </summary>
        public bool InInventory { get; private set; } = false;
        #endregion

        #region Serialized.
        [Header("Weapon")]
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Weapon name.")]
        [SerializeField]
        private WeaponNames _weaponName;
        /// <summary>
        /// Weapon name.
        /// </summary>
        public WeaponNames WeaponName { get { return _weaponName; } }
        /// <summary>
        /// Type of weapon. Used to determine action mechanics.
        /// </summary>
        [Tooltip("Type of weapon. Used to determine action mechanics.")]
        [SerializeField]
        private WeaponTypes _weaponType = WeaponTypes.Raycast;
        /// <summary>
        /// Type of weapon. Used to determine action mechanics.
        /// </summary>
        public WeaponTypes WeaponType { get { return _weaponType; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("How much to modify motor move rate by while holding this weapon.")]
        [Range(0.1f, 2f)]
        [SerializeField]
        private float _moveRateModifier = 1f;
        /// <summary>
        /// How much to modify motor move rate by while holding this weapon.
        /// </summary>
        public float MoveRateModifier { get { return _moveRateModifier; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Index of weapon for the animator.")]
        [SerializeField]
        private int _animatorIndex = 0;
        /// <summary>
        /// Index of weapon for the animator.
        /// </summary>
        public int AnimatorIndex { get { return _animatorIndex; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Which number key equips this weapon.")]
        [SerializeField]
        private int _equipNumber = 0;
        /// <summary>
        /// Which number key equips this weapon.
        /// </summary>
        public int EquipNumber { get { return _equipNumber; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("How long until Fire can be called after this weapon is equipped.")]
        [SerializeField]
        private float _equipDelay = 0.5f;
        /// <summary>
        /// How long until Fire can be called after this weapon is equipped.
        /// </summary>
        public float EquipDelay { get { return _equipDelay; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("How often this weapon may fire.")]
        [SerializeField]
        private float _fireRate = 0.25f;
        /// <summary>
        /// How often this weapon may fire.
        /// </summary>
        public float FireRate { get { return _fireRate; } }
        /// <summary>
        /// Amount of damage this weapon deals.
        /// </summary>
        [Tooltip("Amount of damage this weapon deals.")]
        [SerializeField]
        private int _damage = 10;
        /// <summary>
        /// Amount of damage this weapon deals.
        /// </summary>
        public int Damage { get { return _damage; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("True if this weapon can be equipped while empty or out of ammo.")]
        [SerializeField]
        private bool _canEquipEmpty = true;
        /// <summary>
        /// True if this weapon can be equipped while empty or out of ammo.
        /// </summary>
        public bool CanEquipEmpty { get { return _canEquipEmpty; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("How long to wait before dropping this weapon after it can no longer be equipped.")]
        [SerializeField]
        private float _automaticDropDelay = 0.2f;
        /// <summary>
        /// How long to wait before dropping this weapon after it can no longer be equipped.
        /// </summary>
        public float AutomaticDropDelay { get { return _automaticDropDelay; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("True if an automatic weapon.")]
        [SerializeField]
        private bool _automatic = false;
        /// <summary>
        /// True if an automatic weapon.
        /// </summary>
        public bool Automatic { get { return _automatic; } }
        /// <summary>
        /// First person object in hierarchy for this weapon.
        /// </summary>
        [Tooltip("First person object in hierarchy for this weapon.")]
        [SerializeField]
        protected WeaponModel FirstPersonModel;
        /// <summary>
        /// Third person object in hierarchy for this weapon.
        /// </summary>
        [Tooltip("Third person object in hierarchy for this weapon.")]
        [SerializeField]
        protected WeaponModel ThirdPersonModel;
        /// <summary>
        /// Prefab to spawn when this weapon impacts something other than a player.
        /// </summary>
        [Tooltip("Prefab to spawn when this weapon impacts something other than a player.")]
        [SerializeField]
        protected GameObject TerrainImpactPrefab;
        /// <summary>
        /// Prefab to spawn when this weapon impacts a player.
        /// </summary>
        [Tooltip("Prefab to spawn when this weapon impacts a player.")]
        [SerializeField]
        protected GameObject CharacterImpactPrefab;
        /// <summary>
        /// Audio prefab to spawn when weapon is fired.
        /// </summary>
        [Tooltip("Audio prefab to spawn when weapon is fired.")]
        [SerializeField]
        protected GameObject FireAudioPrefab;
        /// <summary>
        /// Audio prefab to spawn when weapon impacts something other than a character.
        /// </summary>
        [Tooltip("Audio prefab to spawn when weapon impacts something other than a character.")]
        [SerializeField]
        protected GameObject TerrainImpactAudioPrefab;
        /// <summary>
        /// Audio prefab to spawn when weapon impacts a character.
        /// </summary>
        [Tooltip("Audio prefab to spawn when weapon impacts a character.")]
        [SerializeField]
        protected GameObject CharacterImpactAudioPrefab;
        #endregion

        #region Const.
        /// <summary>
        /// Maximum amount of latency to compensate for with thrown item.
        /// When a client throws a weapon the object is sped up based on the clients send tick.
        /// To prevent unreliable gameplay or artifacts the latency restriction will only consider
        /// up this value in milliseconds for the clients time difference.
        /// </summary>
        public const float MAXIMUM_LATENCY_COMPENSATION = 0.1f;
        #endregion

        /// <summary>
        /// Sets the weapon's equipped state.
        /// </summary>
        /// <param name="equipped"></param>
        /// <param name="owner">True if owner.</param>sd
        public virtual void SetEquipped(bool equipped, NetworkRoles networkRoles)
        {
            bool owner = networkRoles.Contains(NetworkRoles.Owner);
            //Show/hide first and third person models so right weapon is shown on death.
            //if (owner)
            FirstPersonModel.gameObject.SetActive(equipped);
            //else
            ThirdPersonModel.gameObject.SetActive(equipped);

            //Send ammunition events so that UIs update.
            if (owner)
            {
                OnClipRemainingChanged?.Invoke(ReturnClipRemaining(), this);
                OnReserveAmmunitionChanged?.Invoke(ReturnReserveAmmunitionRemaining(), this);
            }
        }

        /// <summary>
        /// Invokes the event to spawn a fire prefab.
        /// </summary>
        /// <param name="serverOnly"></param>
        /// <param name="prefab"></param>
        protected void ThrownInstantiateRequired(ThrownInstantiateEventArgs args)
        {
            OnThrownInstantiateRequired?.Invoke(this, args);
        }

        /// <summary>
        /// Adds this weapon to character inventory.
        /// </summary>
        public virtual void AddToInventory()
        {
            InInventory = true;
        }

        /// <summary>
        /// Removes this weapon from character inventory.
        /// </summary>
        public virtual void RemoveFromInventory()
        {
            InInventory = false;
        }

        /// <summary>
        /// Called when this weapon fires.
        /// </summary>
        public virtual void Fire(PreciseTick pt, Vector3 position, Vector3 direction, NetworkRoles networkRoles)
        {
            if (FireAudioPrefab != null && networkRoles != NetworkRoles.Server)
            {
                //Owner, spawn on first person camera.
                if (networkRoles.Contains(NetworkRoles.Owner))
                    OfflineGameplayDependencies.AudioManager.PlayFirstPerson(FireAudioPrefab);
                //Not owner, spawn at position.
                else
                    OfflineGameplayDependencies.AudioManager.PlayAtPoint(FireAudioPrefab, position);
            }
        }

        /// <summary>
        /// Called when this weapon hits using a cast.
        /// </summary>
        /// <param name="hit"></param>
        public virtual void RayImpact(RaycastHit hit, bool serverOnly)
        {
            GameObject prefab = (GlobalManager.LayerManager.InLayerMask(hit.collider.gameObject, GlobalManager.LayerManager.HitboxLayer))
                ? CharacterImpactAudioPrefab : TerrainImpactAudioPrefab;
            if (prefab != null && !serverOnly)
                OfflineGameplayDependencies.AudioManager.PlayAtPoint(prefab, hit.point);
        }

        /// <summary>
        /// Called when this weapon hits using an overlap.
        /// </summary>
        /// <param name="hit"></param>
        public virtual void OverlapImpact(Vector3 position, Vector3 direction, Collider other, NetworkRoles networkRoles) { }

        /// <summary>
        /// Returns the multiplier to use with the next camera recoil. Should be called before firing.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns></returns>
        public virtual float ReturnCameraRecoilMultiplier() { return 1f; }
        /// <summary>
        /// Returns the ShakeData to use for model recoil.
        /// </summary>
        /// <returns></returns>
        public virtual ShakeData ReturnModelRecoilShake() { return null; }
        /// <summary>
        /// Returns the ShakeData to use for camera recoil.
        /// </summary>
        /// <returns></returns>
        public virtual ShakeData ReturnCameraRecoilShake() { return null; }

        /// <summary>
        /// Returns if this weapon is empty of all ammunition.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAmmunitionEmpty() { return false; }
        /// <summary>
        /// Returns if this weapon's current clip is empty of ammunition.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsClipEmpty() { return false; }
        /// <summary>
        /// Returns the amount of ammunition the clip can hold.
        /// </summary>
        public virtual int ReturnClipSize() { return 0; }
        /// <summary>
        /// Returns the amount of ammunition the reserve can hold.
        /// </summary>
        public virtual int ReturnReserveSize() { return 0; }
        /// <summary>
        /// Returns the current ammunition left in the clip.
        /// </summary>
        /// <returns></returns>
        public virtual int ReturnClipRemaining() { return 0; }
        /// <summary>
        /// Returns the reserve ammunition left in the weapon.
        /// </summary>
        /// <returns></returns>
        public virtual int ReturnReserveAmmunitionRemaining() { return 0; }

        /// <summary>
        /// Performs a reload.
        /// </summary>
        public virtual void Reload(NetworkRoles networkRoles, bool instant) { }
        /// <summary>
        /// Received on the owner after server has confirmed a reload finished.
        /// </summary>
        public virtual void ConfirmReload()
        {
            OnServerConfirmReload?.Invoke(this);
        }
        /// <summary>
        /// Removes a specified amount of ammunition from the current clip.
        /// </summary>
        /// <param name="value"></param>
        protected virtual void RemoveFromCurrentClip(int value)
        {
            OnClipRemainingChanged?.Invoke(ReturnClipRemaining(), this);
        }
        /// <summary>
        /// Adds a specified amount of ammunition to the current clip.
        /// </summary>
        /// <param name="value"></param>
        protected virtual void AddToCurrentClip(int value)
        {
            OnClipRemainingChanged?.Invoke(ReturnClipRemaining(), this);
        }
        /// <summary>
        /// Removes a specified amount of ammunition from reserve ammunition.
        /// </summary>
        /// <param name="value"></param>
        protected virtual void RemoveFromReserveAmmunition(int value)
        {
            OnReserveAmmunitionChanged?.Invoke(ReturnReserveAmmunitionRemaining(), this);
        }

        /// <summary>
        /// Adds to the reserve ammunition.
        /// </summary>
        /// <param name="value"></param>
        public virtual void AddToCurrentReserve(int value)
        {
            OnReserveAmmunitionChanged?.Invoke(ReturnReserveAmmunitionRemaining(), this);
        }

        /// <summary>
        /// Called on the server when reload conditions complete. Called before ConfirmReload.
        /// </summary>
        public virtual void ServerReloadConditionsComplete()
        {
            OnServerReloadConditionsComplete?.Invoke(this);
        }
        /// <summary>
        /// Returns how long it takes to reload.
        /// </summary>
        /// <returns></returns>
        public virtual float ReturnReloadDuration() { return -1f; }

        /// <summary>
        /// Returns distance of cast to use for melee attacks.
        /// </summary>
        /// <returns></returns>
        public virtual float ReturnMeleeDistance() { return 1f; }
        /// <summary>
        /// Returns radius of cast to use for melee attacks.
        /// </summary>
        /// <returns></returns>
        public virtual float ReturnMeleeRadius() { return 0.5f; }
    }

}