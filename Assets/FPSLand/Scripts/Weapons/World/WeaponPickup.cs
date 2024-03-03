using FirstGearGames.FPSLand.Characters.Weapons;
using FirstGearGames.FPSLand.Managers.Gameplay;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using UnityEngine;

namespace FirstGearGames.FPSLand.Weapons
{
    public class WeaponPickup : NetworkBehaviour
    {
        #region Serialized.
        /// <summary>
        /// Weapon this pickup is for.
        /// </summary>
        [Tooltip("Weapon this pickup is for.")]
        [SerializeField]
        private WeaponNames _weapon;
        /// <summary>
        /// How much of weapon to give.
        /// </summary>
        [Tooltip("How much of weapon to give.")]
        [SerializeField]
        private int _quantity = 1;
        /// <summary>
        /// True to also add this item to the characters inventory if it does not already exist.
        /// </summary>
        [Tooltip("True to also add this item to the characters inventory if it does not already exist.")]
        [SerializeField]
        private bool _addToInventory = true;
        /// <summary>
        /// How long until this pickup returns.
        /// </summary>
        [Tooltip("How long until this pickup returns.")]
        [SerializeField]
        private float _respawnDelay = 60f;
        /// <summary>
        /// Object to rotate for visual effects. This may be null.
        /// </summary>
        [Tooltip("Object to rotate for visual effects. This may be null.")]
        [SerializeField]
        private GameObject _rotatingObject;
        /// <summary>
        /// Audio to play when picked up.
        /// </summary>
        [Tooltip("Audio to play when picked up.")]
        [SerializeField]
        private GameObject _pickupAudioPrefab;
        #endregion

        #region Private.
        /// <summary>
        /// Next time to spawn this weapon.
        /// </summary>
        private float _nextSpawnTime = -1f;
        /// <summary>
        /// True if pickup is active.
        /// </summary>
        private bool _active = true;
        #endregion

        #region Const.
        /// <summary>
        /// How quickly to rotate RotatingObject.
        /// </summary>
        private const float ROTATE_RATE = 90f;
        #endregion

        private void Update()
        {
            if (base.IsClient)
            {
                Rotate();
            }
            if (base.IsServer)
            {
                CheckRespawn();
            }
        }

        /// <summary>
        /// Rotates RotatingObject.
        /// </summary>
        [Client(Logging = LoggingType.Off)]
        private void Rotate()
        {
            if (_rotatingObject == null)
                return;

            _rotatingObject.transform.Rotate(new Vector3(0f, 0f, ROTATE_RATE * Time.deltaTime));
        }


        /// <summary>
        /// Checks if this object needs to be respawned.
        /// </summary>
        [Server(Logging = LoggingType.Off)]
        private void CheckRespawn()
        {
            if (_nextSpawnTime == -1f)
                return;
            if (Time.time < _nextSpawnTime)
                return;

            //Unset next spawn time.
            _nextSpawnTime = -1f;
            //Enable pickup.
            SetActive(null, true, false);
        }

        /// <summary>
        /// Called when a player enters the pickup.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter(Collider other)
        {
            if (!_active)
                return;

            if (base.IsServer)
            {
                WeaponHandler wh = other.GetComponent<WeaponHandler>();
                if (wh != null)
                {
                    int added = wh.AddToWeaponReserve(new WeaponReserveData(_weapon, _quantity, _addToInventory));
                    //If was consumed then unspawn and set respawn.
                    if (added > 0)
                    {
                        SetActive(wh.NetworkObject.Owner, false, true);
                        _nextSpawnTime = Time.time + _respawnDelay;
                    }
                }
            }
        }

        /// <summary>
        /// Sets active state on visual.
        /// </summary>
        /// <param name="active"></param>
        private void SetActive(NetworkConnection conn, bool active, bool playAudio)
        {
            _rotatingObject.SetActive(active);
            _active = active;

            //Only play audio if a client.
            if (conn != null && playAudio && !base.IsServerOnly)
            {
                //If connection id is self then play on camera.
                if (conn.IsLocalClient)
                    OfflineGameplayDependencies.AudioManager.PlayFirstPerson(_pickupAudioPrefab);
                else
                    OfflineGameplayDependencies.AudioManager.PlayAtPoint(_pickupAudioPrefab, transform.position);
            }

            if (base.IsServer)
                ObserversSetActive(conn, active, playAudio);
        }

        /// <summary>
        /// Sets active state on visual.
        /// </summary>
        /// <param name="active"></param>
        [ObserversRpc(BufferLast = true)]
        private void ObserversSetActive(NetworkConnection conn, bool active, bool playAudio)
        {
            //Already ran on server.
            if (base.IsServer)
                return;

            SetActive(conn, active, playAudio);
        }
    }


}