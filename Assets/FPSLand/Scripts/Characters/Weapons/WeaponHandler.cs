using FirstGearGames.FPSLand.Characters.Bodies;
using FirstGearGames.FPSLand.Characters.Motors;
using FirstGearGames.FPSLand.Characters.Vitals;
using FirstGearGames.FPSLand.Weapons;
using FirstGearGames.Managers.Global;
using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Managing.Timing;
using FishNet.Serializing.Helping;
using FirstGearGames.FPSLand.Network;
using FishNet.Component.ColliderRollback;
using GameKit.Utilities;
using GameKit.CameraShakers;

namespace FirstGearGames.FPSLand.Characters.Weapons
{


    public class WeaponHandler : NetworkBehaviour
    {

        #region Public.
        /// <summary>
        /// Dispatched when ammunition in the clip changes for the current weapon.
        /// </summary>
        public event Action<int> OnClipRemainingChanged;
        /// <summary>
        /// Dispatched whene total ammunition of the current weapon changes.
        /// </summary>
        public event Action<int> OnTotalAmmunitionChanged;
        /// <summary>
        /// Dispatched when a weapon is added to inventory.
        /// </summary>
        public event Action<WeaponNames> OnWeaponAddedToInventory;
        /// <summary>
        /// Dispatched when a weapon is removed from inventory.
        /// </summary>
        public event Action<WeaponNames> OnWeaponRemovedFromInventory;
        /// <summary>
        /// Dispatched when weapon is equipped.
        /// </summary>
        public event Action<WeaponNames> OnWeaponEquipped;
        /// <summary>
        /// Accessor for the current weapon. This does not consider out of range.
        /// </summary>
        public Weapon Weapon
        {
            get
            {
                if (WeaponIndexValid())
                    return Weapons[_weaponIndex];
                else
                    return null;
            }
        }
        #endregion

        #region Serialized.
        /// <summary>
        /// Transform which holds weapon data.
        /// </summary>
        [Tooltip("Transform which holds weapon data.")]
        [SerializeField]
        private GameObject _weaponsParent;
        #endregion

        #region Private.
        /// <summary>
        /// Health for this character.
        /// </summary>
        private Health _health;
        /// <summary>
        /// Weapons found under the WeaponsParent.
        /// </summary>
        public Weapon[] Weapons { get; private set; }
        /// <summary>
        /// Current weapon equipped.
        /// </summary>
        private int _weaponIndex = -1;
        /// <summary>
        /// Last time a weapon was fired. This is reset when a weapon switch occurs.
        /// </summary>
        private float _nextFireTime = 0f;
        /// <summary>
        /// Transform of the camera for first person.
        /// </summary>
        private Transform _cameraTransform;
        /// <summary>
        /// Set the transform of the first person camera.
        /// </summary>
        /// <param name="t"></param>
        public void SetCameraTransform(Transform t)
        {
            _cameraRecoilShaker = t.GetComponent<ObjectShaker>();
            _cameraTransform = t;
        }
        /// <summary>
        /// ObjectShaker used to create camera recoil.
        /// </summary>
        private ObjectShaker _cameraRecoilShaker;
        /// <summary>
        /// RecoilIK for the perspective being used.
        /// </summary>
        private RecoilIK _recoilIK;
        /// <summary>
        /// If not -1 this holds a weapon change to send over the server.
        /// </summary>
        private int _queuedWeaponChangeIndex = -1;
        /// <summary>
        /// True if a reload of current weapon is queued to occur.
        /// </summary>
        private bool _reloadQueued;
        /// <summary>
        /// Animator controller on this object.
        /// </summary>
        private AnimatorController _animatorController;
        /// <summary>
        /// Next time player can switch weapons via mouse wheel. Used to prevent accidental overshooting.
        /// </summary>
        private float _nextWheelWeaponSwitchTime = 0f;
        /// <summary>
        /// Time when to switch to the first available weapon. Not enabled if value is -1f.
        /// </summary>
        private float _selectFirstAvailableWeaponTime = -1f;
        #endregion

        #region Const.
        /// <summary>
        /// Weapons the player starts with.
        /// </summary>
        private const WeaponNames STARTING_WEAPONS = (WeaponNames.M4A1 | WeaponNames.Glock | WeaponNames.Knife);
        #endregion

        private void Awake()
        {
            FirstInitialize();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            NetworkInitialize(true);
        }

        public override void OnSpawnServer(NetworkConnection connection)
        {
            base.OnSpawnServer(connection);
            ObjectSpawnedForPlayer(connection);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            NetworkInitialize(false);
        }

        private void Update()
        {
            if (base.IsOwner)
            {
                CheckQueueSwitchWeapon();
                CheckQueueReloadWeapon();
                CheckFire();
            }
        }

        private void TimeManager_OnTick()
        {
            if (base.IsOwner)
            {
                SendWeaponChange();
                SendWeaponReload();
            }
        }

