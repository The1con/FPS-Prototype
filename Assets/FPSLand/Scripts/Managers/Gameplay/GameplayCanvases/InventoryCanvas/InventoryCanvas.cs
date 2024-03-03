using FirstGearGames.FPSLand.Characters.Weapons;
using FirstGearGames.FPSLand.Clients;
using FirstGearGames.FPSLand.Utilities;
using FirstGearGames.FPSLand.Weapons;
using UnityEngine;

namespace FirstGearGames.FPSLand.Managers.Gameplay.Canvases
{

    public class InventoryCanvas : MonoBehaviour
    {
        #region Private.
        /// <summary>
        /// CanvasGroup on this object.
        /// </summary>
        private CanvasGroup _canvasGroup;
        /// <summary>
        /// InventoryCanvasEntry components beneath this object.
        /// </summary>
        private InventoryCanvasEntry[] _entries = new InventoryCanvasEntry[0];
        #endregion

        private void Awake()
        {
            FirstInitialize();
        }

        private void OnDestroy()
        {
            PlayerSpawner.OnCharacterUpdated -= PlayerSpawner_OnCharacterUpdated;
        }

        /// <summary>
        /// Initializes this script for use. Should only be completed once.
        /// </summary>
        private void FirstInitialize()
        {
            //Deselect all entr
            _entries = GetComponentsInChildren<InventoryCanvasEntry>();
            for (int i = 0; i < _entries.Length; i++)
            {
                _entries[i].SetSelected(false);
                _entries[i].SetInInventory(false);
            }

            PlayerSpawner.OnCharacterUpdated += PlayerSpawner_OnCharacterUpdated;
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.SetActive(false, true);
        }

        /// <summary>
        /// Received when the character is updated.
        /// </summary>
        /// <param name="obj"></param>
        private void PlayerSpawner_OnCharacterUpdated(GameObject obj)
        {
            if (obj == null)
            {
                _canvasGroup.SetActive(false, true);
                return;
            }

            //Subscribe to future ammunition changes.
            WeaponHandler wh = obj.GetComponent<WeaponHandler>();
            wh.OnWeaponAddedToInventory += WeaponHandler_OnWeaponAddedToInventory;
            wh.OnWeaponRemovedFromInventory += WeaponHandler_OnWeaponRemovedFromInventory;
            wh.OnWeaponEquipped += WeaponHandler_OnWeaponEquipped;

            for (int i = 0; i < wh.Weapons.Length; i++)
            {
                //If in inventory then add.
                if (wh.Weapons[i].InInventory)
                { 
                    WeaponHandler_OnWeaponAddedToInventory(wh.Weapons[i].WeaponName);
                    //If currently equipped.
                    if (wh.WeaponIndexValid() && wh.Weapon == wh.Weapons[i])
                        WeaponHandler_OnWeaponEquipped(wh.Weapon.WeaponName);
                }
            }
        }

        /// <summary>
        /// Received when weapon is equipped.
        /// </summary>
        private void WeaponHandler_OnWeaponEquipped(WeaponNames obj)
        {
            //Unequip all other entries.
            for (int i = 0; i < _entries.Length; i++)
                _entries[i].SetSelected(false);

            int index = ReturnEntryIndex(obj);
            if (index != -1)
                _entries[index].SetSelected(true);
        }

        /// <summary>
        /// Received when a weapon is removed from inventory.
        /// </summary>
        private void WeaponHandler_OnWeaponRemovedFromInventory(WeaponNames obj)
        {
            int index = ReturnEntryIndex(obj);
            if (index != -1)
                _entries[index].SetInInventory(false);
        }

        /// <summary>
        /// Received when a weapon is added to inventory.
        /// </summary>
        private void WeaponHandler_OnWeaponAddedToInventory(WeaponNames obj)
        {
            int index = ReturnEntryIndex(obj);
            if (index != -1)
                _entries[index].SetInInventory(true);
        }

        /// <summary>
        /// Returns the index in Entries for the specified weapon name.
        /// </summary>
        /// <param name="weaponName"></param>
        /// <returns></returns>
        private int ReturnEntryIndex(WeaponNames weaponName)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].WeaponName == weaponName)
                    return i;
            }

            //Fall through.
            return -1;
        }


    }


}