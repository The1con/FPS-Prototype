using UnityEngine;

namespace FirstGearGames.Managers.Global
{

    public class GlobalManager : MonoBehaviour
    {
        /// <summary>
        /// Singleton reference for this manager.
        /// </summary>
        public static GlobalManager Instance;

        /// <summary>
        /// LayerManager reference.
        /// </summary>
        [Tooltip("LayerManager reference.")]
        [SerializeField]
        private LayerManager _layerManager;
        /// <summary>
        /// LayerManager reference.
        /// </summary>
        public static LayerManager LayerManager { get { return Instance._layerManager; } }

        private void Awake()
        {
            FirstInitialize();
        }

        /// <summary>
        /// Initializes this script for use. Should only be completed once.
        /// </summary>
        private void FirstInitialize()
        {
            //If singleton was somehow loaded twice.
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }
    }


}