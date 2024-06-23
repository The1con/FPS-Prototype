using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using PhotonFusionTutorial.Scripts.Movement;
using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Network
{
    public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private NetworkPlayer playerPrefab;
        private CharacterInputHandler characterInputHandler;
    
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                Debug.Log("OnPlayerJoined we are server. Spawning Player");
                runner.Spawn(playerPrefab, Utilities.Utilities.GetRandomSpawnPoint(), Quaternion.identity, player);
            }
            Debug.Log("OnPlayerJoined");
        }
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("OnPlayerLeft");
        }
        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (characterInputHandler == null && NetworkPlayer.Local != null)
                characterInputHandler = NetworkPlayer.Local.GetComponent<CharacterInputHandler>();
            
            if (characterInputHandler != null)
                input.Set(characterInputHandler.GetNetworkInput());
        }
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
            Debug.Log("OnInputMissing");
        }
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log("OnShutdown");
        }
        public void OnConnectedToServer(NetworkRunner runner)
        {
            Debug.Log("OnConnectedToServer");
        }
        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            Debug.Log("OnDisconnectedFromServer");
        }
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
            Debug.Log("OnConnectRequest");
        }
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
            Debug.Log("OnConnectFailed");
        }
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
            Debug.Log("OnUserSimulationMessage");
        }
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            Debug.Log("OnSessionListUpdated");
        }
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
            Debug.Log("OnCustomAuthenticationResponse");
        }
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            Debug.Log("OnHostMigration");
        }
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
            Debug.Log("OnReliableDataReceived");
        }
        public void OnSceneLoadDone(NetworkRunner runner)
        {
            Debug.Log("OnSceneLoadDone");
        }
        public void OnSceneLoadStart(NetworkRunner runner)
        {
            Debug.Log("OnSceneLoadStart");
        }
    }
}