        /// <summary>
        /// Initializes this script for use. Should only be completed once.
        /// </summary>
        private void FirstInitialize()
        {
            _animatorController = GetComponent<AnimatorController>();
            Weapons = _weaponsParent.GetComponentsInChildren<Weapon>();
            _health = GetComponent<Health>();
        }

        /// <summary>
        /// Initializes this script for use. Should only be completed once.
        /// </summary>
        private void NetworkInitialize(bool asServer)
        {
            /* The player object is never despawned. It becomes disabled
             * and re-enabled when 'respawning', but it never leaves the network.
             * If it were to despawn the event subscriptions below would
             * have to be undone in OnStopNetwork. */
            bool isServer = base.IsServer;
            bool isOwner = base.IsOwner;

            if (isServer)
            {
                //Subscribe to reload events on all weapons so that reloads may occur on weapons which aren't equipped.
                for (int i = 0; i < Weapons.Length; i++)
                {
                    Weapons[i].OnServerReloadConditionsComplete += Weapon_OnServerReloadConditionsComplete;
                    Weapons[i].OnServerConfirmReload += Weapon_OnServerConfirmReload;
                }
            }

            //Gives standard weapons to character. Run on client and server.
            AddDefaultWeaponsToInventory();

            //Needed by owner and server.
            if (isOwner || isServer)
            {
                //Subscribe only if serveronly or clientonly, or if client host + initializing as server.
                _health.OnDeath += Health_OnDeath;
                _health.OnRespawned += Health_OnRespawned;
            }

            //If a client get the recoil script.
            if (!asServer)
            {
                BodiesConfigurations bc = GetComponent<BodiesConfigurations>();
                GameObject recoilObj = (isOwner) ? bc.FirstPersonObject : bc.ThirdPersonObject;
                _recoilIK = recoilObj.GetComponent<RecoilIK>();

                if (isOwner)
                {
                    //Subscribe to ticks to send weapon related changes such as switching, and reloading.
                    base.TimeManager.OnTick += TimeManager_OnTick;
                    //Set to 0f to select a weapon next frame.
                    _selectFirstAvailableWeaponTime = 0f;
                }
            }
        }

        /// <summary>
        /// Received after a client has it's player instantiated.
        /// </summary>
        private void ObjectSpawnedForPlayer(NetworkConnection conn)
        {
            //Weapons to add to reserves for this player.
            List<WeaponReserveData> weaponReserveDatas = new List<WeaponReserveData>();
            //Sync items which are not normally present.
            foreach (Weapon w in Weapons)
            {
                //If not a starting weapon see if this player has it.
                if (!STARTING_WEAPONS.Contains(w.WeaponName))
                {
                    if (w.InInventory)
                    {
                        WeaponReserveData wrd = new WeaponReserveData
                        {
                            WeaponName = w.WeaponName,
                            Quantity = w.ReturnClipRemaining() + w.ReturnReserveAmmunitionRemaining(),
                            AddToInventory = true
                        };
                        weaponReserveDatas.Add(wrd);
                    }
                }
            }

            //If need to send new weapons.
            if (weaponReserveDatas.Count > 0)
                TargetAddToWeaponReserve(conn, weaponReserveDatas.ToArray());

            //This doesn't need to be sent to the owner, they're not a spectator.
            if (conn != base.Owner && WeaponIndexValid())
                TargetSpectatorEquipWeapon(conn, _weaponIndex);
        }

        /// <summary>
        /// Received when the character is respawned.
        /// </summary>
        private void Health_OnRespawned()
        {
            AddDefaultWeaponsToInventory();

            //Set to 0f to select a weapon next frame if client.
            if (base.IsClient)
                _selectFirstAvailableWeaponTime = 0f;

            this.enabled = true;
        }

        /// <summary>
        /// Received when the character is dead.
        /// </summary>
        private void Health_OnDeath()
        {
            RemoveWeaponsFromInventory();
            this.enabled = false;
        }

        /// <summary>
        /// Subscribes to subscribes to weapon events.
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="subscribe"></param>
        private void SubscribeToWeaponEvents(Weapon weapon, bool subscribe)
        {
            if (weapon == null)
                return;

            if (subscribe)
            {
                weapon.OnClipRemainingChanged += Weapon_OnClipRemainingChanged;
                weapon.OnReserveAmmunitionChanged += Weapon_OnTotalAmmunitionChanged;
                weapon.OnThrownInstantiateRequired += Weapon_OnThrownInstantiateRequired;
            }
            else
            {
                weapon.OnClipRemainingChanged -= Weapon_OnClipRemainingChanged;
                weapon.OnReserveAmmunitionChanged -= Weapon_OnTotalAmmunitionChanged;
                weapon.OnThrownInstantiateRequired -= Weapon_OnThrownInstantiateRequired;
            }
        }

