using FirstGearGames.FPSLand.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace FirstGearGames.FPSLand.Managers.Gameplay.Canvases
{


    public class InventoryCanvasEntry : MonoBehaviour
    {
        #region Serialized.
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("WeaponName for this inventory entry.")]
        [SerializeField]
        private WeaponNames _weaponName;
        /// <summary>
        /// WeaponName for this inventory entry.
        /// </summary>
        public WeaponNames WeaponName { get { return _weaponName; } }
        /// <summary>
        /// Image to color for selected indicator.
        /// </summary>
        [Tooltip("Image to color for selected indicator.")]
        [SerializeField]
        private Image _backgroundImage;
        #endregion

        #region Private.
        /// <summary>
        /// Color when this inventory item is selected.
        /// </summary>
        private readonly Color _selectedColor = new Color(1f, 1f, 1f, 0.3f);
        /// <summary>
        /// Color when this inventory item is deselected.
        /// </summary>
        private readonly Color _deselectedColor = new Color(0f, 0f, 0f, 0.2f);
        #endregion

        /// <summary>
        /// Sets selected visual.
        /// </summary>
        /// <param name="selected"></param>
        public void SetSelected(bool selected)
        {
            Color c = (selected) ? _selectedColor : _deselectedColor;
            _backgroundImage.color = c;
        }

        /// <summary>
        /// Sets if this entry is in inventory.
        /// </summary>
        /// <param name="inInventory"></param>
        public void SetInInventory(bool inInventory)
        {
            gameObject.SetActive(inInventory);
        }
    }


}