using Fusion;
using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Network
{
    public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
    {
        private const string LayerName = "LocalPlayerModel";
        
        [SerializeField] private UnityEngine.Camera playerCamera;
        [SerializeField] private AudioListener audioListener;

        public Transform playerModel;
        public static NetworkPlayer Local { get; private set; }
        
        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                Local = this;
                
                Utilities.Utilities.SetRenderLayerInChildren(playerModel, LayerMask.NameToLayer(LayerName));
                
                UnityEngine.Camera.main.gameObject.SetActive(false);
                
                Debug.Log("Spawned local player");
            }
            else
            {
                playerCamera.enabled = false;
                audioListener.enabled = false;
                
                Debug.Log("Spawned remote player");
            }
            
            transform.name = $"Player {Object.Id}";
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (Object.HasInputAuthority == player)
                Runner.Despawn(Object);
        }
    }
}
