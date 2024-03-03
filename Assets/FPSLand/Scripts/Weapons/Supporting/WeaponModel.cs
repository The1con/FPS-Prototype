using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FirstGearGames.FPSLand.Weapons
{


    public class WeaponModel : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("Exit point for this weapon.")]
        [SerializeField]
        private Transform _exitPoint;
        /// <summary>
        /// Exit point for this weapon.
        /// </summary>
        public Transform ExitPoint { get { return _exitPoint; } }


    }


}