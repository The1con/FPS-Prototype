using FirstGearGames.FPSLand.Managers.Gameplay.Canvases;
using UnityEngine;

namespace FirstGearGames.FPSLand.Managers.Gameplay
{

    public class OfflineGameplayDependencies : MonoBehaviour
    {

        #region Serialized.
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("SpawnManager component.")]
        [SerializeField]
        private SpawnManager _spawnManager;
        /// <summary>
        /// SpawnManager reference.
        /// </summary>
        public static SpawnManager SpawnManager { get { return _instance._spawnManager; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("GameplayCanvases component.")]
        [SerializeField]
        private GameplayCanvases _gameplayCanvases;
        /// <summary>
        /// GameplayCanvases reference.
        /// </summary>
        public static GameplayCanvases Gameplaycanvases { get { return _instance._gameplayCanvases; } }
        /// <summary>
        /// 
        /// </summary>
        [Tooltip("AudioManager reference.")]
        [SerializeField]
        private AudioManager _audioManager;
        /// <summary>
        /// AudioManager reference.
        /// </summary>
        public static AudioManager AudioManager { get { return _instance._audioManager; } }
        #endregion

        #region Private.
        /// <summary>
        /// Singleton reference of this component.
        /// </summary>
        private static OfflineGameplayDependencies _instance;
        #endregion

        private void Awake()
        {
            _instance = this;
        }


    }


}