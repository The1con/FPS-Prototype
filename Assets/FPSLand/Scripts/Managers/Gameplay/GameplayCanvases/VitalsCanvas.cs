using FirstGearGames.FPSLand.Characters.Vitals;
using FirstGearGames.FPSLand.Clients;
using GameKit.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FirstGearGames.FPSLand.Managers.Gameplay.Canvases
{

    public class VitalsCanvas : MonoBehaviour
    {
        #region Serialized.
        /// <summary>
        /// Text to show health numeric value.
        /// </summary>
        [Tooltip("Text to show health numeric value.")]
        [SerializeField]
        private TextMeshProUGUI _healthText;
        /// <summary>
        /// Image used to show health as a fill bar.
        /// </summary>
        [Tooltip("Image used to show health as a fill bar.")]
        [SerializeField]
        private Image _fillImage;
        /// <summary>
        /// Image which changes color based on health value.
        /// </summary>
        [Tooltip("Image which changes color based on health value.")]
        [SerializeField]
        private Image _healthBackground;
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

            Health health = obj.GetComponent<Health>();
            health.OnRespawned += Health_OnRespawned;
            health.OnDeath += Health_OnDeath;            
            health.OnHealthChanged += Health_OnHealthChanged;

            float percent = ((float)health.CurrentHealth / (float)health.MaximumHealth);
            UpdateCanvasElements(health.CurrentHealth, percent);
            _canvasGroup.SetActive(true, true);
        }


        /// <summary>
        /// Received with old and new values of character health when changed.
        /// </summary>
        private void Health_OnHealthChanged(int oldHealth, int newHealth, int maxHealth)
        {
            float percent = ((float)newHealth / (float)maxHealth);
            UpdateCanvasElements(newHealth, percent);
        }

        /// <summary>
        /// Updates canvas elements using specified values.
        /// </summary>
        /// <param name="healthValue"></param>
        /// <param name="healthPercent"></param>
        private void UpdateCanvasElements(int healthValue, float healthPercent)
        {
            //Color a dark red if at or below 25% health.
            Color backgroundColor = (healthPercent <= 0.25f) ? new Color(0.75f, 0f, 0f, 0.5f) : new Color(0f, 0f, 0f, 0.5f);
            //Set UI values.
            _healthText.text = healthValue.ToString();
            _healthBackground.color = backgroundColor;
            _fillImage.fillAmount = healthPercent;
        }

        /// <summary>
        /// Received when the character health is restored.
        /// </summary>
        private void Health_OnRespawned()
        {
            _canvasGroup.SetActive(true, true);
        }

        /// <summary>
        /// Received when the character health is depleted.
        /// </summary>
        private void Health_OnDeath()
        {
            _canvasGroup.SetActive(false, true);
        }


    }


}