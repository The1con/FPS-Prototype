using FirstGearGames.FPSLand.Network;
using FishNet.Managing.Timing;
using GameKit.Utilities;
using System.Collections;
using UnityEngine;

namespace FirstGearGames.FPSLand.Weapons
{

    public class Thrown : Weapon
    {
        #region Serialized.
        [Header("Thrown")]
        /// <summary>
        /// Prefab of thrown object.
        /// </summary>
        [Tooltip("Prefab of thrown object.")]
        [SerializeField]
        private GameObject _thrownPrefab;
        /// <summary>
        /// True to instantiate the ThrownPrefab only on the server.
        /// </summary>
        [Tooltip("True to instantiate the ThrownPrefab only on the server.")]
        [SerializeField]
        private bool _instantiateOnServerOnly = true;
        /// <summary>
        /// How much force while aiming up to apply towards thrown when instantiated.
        /// </summary>
        [Tooltip("How much force while aiming up to apply towards thrown when instantiated.")]
        [SerializeField]
        private float _upForce = 10f;
        /// <summary>
        /// How much force while aiming down to apply towards thrown when instantiated.
        /// </summary>
        [Tooltip("How much force while aiming down to apply towards thrown when instantiated.")]
        [SerializeField]
        private float _downForce = 10f;
        /// <summary>
        /// Extra height power to add onto the fire direction. This creates a lob effect rather than firing straight out.
        /// </summary>
        [Tooltip("Extra height power to add onto the fire direction. This creates a lob effect rather than firing straight out.")]
        [Range(0f, 1f)]
        public float _lobHeight = 0.25f;
        /// <summary>
        /// How long to wait after firing before hiding the weapon.
        /// </summary>
        [Tooltip("How long to wait after firing before hiding the weapon.")]
        [SerializeField]
        private float _hideDelay = 0.3f;
        /// <summary>
        /// After being hidden how long to wait before showing the weapon again.
        /// </summary>
        [Tooltip("After being hidden how long to wait before showing the weapon again.")]
        [SerializeField]
        private float _reshowDelay = 0.2f;
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Maximum reserve ammunition this weapon can hold.")]
        [SerializeField]
        private int _maximumReverseAmmunition = 90;
        #endregion

        #region Private.
        /// <summary>
        /// Current amount of ammunition in the clip.
        /// </summary>
        private int _currentClipAmmunition;
        /// <summary>
        /// Current amount of ammunition remaining in reserve.
        /// </summary>
        private int _currentReserveAmmunition;
        /// <summary>
        /// Coroutine used to hide and reshow weapon.
        /// </summary>
        private Coroutine _graphicUpdateCoroutine = null;
        #endregion

        /// <summary>
        /// Resets the weapon as though it's not been used.
        /// </summary>
        private void Reset()
        {
            _currentClipAmmunition = 0;
            _currentReserveAmmunition = 0;
        }

        /// <summary>
        /// Sets the weapon's equipped state.
        /// </summary>
        /// <param name="equipped"></param>
        /// <param name="owner">True if owner.</param>
        public override void SetEquipped(bool equipped, NetworkRoles networkRoles)
        {
            base.SetEquipped(equipped, networkRoles);
            if (!equipped)
                StopGraphicUpdate();
        }

        /// <summary>
        /// Called when this weapon fires.
        /// </summary>
        public override void Fire(PreciseTick pt, Vector3 position, Vector3 direction, NetworkRoles networkRoles)
        {
            //Add extra height to the direction.
            direction = (direction + new Vector3(0f, _lobHeight, 0f)).normalized;
            bool serverOnly = (networkRoles == NetworkRoles.Server);
            bool owner = networkRoles.Contains(NetworkRoles.Owner);
            base.Fire(pt, position, direction, networkRoles);
            //Don't need to show this if server only.
            if (!serverOnly)
            {
                StopGraphicUpdate();
                _graphicUpdateCoroutine = StartCoroutine(__GraphicUpdate(owner));
            }

            float lerpPercent = Mathf.InverseLerp(-1f, 1f, direction.y);
            float force = Mathf.Lerp(_downForce, _upForce, lerpPercent);
            //Causes instantiation of thrown prefab.
            base.ThrownInstantiateRequired(new ThrownInstantiateEventArgs(this, pt, position, direction, force, _instantiateOnServerOnly, _thrownPrefab));

            RemoveFromCurrentClip(1);
            Reload(networkRoles, true);
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
        /// Called when a reload is interrupted.
        /// </summary>
        private void StopGraphicUpdate()
        {
            if (_graphicUpdateCoroutine != null)
            {
                StopCoroutine(_graphicUpdateCoroutine);
                _graphicUpdateCoroutine = null;
            }
        }

        /// <summary>
        /// Hides the thrown, then re-shows it.
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        private IEnumerator __GraphicUpdate(bool owner)
        {
            //Get model to show/hide.
            GameObject model = (owner) ? base.FirstPersonModel.gameObject : base.ThirdPersonModel.gameObject;

            //Hide.
            yield return new WaitForSeconds(_hideDelay);
            model.SetActive(false);

            //Show if not out of ammunition.
            if (!IsAmmunitionEmpty())
            {
                yield return new WaitForSeconds(_reshowDelay);
                model.SetActive(true);
            }

            _graphicUpdateCoroutine = null;
        }

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
            return 1;
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
        /// Returns the amount of ammunition the reserve can hold.
        /// </summary>
        public override int ReturnReserveSize()
        {
            return _maximumReverseAmmunition;
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
        /// Adds to the reserve ammunition.
        /// </summary>
        /// <param name="value"></param>
        public override void AddToCurrentReserve(int value)
        {
            _currentReserveAmmunition = Mathf.Min(_currentReserveAmmunition + value, _maximumReverseAmmunition);
            base.AddToCurrentReserve(value);
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
        /// Reloads this weapon using settings.
        /// </summary>
        /// <returns></returns>
        public override void Reload(NetworkRoles networkRoles, bool instant)
        {
            base.Reload(networkRoles, instant);

            //If server confirm reload immediately.
            if (networkRoles.Contains(NetworkRoles.Server))
                base.ServerReloadConditionsComplete();
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

    }


}