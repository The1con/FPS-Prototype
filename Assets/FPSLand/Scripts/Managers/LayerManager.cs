using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace FirstGearGames.Managers.Global
{


    public class LayerManager : MonoBehaviour
    {
        #region Public.
        /// <summary>
        /// Layers which are considered terrain.
        /// </summary>
        public LayerMask MovementBlockingLayers { get { return (DefaultLayer | DefaultLayerBulletPassthrough); } }
        #endregion

        #region Serialized.
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Layer for Hitbox.")]
        [SerializeField]
        private LayerMask _hitboxLayer;
        /// <summary>
        /// Layer for Hitbox.
        /// </summary>
        public LayerMask HitboxLayer { get { return _hitboxLayer; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Layer for Default.")]
        [SerializeField]
        private LayerMask _defaultLayer;
        /// <summary>
        /// Layer for Default.
        /// </summary>
        public LayerMask DefaultLayer { get { return _defaultLayer; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Layer for DefaultBulletPassthrough.")]
        [SerializeField]
        private LayerMask _defaultLayerBulletPassthrough;
        /// <summary>
        /// Layer for Default.
        /// </summary>
        public LayerMask DefaultLayerBulletPassthrough { get { return _defaultLayerBulletPassthrough; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Layer for NoClip.")]
        [SerializeField]
        private LayerMask _noClipLayer;
        /// <summary>
        /// Layer for NoClip.
        /// </summary>
        public LayerMask NoClipLayer { get { return _noClipLayer; } }

        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Layer for IgnoreCollision.")]
        [SerializeField]
        private LayerMask _ignoreCollisionLayer;
        /// <summary>
        /// Layer for IgnoreCollision.
        /// </summary>
        public LayerMask IgnoreCollisionLayer { get { return _ignoreCollisionLayer; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Layer for Characters.")]
        [SerializeField]
        private LayerMask _characterLayer;
        /// <summary>
        /// Layer for Characters.
        /// </summary>
        public LayerMask CharacterLayer { get { return _characterLayer; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Layer for IgnoreCharacter.")]
        [SerializeField]
        private LayerMask _ignoreCharacterLayer;
        /// <summary>
        /// Layer for IgnoreCharacter.
        /// </summary>
        public LayerMask IgnoreCharacterLayer { get { return _ignoreCharacterLayer; } }
        #endregion

        /// <summary>
        /// Returns if a GameObject layer is in LayerMask.
        /// </summary>
        public bool InLayerMask(GameObject go, LayerMask layerMask)
        {
            return ((layerMask.value & (1 << go.layer)) > 0);
        }

    }


}