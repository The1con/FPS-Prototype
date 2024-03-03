using FirstGearGames.FPSLand.Characters.Vitals;
using FirstGearGames.FPSLand.Characters.Weapons;
using FirstGearGames.FPSLand.Clients;
using FirstGearGames.FPSLand.Utilities;
using TMPro;
using UnityEngine;

namespace FirstGearGames.FPSLand.Managers.Gameplay.Canvases
{

    public class ActiveWeaponCanvas : MonoBehaviour
    {
        #region Serialized.
        /// <summary>
        /// Text to show ammunition remaining in the current clip.
        /// </summary>
        [Tooltip("Text to show ammunition remaining in the current clip.")]
        [SerializeField]
        private TextMeshProUGUI _currentClipText;
        /// <summary>
        /// Text to show reserve ammunition remaining in the weapon.
        /// </summary>
        [Tooltip("Text to show reserve ammunition remaining in the weapon.")]
        [SerializeField]
        private TextMeshProUGUI _reserveAmmunitionText;
        #endregion

        #region Private.
        /// <summary>
        /// CanvasGroup on this object.
        /// </summary>
        private CanvasGroup _canvasGroup;
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
                return;

            Health h = obj.GetComponent<Health>();
            if (h != null)
            {
                _canvasGroup.SetActive(true, true);
                h.OnRespawned += Health_OnRespawned;
                h.OnDeath += Health_OnDeath;
            }

            //Subscribe to future ammunition changes.
            WeaponHandler wh = obj.GetComponent<WeaponHandler>();
            wh.OnClipRemainingChanged += WeaponHandler_OnClipRemainingChanged;
            wh.OnTotalAmmunitionChanged += WeaponHandler_OnTotalAmmunitionChanged;
            
            //Get current ammunition.
            if (wh.WeaponIndexValid())
            {
                WeaponHandler_OnClipRemainingChanged(wh.Weapon.ReturnClipRemaining());
                WeaponHandler_OnTotalAmmunitionChanged(wh.Weapon.ReturnReserveAmmunitionRemaining());
            }
        }


        /// <summary>
        /// Received when character dies.
        /// </summary>
        private void Health_OnDeath()
        {
            _canvasGroup.SetActive(false, true);           
        }

        /// <summary>
        /// Received when character is respawned.
        /// </summary>
        private void Health_OnRespawned()
        {
            _canvasGroup.SetActive(true, true);
        }

        /// <summary>
        /// Received whenever ammunition in the clip changes.
        /// </summary>
        private void WeaponHandler_OnTotalAmmunitionChanged(int ammo)
        {
            _reserveAmmunitionText.text = ammo.ToString();
        }
        /// <summary>
        /// Received whenever reserve ammunition in the weapon changes.
        /// </summary>
        private void WeaponHandler_OnClipRemainingChanged(int ammo)
        {
            _currentClipText.text = ammo.ToString();
        }

    }


}