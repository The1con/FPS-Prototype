
using FishNet;
using FishNet.Connection;
using FishNet.Object;

namespace FirstGearGames.FPSLand.Clients
{

    public class ClientInstance : NetworkBehaviour
    {
        #region Public.
        /// <summary>
        /// Singleton reference to the client instance.
        /// </summary>
        public static ClientInstance Instance;
        /// <summary>
        /// PlayerSpawner on this object.
        /// </summary>
        public PlayerSpawner PlayerSpawner { get; private set; }
        #endregion

        #region Constants.
        /// <summary>
        /// Maximum time between UserInput and UserInputResults which the player can have before their sending becomes limited.
        /// If a packet takes longer than this value to get to the server then it's discarded.
        /// If the server's last network time to the client is longer than this value then the clients input isn't applied.
        /// </summary>
        public const double MAX_INPUTS_DELAY = 150d;
        /// <summary>
        /// Version of this build.
        /// </summary>
        private const int VERSION_CODE = 7;
        #endregion

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (base.IsOwner)
            {
                Instance = this;
                PlayerSpawner = GetComponent<PlayerSpawner>();
                PlayerSpawner.TryRespawn();
            }
        }

        /// <summary>
        /// Returns the current client instance for the connection.
        /// </summary>
        /// <returns></returns>
        public static ClientInstance ReturnClientInstance(NetworkConnection conn)
        {
            /* If server and connection isnt null.
             * When trying to access as server connection
             * will always contain a value. But if client it will be
             * null. */
            if (InstanceFinder.IsServer && conn != null)
            {
                NetworkObject nob = conn.FirstObject;
                return (nob == null) ? null : nob.GetComponent<ClientInstance>();
            }
            //If not server or connection is null, then is client.
            else
            {
                return Instance;
            }
        }

    }

}
