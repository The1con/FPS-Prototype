using FirstGearGames.Managers.Global;

using UnityEngine;

namespace FirstGearGames.FPSLand.Managers.Gameplay
{

    public class SpawnPoint : MonoBehaviour
    {
        #region Serialized.
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Radius of this spawn point.")]
        [SerializeField]
        private float _radius = 3f;
        /// <summary>
        /// Radius of this spawn point.
        /// </summary>
        public float Radius { get { return _radius; } }
        #endregion

        private void Start()
        {
            FirstInitialize();
        }

        /// <summary>
        /// Initializes this script for use. Should only be completed once.
        /// </summary>
        private void FirstInitialize()
        {
            SnapToGround();
        }

        /// <summary>
        /// Snaps the transform to the ground when possible.
        /// </summary>
        private void SnapToGround()
        {
            Ray ray = new Ray(transform.position, Vector3.down);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, Radius * 2f, GlobalManager.LayerManager.MovementBlockingLayers))
                transform.position = hit.point;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }


}