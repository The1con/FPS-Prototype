
using System;
using UnityEngine;

namespace FirstGearGames.FPSLand.Characters.Vitals
{

    public class Hitbox : MonoBehaviour
    {
        #region Public.
        /// <summary>
        /// Dispatched when this hitbox is hit
        /// </summary>
        public event Action<Hitbox, int> OnHit;
        /// <summary>
        /// Topmost parent of this hitbox.
        /// </summary>
        public Transform TopmostParent { get; private set; }
        /// <summary>
        /// Sets the TopmostParent value.
        /// </summary>
        /// <param name="t"></param>
        public void SetTopmostParent(Transform t)
        {
            TopmostParent = t;
        }
        #endregion

        #region Serialized.
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Amount of multiplier to apply towards normal damage when this hitbox is hit.")]
        [SerializeField]
        private float _multiplier = 1f;
        /// <summary>
        /// Amount of multiplier to apply towards normal damage when this hitbox is hit.
        /// </summary>
        public float Multiplier { get { return _multiplier; } }
        #endregion

        /// <summary>
        /// Indicates a hit to this hitbox.
        /// </summary>
        /// <param name="damage">Amount of damage from hit.</param>
        public void Hit(int damage)
        {
            OnHit?.Invoke(this, damage);
        }

    }


}