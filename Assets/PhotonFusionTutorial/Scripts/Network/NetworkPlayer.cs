using Fusion;
using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Network
{
    public class NetworkPlayer : NetworkBehaviour, IPlayerLeft
    {
        public static NetworkPlayer Local { get; private set; }

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                Local = this;
                
                Debug.Log("Spawned local player");
            }
            else
                Debug.Log("Spawned remote player");
        }

        public void PlayerLeft(PlayerRef player)
        {
            if (Object.HasInputAuthority == player)
                Runner.Despawn(Object);
        }
    }
}
