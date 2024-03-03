using FirstGearGames.FPSLand.Characters.Vitals;
using FirstGearGames.FPSLand.Managers.Gameplay;
using FishNet.Connection;
using FishNet.Managing.Logging;
using FishNet.Object;
using System;
using UnityEngine;

namespace FirstGearGames.FPSLand.Clients
{


    public class PlayerSpawner : NetworkBehaviour
    {
        #region Types.
        public class PlayerData
        {
            public NetworkObject NetworkObject;
            public Health Health;
        }
        #endregion

        #region Public.
        /// <summary>
        /// Dispatched when the character is updated.
        /// </summary>
        public static event Action<GameObject> OnCharacterUpdated;
        /// <summary>
        /// Data about the currently spawned player.
        /// </summary>
        public PlayerData SpawnedCharacterData { get; private set; } = new PlayerData();
        #endregion

        #region Serialized.
        /// <summary>
        /// Character prefab to spawn.
        /// </summary>
        [Tooltip("Character prefab to spawn.")]
        [SerializeField]
        private GameObject _characterPrefab;
        #endregion

        /// <summary>
        /// Tries to respawn the player.
        /// </summary>
        [Client(Logging = LoggingType.Off)]
        public void TryRespawn()
        {
            CmdRespawn();
        }

        /// <summary>
        /// Sets up SpawnedCharacterData using a gameObject.
        /// </summary>
        /// <param name="go"></param>
        private void SetupSpawnedCharacterData(GameObject go)
        {
            SpawnedCharacterData.NetworkObject = go.GetComponent<NetworkObject>();
            SpawnedCharacterData.Health = go.GetComponent<Health>();
        }


        /// <summary>
        /// Request a respawn from the server.
        /// </summary>
        [ServerRpc]
        private void CmdRespawn()
        {
            Transform spawn = OfflineGameplayDependencies.SpawnManager.ReturnSpawnPoint();
            if (spawn == null)
            {
                Debug.LogError("All spawns are occupied.");
            }
            else
            {
                //If the character is not spawned yet.
                if (SpawnedCharacterData.NetworkObject == null)
                {
                    GameObject r = Instantiate(_characterPrefab, spawn.position, Quaternion.Euler(0f, spawn.eulerAngles.y, 0f));
                    base.Spawn(r, base.Owner);

                    SetupSpawnedCharacterData(r);
                    TargetCharacterSpawned(base.Owner, SpawnedCharacterData.NetworkObject);
                }
                //Character is already spawned.
                else
                {
                    SpawnedCharacterData.NetworkObject.transform.position = spawn.position;
                    SpawnedCharacterData.NetworkObject.transform.rotation = Quaternion.Euler(0f, spawn.eulerAngles.y, 0f);
                    Physics.SyncTransforms();
                    //Restore health and set respawned.
                    SpawnedCharacterData.Health.RestoreHealth();
                    SpawnedCharacterData.Health.Respawned();
                }

            }
        }

        /// <summary>
        /// Received when the server has spawned the character.
        /// </summary>
        /// <param name="character"></param>
        [TargetRpc]
        private void TargetCharacterSpawned(NetworkConnection conn, NetworkObject character)
        {
            GameObject playerObj = (character == null) ? null : playerObj = character.gameObject;
            OnCharacterUpdated?.Invoke(playerObj);

            //If player was spawned.
            if (playerObj != null)
                SetupSpawnedCharacterData(character.gameObject);
        }

    }


}