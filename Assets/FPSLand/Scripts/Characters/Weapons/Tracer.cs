using FirstGearGames.Managers.Global;
using GameKit.Utilities.ObjectPooling;
using UnityEngine;

namespace FirstGearGames.FPSLand.Characters.Weapons
{


    public class Tracer : MonoBehaviour
    {
        #region Private.
        /// <summary>
        /// Direction to travel in.
        /// </summary>
        private Vector3 _direction;
        /// <summary>
        /// How fast to move.
        /// </summary>
        private float _moveRate;
        /// <summary>
        /// TrailRenderer on this object.
        /// </summary>
        private TrailRenderer _trailRenderer;
        #endregion

        private void Awake()
        {
            _trailRenderer = GetComponent<TrailRenderer>();
        }

        private void Update()
        {
            MoveTracer();
        }

        private void MoveTracer()
        {
            //If this step would hit something destroy tracer.
            if (Physics.Linecast(transform.position, transform.position + (_direction * _moveRate * Time.deltaTime * 1.1f), (GlobalManager.LayerManager.DefaultLayer | GlobalManager.LayerManager.HitboxLayer)))
                ObjectPool.Store(gameObject);
            //Otherwise travel forward.
            else
                transform.position += (_direction * Time.deltaTime * _moveRate);            
        }

        /// <summary>
        /// Initializes this script for use.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="rate"></param>
        public void Initialize(Vector3 direction, float rate)
        {
            _trailRenderer.Clear();
            _direction = direction;
            _moveRate = rate;           
        }


    }


}