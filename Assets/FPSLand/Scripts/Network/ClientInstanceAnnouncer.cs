
using FishNet.Object;
using System;


namespace FirstGearGames.FPSLand.Network
{

    /// <summary>
    /// To be attached to the client instance prefab to know when it's instantiated for owner.
    /// </summary>
    public class ClientInstanceAnnouncer : NetworkBehaviour
    {

        #region Public.
        /// <summary>
        /// Dispatched when the player object spawns or is enabled/disabled.
        /// </summary>
        public static event Action<NetworkObject> OnPlayerUpdated;
        #endregion

        #region Start/Destroy
        public override void OnStartClient()
        {
            base.OnStartClient();
            if (base.IsOwner)
                OnPlayerUpdated?.Invoke(base.NetworkObject);
        }
        #endregion

        #region OnEnable/Disable.
        private void OnEnable()
        {
            if (base.IsOwner)
                OnPlayerUpdated?.Invoke(base.NetworkObject);
        }
        private void OnDisable()
        {
            if (base.IsOwner)
                OnPlayerUpdated?.Invoke(null);
        }
        #endregion

    }


}