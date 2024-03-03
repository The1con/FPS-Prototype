using UnityEngine;

namespace FirstGearGames.FPSLand.Characters.Bodies
{

    /// <summary>
    /// Changes visibility of body gameObject based on ownership.
    /// </summary>
    public class BodiesConfigurations : MonoBehaviour
    {

        /// <summary>
        /// 
        /// </summary>
        [Tooltip("GameObjects to enabled as first person.")]
        [SerializeField]
        private GameObject _firstPersonObject;
        /// <summary>
        /// GameObjects to enabled as first person.
        /// </summary>
        public GameObject FirstPersonObject { get { return _firstPersonObject; } }

        /// <summary>
        /// 
        /// </summary>
        [Tooltip("GameObjects to enabled as third person.")]
        [SerializeField]
        private GameObject _thirdPersonObject;
        /// <summary>
        /// GameObjects to enabled as third person.
        /// </summary>
        public GameObject ThirdPersonObject { get { return _thirdPersonObject; } }
    }

}