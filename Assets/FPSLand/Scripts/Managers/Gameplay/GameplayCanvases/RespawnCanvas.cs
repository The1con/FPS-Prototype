using FirstGearGames.FPSLand.Characters.Vitals;
using FirstGearGames.FPSLand.Clients;
using FishNet;
using FishNet.Object;
using UnityEngine;
using FirstGearGames.FPSLand.Network;
using GameKit.Utilities;
using UnityEngine.EventSystems;

namespace FirstGearGames.FPSLand.Managers.Gameplay.Canvases
{

    public class RespawnCanvas : MonoBehaviour
    {
        #region Serialized.
        /// <summary>
        /// PlayerSpawner in the scene.
        /// </summary>
        private PlayerSpawner _playerSpawner;
        #endregion

        #region Private.
        /// <summary>
        /// CanvasGroup on this object.
        /// </summary>
        private CanvasGroup _canvasGroup;
        #endregion

        private void Awake()
        {
            ClientInstanceAnnouncer.OnPlayerUpdated += ClientInstanceAnnouncer_OnUpdated;
            PlayerSpawner.OnCharacterUpdated += PlayerSpawner_OnCharacterUpdated;
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvasGroup.SetActive(false, true);
        }

        private void Update()
        {
            //Show cursor if not connected.
            if (InstanceFinder.IsOffline)
                SetCursorVisibility(true);
        }

        /// <summary>
        /// Received when the localPlayer is updated.
        /// </summary>
        /// <param name="obj"></param>
        private void ClientInstanceAnnouncer_OnUpdated(NetworkObject obj)
        {
            if (obj != null)
            {
                SetCursorVisibility(false);
                ClientInstance ci = obj.GetComponent<ClientInstance>();
                _playerSpawner = ci.PlayerSpawner;
            }
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
        }

        /// <summary>
        /// Received when the character is respawned.
        /// </summary>
        private void Health_OnRespawned()
        {
            SetCursorVisibility(false);
            _canvasGroup.SetActive(false, true);
        }

        /// <summary>
        /// Received when the character is dead.
        /// </summary>
        private void Health_OnDeath()
        {
            _canvasGroup.SetActive(true, true);
            SetCursorVisibility(true);
        }

        /// <summary>
        /// Sets cursor visibility. //todo Test code. This needs to go somewhere else but I'm feeling lazy.
        /// </summary>
        /// <param name="visible"></param>
        private void SetCursorVisibility(bool visible)
        {
            CursorLockMode lockMode = (visible) ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.lockState = lockMode;
            Cursor.visible = visible;
        }

        /// <summary>
        /// Received when Respawn button is clicked.
        /// </summary>
        public void OnClick_Respawn()
        {
            _playerSpawner.TryRespawn();
#if !ENABLE_INPUT_SYSTEM
            /* Deselect any canvas because when the respawn canvas
             * disappears it sometimes defaults to the server / client
             * button. In result, the next time the player pressed space
             * they become disconnected. */
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            eventSystem?.SetSelectedGameObject(null);
#endif
        }
    }


}