        /// <summary>
        /// Received on the server when it completes a reload.
        /// </summary>
        /// <param name="obj"></param>
        private void Weapon_OnServerReloadConditionsComplete(Weapon weapon)
        {
            /* Doesn't matter if weapon index is valid on server
             * because server will always be right. But we do need
             * to get the index weapon is on to pass into a rpc. */
            weapon.ConfirmReload();
        }
        /// <summary>
        /// Received on the server after a reload has finished in full on the server.
        /// </summary>
        /// <param name="weapon"></param>
        private void Weapon_OnServerConfirmReload(Weapon weapon)
        {
            int index = ReturnWeaponIndex(weapon);
            if (index != -1)
                TargetConfirmReload(base.Owner, index);
        }

        /// <summary>
        /// Returns index in the weapon array for the specified Weapon.
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        private int ReturnWeaponIndex(Weapon weapon)
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                if (Weapons[i] == weapon)
                    return i;
            }

            //Fall through, not found.
            return -1;
        }
        /// <summary>
        /// Returns index in the weapon array for the specified WeaponNames.
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        private int ReturnWeaponIndex(WeaponNames weaponName)
        {
            for (int i = 0; i < Weapons.Length; i++)
            {
                if (Weapons[i].WeaponName == weaponName)
                    return i;
            }

            //Fall through, not found.
            return -1;
        }

        /// <summary>
        /// Confirm the reload completed to clients.
        /// </summary>
        /// <param name="index"></param>
        [TargetRpc]
        private void TargetConfirmReload(NetworkConnection conn, int index)
        {
            //Ignore if server as this already occured on server..
            if (base.IsServer)
                return;

            Weapons[index].ConfirmReload();
        }

        /// <summary>
        /// Received whenever an object must be instantiated for thrown weapons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Weapon_OnThrownInstantiateRequired(object sender, ThrownInstantiateEventArgs e)
        {
            if (e.ServerOnly && !base.IsServer)
                return;
            if (WeaponIndexValid() && e.Weapon != Weapon)
                return;

            GameObject result = Instantiate(e.Prefab, e.Position, Quaternion.identity);
            base.Spawn(result);

            //Initialized after spawn because IsServer has to be known to initialize.
            IThrowable throwable = result.GetComponent<IThrowable>();
            throwable.Initialize(e.PreciseTick, e.Direction * e.Force);
        }

        /// <summary>
        /// Received when ammunition in the clip changes for the current weapon.
        /// </summary>
        private void Weapon_OnTotalAmmunitionChanged(int ammunitionRemaining, Weapon weapon)
        {
            //Weapon index has changed since callback.
            if (WeaponIndexValid() && weapon != Weapon)
                return;
            OnTotalAmmunitionChanged?.Invoke(ammunitionRemaining);
        }
        /// <summary>
        /// Received when total ammunition on the current weapon changes.
        /// </summary>
        private void Weapon_OnClipRemainingChanged(int ammunitionRemaining, Weapon weapon)
        {
            //Weapon index has changed since callback.
            if (WeaponIndexValid() && weapon != Weapon)
                return;

            OnClipRemainingChanged?.Invoke(ammunitionRemaining);

            /* Make sure weapon doesn't need to be removed.
            * Weapon index should always be valid in this scenario, but just
            * in case. */
            if (WeaponIndexValid())
            {
                //If out of ammo and cannot equip empty remove from inventory.
                if (Weapon.IsAmmunitionEmpty() && !Weapon.CanEquipEmpty)
                {
                    //Remove from inventory immediately.
                    RemoveWeaponFromInventory(Weapon.WeaponName);
                    //Set time to select next weapon.
                    _selectFirstAvailableWeaponTime = Time.time + Weapon.AutomaticDropDelay;
                }
            }
        }

        /// <summary>
        /// Gives weapons to the character.
        /// </summary>
        private void AddDefaultWeaponsToInventory()
        {
            //Simulates picking up weapons.
            for (int i = 0; i < Weapons.Length; i++)
            {
                if (STARTING_WEAPONS.Contains(Weapons[i].WeaponName))
                    AddWeaponToInventory(i);
            }
        }

        /// <summary>
        /// Removes all weapons from the character.
        /// </summary>
        private void RemoveWeaponsFromInventory()
        {
            //Unset weapon index, changes, and reload
            UnsetWeaponIndex();
            _queuedWeaponChangeIndex = -1;
            _reloadQueued = false;

            for (int i = 0; i < Weapons.Length; i++)
            {
                Weapons[i].SetEquipped(false, ReturnNetworkRoles());
                RemoveWeaponFromInventory(i);
            }
        }

        /// <summary>
        /// Returns network roles for this character.
        /// </summary>
        /// <returns></returns>
        private NetworkRoles ReturnNetworkRoles()
        {
            NetworkRoles networkRoles = NetworkRoles.Unset;
            if (base.IsClient)
            {
                networkRoles |= NetworkRoles.Client;
                if (base.IsOwner)
                    networkRoles |= NetworkRoles.Owner;
            }
            if (base.IsServer)
                networkRoles |= NetworkRoles.Server;

            return networkRoles;
        }

        /// <summary>
        /// Unsets the current weapon index.
        /// </summary>
        private void UnsetWeaponIndex()
        {
            //Unsubscribe from old weapon.
            if (WeaponIndexValid())
                SubscribeToWeaponEvents(Weapon, false);

            _weaponIndex = -1;
        }

        /// <summary>
        /// Adds ammunition to a weapons reserve.
        /// </summary>
        public int AddToWeaponReserve(WeaponReserveData wrd)
        {
            WeaponNames weaponName = wrd.WeaponName;
            int quantity = wrd.Quantity;
            bool addToInventory = wrd.AddToInventory;

            int index = ReturnWeaponIndex(weaponName);
            if (index != -1)
            {
                bool inInventory = Weapons[index].InInventory;
                //Not added because not in inventory, and not to add to inventory.
                if (!addToInventory && !inInventory)
                    return -1;
                //Add to inventory as well.
                if (addToInventory && !inInventory)
                    AddWeaponToInventory(index);

                /* Check out how to add, then add it
                 * to current reserve. */
                int maxToAdd = (Weapons[index].ReturnReserveSize() - Weapons[index].ReturnReserveAmmunitionRemaining());
                int added = Mathf.Min(maxToAdd, quantity);
                //If any is to be added.
                if (added > 0)
                {
                    Weapons[index].AddToCurrentReserve(added);
                    //Add to clients reserve
                    if (base.IsServer)
                        ObserversAddToWeaponReserve(new WeaponReserveData(weaponName, quantity, addToInventory));
                    //After adding to reserve, reload instantly if clip empty.
                    if (Weapons[index].ReturnClipRemaining() == 0)
                        Weapons[index].Reload(ReturnNetworkRoles(), !inInventory);
                }

                return added;
            }
            //Weapon not found.
            else
            {
                return -1;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="weaponName"></param>
        /// <param name="quantity"></param>
        /// <param name="addToInventory"></param>
        [ObserversRpc]
        private void ObserversAddToWeaponReserve(WeaponReserveData wrd)
        {
            //Client host, server side already ran.
            if (base.IsServer && base.IsClient)
                return;

            AddToWeaponReserve(wrd);
        }

        /// <summary>
        /// 
        /// </summary>
        [TargetRpc]
        private void TargetAddToWeaponReserve(NetworkConnection conn, WeaponReserveData[] wrd)
        {
            //Client host, server side already ran.
            if (base.IsHost)
                return;

            for (int i = 0; i < wrd.Length; i++)
                AddToWeaponReserve(wrd[i]);
        }


        /// <summary>
        /// Adds a weapon to inventory using weapon name.
        /// </summary>
        /// <param name="weaponName">WeaponName to add.</param>
        /// <param name="quantity">Quantity of ammunition to add for weapon.</param>
        /// <returns>Quantity added to inventory.</returns>
        private void AddWeaponToInventory(WeaponNames weaponName)
        {
            int index = ReturnWeaponIndex(weaponName);
            AddWeaponToInventory(index);
        }
        /// <summary>
        /// Adds a weapon to inventory using an index.
        /// </summary>
        /// <param name="index"></param>
        private void AddWeaponToInventory(int index)
        {
            if (index < 0 || index >= Weapons.Length)
                return;

            Weapons[index].AddToInventory();
            OnWeaponAddedToInventory?.Invoke(Weapons[index].WeaponName);
        }

        /// <summary>
        /// Removes a weapon from inventory.
        /// </summary>
        /// <param name="weaponName"></param>
        private void RemoveWeaponFromInventory(WeaponNames weaponName)
        {
            int index = ReturnWeaponIndex(weaponName);
            RemoveWeaponFromInventory(index);
        }

        /// <summary>
        /// Removes a weapon from inventory.
        /// </summary>
        /// <param name="index"></param>
        private void RemoveWeaponFromInventory(int index)
        {
            if (index < 0 || index >= Weapons.Length)
                return;

            Weapons[index].RemoveFromInventory();
            OnWeaponRemovedFromInventory?.Invoke(Weapons[index].WeaponName);
        }

        /// <summary>
        /// Sends a weapon change if queued.
        /// </summary>
        [Client(Logging = LoggingType.Off)]
        private void SendWeaponChange()
        {
            if (_queuedWeaponChangeIndex == -1)
                return;

            CmdSendWeaponChange(_queuedWeaponChangeIndex);
            _queuedWeaponChangeIndex = -1;
        }
        /// <summary>
        /// Sends a weapon reloadif queued.
        /// </summary>
        [Client(Logging = LoggingType.Off)]
        private void SendWeaponReload()
        {
            if (!_reloadQueued)
                return;

            _reloadQueued = false;
            if (!CanReloadWeapon())
                return;

            ReloadWeapon();
            CmdSendWeaponReload();
        }



        /// <summary>
        /// Sends a weapon change to the server reliably.
        /// </summary>
        [ServerRpc]
        private void CmdSendWeaponChange(int index)
        {
            /* Weapon index is not valid. Player is likely trying to cheat.
             * Tell player to switch to their first valid weapon. */
            if (!WeaponIndexValid(index))
            {
                TargetSelectFirstValidWeapon(base.Owner);
            }
            //Index is valid. Inform others of the switch.
            else
            {
                bool switchSuccessful = false;
                //If server only, or client host without authority switch normally.
                if (base.IsServerOnly || (base.IsHost && !base.IsOwner))
                    switchSuccessful = EquipWeapon(index, false);
                //clientHost fix.
                else if (base.IsOwner)
                    switchSuccessful = EquipWeapon(index, false, true);

                if (switchSuccessful)
                    ObserversSpectatorEquipWeapon(index);
            }
        }

        /// <summary>
        /// Returns if a reload can occur for the current weapon.
        /// </summary>
        /// <returns></returns>
        private bool CanReloadWeapon()
        {
            if (!WeaponIndexValid())
                return false;
            //Doesn't support reload.
            if (Weapon.ReturnReloadDuration() == -1f)
                return false;
            //No ammunition is missing.
            if (Weapon.ReturnClipRemaining() >= Weapon.ReturnClipSize())
                return false;
            //No ammunition left to reload into clip.
            if (Weapon.ReturnReserveAmmunitionRemaining() == 0)
                return false;
            /* Fire time not met. Means character may have just fired
             * or are still bringing the weapon up. */
            if (!FireTimeMet())
                return false;

            return true;
        }

        /// <summary>
        /// Sends a weapon change to the server reliably.
        /// </summary>
        [ServerRpc]
        private void CmdSendWeaponReload()
        {
            //Don't reload if owner, clientHost side would have already.
            if (base.IsOwner)
                return;

            //If cannot reload tell owner reload failed.
            if (!CanReloadWeapon())
            {
                TargetReloadFailed(base.Owner);
            }
            else
            {
                ReloadWeapon();
                ObserversSpectatorReloadWeapon();
            }
        }


        /// <summary>
        /// Tells the character to select their first weapon.
        /// </summary>
        [TargetRpc]
        private void TargetSelectFirstValidWeapon(NetworkConnection conn)
        {
            SelectFirstValidWeapon(true);
        }

        /// <summary>
        /// Informs spectators that a weapon change has occurred for this character.
        /// </summary>
        /// <param name="index"></param>
        [ObserversRpc]
        private void ObserversSpectatorEquipWeapon(int index)
        {
            //If owner or server.
            if (base.IsOwner || base.IsServer)
                return;

            //Not a valid index, shouldn't happen, really bad.
            if (!WeaponIndexValid(index))
            {
                Debug.LogError("WeaponIndex is not valid.");
            }
            //Change to new index.
            else
            {
                EquipWeapon(index, false, true);
            }
        }

        /// <summary>
        /// Called on owner when server disallows their reload.
        /// </summary>
        [TargetRpc]
        private void TargetReloadFailed(NetworkConnection conn)
        {
            //Force out of the reload animation.
            _animatorController.SetWeaponIndex(_weaponIndex);
        }

        /// <summary>
        /// Informs spectators that a weapon reload has occurred for this character.
        /// </summary>
        /// <param name="index"></param>
        [ObserversRpc(ExcludeOwner = true)]
        private void ObserversSpectatorReloadWeapon()
        {
            /* Don't include server/owner because they would have already started reloading.
             * The same applies for the server, as they sent this rpc
             * after starting reload. */
            if (base.IsServer)
                return;

            //Only try to reload if the current weapon is valid. It should always be.
            if (WeaponIndexValid())
                ReloadWeapon();
        }

        /// <summary>
        /// Informs a specific spectator that a weapon change has occurred for this character.
        /// </summary>
        /// <param name="index"></param>
        [TargetRpc]
        private void TargetSpectatorEquipWeapon(NetworkConnection conn, int index)
        {
            //If owner or server.
            if (base.IsOwner || base.IsServer)
                return;

            //Not a valid index, shouldn't happen, really bad.
            if (!WeaponIndexValid(index))
            {
                Debug.LogError("WeaponIndex is not valid.");
            }
            //Change to new index.
            else
            {
                EquipWeapon(index, false, true);
            }
        }

        /// <summary>
        /// Sets the layer for Hitboxes.
        /// </summary>
        /// <param name="toIgnore">True to set to ignorePhysics, false to set to default.</param>
        private void SetHitboxesLayer(bool toIgnore)
        {
            int layer = (toIgnore) ?
                Layers.LayerMaskToLayerNumber(GlobalManager.LayerManager.IgnoreCollisionLayer) :
                Layers.LayerMaskToLayerNumber(GlobalManager.LayerManager.HitboxLayer);

            for (int i = 0; i < _health.Hitboxes.Length; i++)
                _health.Hitboxes[i].gameObject.layer = layer;
        }

        /// <summary>
        /// Checks if the client wants to and can fire.
        /// </summary>
        [Client(Logging = LoggingType.Off)]
        private void CheckFire()
        {
            if (!WeaponIndexValid())
                return;
            if (!AmmunitionAvailable())
                return;
            if (!FireTimeMet())
                return;

            //Semi automatic.
            if (!Weapon.Automatic)
            {
                //Mouse fire not pressed.
                if (!Input.GetKeyDown(KeyCode.Mouse0))
                    return;
            }
            //Automatic.
            else
            {
                //Mouse fire not pressed.
                if (!Input.GetKey(KeyCode.Mouse0))
                    return;
            }

            Fire();
            CmdFire(base.TimeManager.GetPreciseTick(TickType.Tick), _cameraTransform.position, _cameraTransform.forward);
        }

        /// <summary>
        /// Returns if there is ammunition available to fire.
        /// </summary>
        /// <returns></returns>
        private bool AmmunitionAvailable()
        {
            return !Weapon.IsClipEmpty();
        }

        /// <summary>
        /// Returns if enough time has passed since last fire. This method assumes the current weapon index is valid.
        /// </summary>
        /// <returns></returns>
        private bool FireTimeMet()
        {
            if (base.IsClient)
            {
                return (Time.time >= _nextFireTime);
            }
            else
            {
                float allowance = Mathf.Min(0.016f, Weapon.FireRate * 0.15f);
                return (Time.time + allowance >= _nextFireTime);
            }
        }

        /// <summary>
        /// Returns if the current WeaponIndex is valid.
        /// </summary>
        /// <returns></returns>
        public bool WeaponIndexValid()
        {
            return (_weaponIndex >= 0 && _weaponIndex < Weapons.Length);
        }

        /// <summary>
        /// Returns if the specified weaponIndex is valid.
        /// </summary>
        /// <returns></returns>
        public bool WeaponIndexValid(int index)
        {
            //Out of range.
            if (index < 0 || index >= Weapons.Length)
                return false;
            //Not in inventory.
            if (!Weapons[index].InInventory)
                return false;
            //Cannot equip empty, and is empty.
            if (!Weapons[index].CanEquipEmpty && Weapons[index].IsAmmunitionEmpty())
                return false;

            return true;
        }

        /// <summary>
        /// Fires the current weapon using the camera. This method assumes the current weapon index is valid.
        /// </summary>
        private void Fire()
        {
            Fire(default, _cameraTransform.position, _cameraTransform.forward);
        }

        /// <summary>
        /// Fires the current weapon from a position and using a forward.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        private void Fire(PreciseTick pt, Vector3 position, Vector3 forward)
        {
            //Set next fire rate.
            _nextFireTime = Time.time + Weapon.FireRate;

            float recoilMultiplier = Weapon.ReturnCameraRecoilMultiplier();
            //Call fire on the weapon in case it has any special effects.
            Weapon.Fire(pt, position, forward, ReturnNetworkRoles());

            _animatorController.Attack();

            //Recoil if client or client host.
            if (!base.IsServerOnly)
            {
                //Apply recoil to model.
                ShakeData modelRecoil = Weapon.ReturnModelRecoilShake();
                if (modelRecoil != null)
                    _recoilIK.ObjectShaker.Shake(modelRecoil);
                //CameraRecoilShaker will be null for non-owners.
                if (_cameraRecoilShaker != null)
                {
                    ShakeData cameraRecoil = Weapon.ReturnCameraRecoilShake();
                    if (cameraRecoil != null)
                    {
                        ShakerInstance si = _cameraRecoilShaker.Shake(cameraRecoil);
                        si.MultiplyMagnitude(recoilMultiplier, 0f);
                    }
                }
            }

            //Ignore layers on attacker temporarily.
            SetHitboxesLayer(true);

            //Rollback only if a rollback time and if not melee.
            bool rollingBack = (!Comparers.IsDefault(pt) && Weapon.WeaponType != WeaponTypes.Melee);
            //If a rollbackTime exist then rollback colliders before firing.
            if (rollingBack)
                RollbackManager.Rollback(pt, RollbackPhysicsType.Physics);

            //Raycast.
            if (Weapon.WeaponType == WeaponTypes.Raycast)
            {
                Ray ray = new Ray(position, forward);
                RaycastHit hit;
                //If ray hits.
                if (Physics.Raycast(ray, out hit, float.PositiveInfinity, (GlobalManager.LayerManager.DefaultLayer | GlobalManager.LayerManager.HitboxLayer)))
                {
                    //Show hit effect.
                    if (base.IsClient)
                        Weapon.RayImpact(hit, false);

                    //Apply damage and other server things.
                    if (base.IsServer)
                    {
                        Hitbox hitbox = hit.collider.GetComponent<Hitbox>();
                        if (hitbox != null)
                            hitbox.Hit(Weapon.Damage);
                    }
                }
            }
            //Melee.
            if (Weapon.WeaponType == WeaponTypes.Melee)
            {
                Ray ray = new Ray(position, forward);
                //If ray hits.
                Collider[] hits = Physics.OverlapSphere(position + (forward * Weapon.ReturnMeleeDistance()), Weapon.ReturnMeleeRadius(), (GlobalManager.LayerManager.DefaultLayer | GlobalManager.LayerManager.HitboxLayer));
                {
                    for (int i = 0; i < hits.Length; i++)
                    {

                        //Show hit effect.
                        if (base.IsClient)
                            Weapon.OverlapImpact(position, forward, hits[i], ReturnNetworkRoles());

                        //Apply damage and other server things.
                        if (base.IsServer)
                        {
                            Hitbox hitbox = hits[i].GetComponent<Hitbox>();
                            if (hitbox != null)
                            {
                                /* Melee abilities should do the same amount of damage every time,
                                 * and double damage it from behind. */
                                int modifiedDamage = Mathf.CeilToInt((1f / hitbox.Multiplier) * Weapon.Damage);
                                float angle = Vector3.Angle(hitbox.TopmostParent.forward, forward);
                                if (angle >= 0f && angle <= 45f)
                                    modifiedDamage *= 2;

                                hitbox.Hit(modifiedDamage);
                            }
                        }
                    }
                }
            }

            if (rollingBack)
                RollbackManager.Return();

            //Set layers back to normal.
            SetHitboxesLayer(false);
        }


        /// <summary>
        /// Calls Fire over the network.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        [ServerRpc]
        private void CmdFire(PreciseTick pt, Vector3 position, Vector3 forward)
        {
            //Only fire again on server if not client host/owner.
            if (!base.IsOwner)
            {
                if (!WeaponIndexValid())
                    return;
                if (!AmmunitionAvailable())
                    return;
                if (!FireTimeMet())
                    return;
                //If player is too far from fire point on server then do not fire.
                float maxDistance = 1f;
                float distance = Vector3.Distance(position, new Vector3(transform.position.x, position.y, transform.position.z));
                if (distance >= maxDistance)
                    return;

                Fire(pt, position, forward);
            }
            //Send fire to other players so they can show the shot.
            ObserversFire(position, forward);
        }

        /// <summary>
        /// Fires on all clients.
        /// </summary>
        [ObserversRpc]
        private void ObserversFire(Vector3 position, Vector3 forward)
        {
            //If owner or server.
            if (base.IsOwner || base.IsServer)
                return;
            if (!WeaponIndexValid())
                return;

            Fire(default, position, forward);
        }

        #region Weapon switching.
        /// <summary>
        /// Selects the first weapon available in Weapons.
        /// </summary>
        /// <param name="owner"></param>
        private void SelectFirstValidWeapon(bool owner)
        {
            int index = -1;
            for (int i = 0; i < Weapons.Length; i++)
            {
                //Weapon index can be used.
                if (WeaponIndexValid(i))
                {
                    index = i;
                    break;
                }
            }

            //No valid index found.
            if (index == -1)
                return;

            EquipWeapon(index, owner, true);
        }

        /// <summary>
        /// Equip the specified weapon name.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="owner"></param>
        /// <param name="ignoreActive">True to equip weapon regardless if already active.</param>
        /// <returns>True if was able to start equip weapon.</returns>
        private bool EquipWeapon(int index, bool owner, bool ignoreActive = false)
        {
            //Weapon is equipped, no need to switch to it.
            if (!ignoreActive && index == _weaponIndex)
                return false;
            //Unsubscribe from old weapon.
            if (WeaponIndexValid())
                SubscribeToWeaponEvents(Weapon, false);
            //Subscribe to new weapon.
            SubscribeToWeaponEvents(Weapons[index], true);

            //Set to new index.
            _weaponIndex = index;
            //Set next fire time to equip time.
            _nextFireTime = Time.time + Weapons[index].EquipDelay;
            //Find the weapon to equip.
            for (int i = 0; i < Weapons.Length; i++)
                Weapons[i].SetEquipped((i == index), ReturnNetworkRoles());

            //Update animator.
            _animatorController.SetWeaponIndex(Weapons[index].AnimatorIndex);
            //If owner queue weapon change to server.
            if (owner)
            {
                //Reset select first weapon time.
                _selectFirstAvailableWeaponTime = -1f;
                //Queue weapon change index.
                _queuedWeaponChangeIndex = index;
                //Changing weapon unsets reload.
                _reloadQueued = false;
            }

            //Only clients use IK weight.
            if (base.IsClient)
            {
                //Only use IK weight if a weapon that uses recoil.
                float ikWeight = (Weapons[index].WeaponType == WeaponTypes.Raycast) ? 1f : 0f;
                _recoilIK.SetIKWeight(ikWeight);
            }

            OnWeaponEquipped?.Invoke(Weapons[_weaponIndex].WeaponName);

            return true;
        }


        /// <summary>
        /// Reloads the weapon.
        /// </summary>
        /// <param name="owner">True if reloading as owner.</param>
        /// <returns>True if able to reload the weapon.</returns>
        private void ReloadWeapon()
        {
            //Set next fire time to equip time.
            _nextFireTime = Time.time + Weapon.EquipDelay + Weapon.ReturnReloadDuration();
            //AddRoles
            Weapon.Reload(ReturnNetworkRoles(), false);
            //Start reload animation if a client.
            if (!base.IsServerOnly)
                _animatorController.Reload();
        }

        /// <summary>
        /// Checks to switch weapons.
        /// </summary>
        [Client(Logging = LoggingType.Off)]
        private void CheckQueueReloadWeapon()
        {
            if (Input.GetKeyDown(KeyCode.R))
                _reloadQueued = true;
        }

        /// <summary>
        /// Checks to switch weapons.
        /// </summary>
        [Client(Logging = LoggingType.Off)]
        private void CheckQueueSwitchWeapon()
        {
            if (Weapons.Length == 0)
                return;

            //If to select first weapon then ignore input switches.
            if (_selectFirstAvailableWeaponTime != -1f && Time.time > _selectFirstAvailableWeaponTime)
            {
                SelectFirstValidWeapon(base.IsOwner);
                //Unset.
                _selectFirstAvailableWeaponTime = -1f;

                return;
            }

            //Numeric keys.
            int numericIndex = -1;
            if (Input.GetKeyDown(KeyCode.Alpha1))
                numericIndex = 1;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                numericIndex = 2;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                numericIndex = 3;
            if (Input.GetKeyDown(KeyCode.Alpha4))
                numericIndex = 4;

            float wheelValue = 0f;
            //Only get wheel value if no numeric input.
            if (numericIndex == -1)
                wheelValue = Input.GetAxisRaw("Mouse ScrollWheel");

            //No input.
            if (wheelValue == 0f && numericIndex == -1)
                return;

            if (wheelValue != 0f)
                TryMouseWheelWeaponSwitch(wheelValue);
            else
                TryNumericWeaponSwitch(numericIndex);

        }

        /// <summary>
        /// Tries to switch weapons using numeric keys.
        /// </summary>
        /// <param name="index"></param>
        private void TryNumericWeaponSwitch(int index)
        {
            /* Try to equip the first weapon on whichever key is pressed.
             * Selecting between multiple weapons on the same index is not yet supported. //todo */
            for (int i = 0; i < Weapons.Length; i++)
            {
                //Weapon to switch to.
                if (Weapons[i].InInventory && Weapons[i].EquipNumber == index)
                {
                    //Try to equip desired index. If valid setup weapon change data.
                    EquipWeapon(i, base.IsOwner);
                    break;
                }
            }
        }

        /// <summary>
        /// Tries to switch weapons using mouse wheel.
        /// </summary>
        /// <param name="value"></param>
        private void TryMouseWheelWeaponSwitch(float value)
        {
            if (Time.time < _nextWheelWeaponSwitchTime)
                return;

            //Inverse mouse wheel.
            value *= -1f;

            //Limit wheel switches to every 100ms.
            _nextWheelWeaponSwitchTime = Time.time + 0.250f;
            /* Set the desired index to current index.
             * DesiredIndex is used rather than WeaponIndex
             * because I don't want to actually change the weapon
             * index unless a switch is successful, and there are
             * checks which may prevent a switch. */
            int desiredIndex = _weaponIndex;

            bool changeAllowed = false;
            /* Run until a valid index is found.
             * This is to cycle over empty weapons. */
            while (!changeAllowed)
            {
                //Cycle index based on mouse input.
                if (value < 0f)
                    desiredIndex--;
                else if (value > 0f)
                    desiredIndex++;

                //If over or undershooting weapon list then reset on opposite side.
                if (desiredIndex < 0)
                    desiredIndex = Weapons.Length - 1;
                else if (desiredIndex >= Weapons.Length)
                    desiredIndex = 0;

                //Went full circle, no other weapons could be found.
                if (desiredIndex == _weaponIndex)
                    return;
                //Valid change.
                if (WeaponIndexValid(desiredIndex))
                    changeAllowed = true;
            }

            //Try to equip desired index. If valid setup weapon change data.
            EquipWeapon(desiredIndex, base.IsOwner);
        }
        #endregion


    }


}