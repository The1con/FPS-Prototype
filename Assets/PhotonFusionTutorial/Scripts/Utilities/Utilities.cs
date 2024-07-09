using UnityEngine;
namespace PhotonFusionTutorial.Scripts.Utilities
{
    public static class Utilities
    {
        public static Vector3 GetRandomSpawnPoint()
        {
            return new Vector3(Random.Range(-20, 20), 4, Random.Range(-20, 20));
        }
        
        public static void SetRenderLayerInChildren(Transform parent, int layer)
        {
            foreach (var child in parent.GetComponentsInChildren<Transform>())
                child.gameObject.layer = layer;
        }
    }
}
