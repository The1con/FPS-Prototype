using FirstGearGames.Managers.Global;
using GameKit.Utilities;
using UnityEngine;

namespace FirstGearGames.FPSLand.Managers.Gameplay
{

    public class SpawnManager : MonoBehaviour
    {
        /// <summary>
        /// Parent object which hold spawn points.
        /// </summary>
        [Tooltip("Parent object which hold spawn points.")]
        [SerializeField]
        private Transform _spawnPointParent;

        #region Private.
        /// <summary>
        /// Found spawn points.
        /// </summary>
        private SpawnPoint[] _spawnPoints = new SpawnPoint[0];
        #endregion

        private void Awake()
        {
            _spawnPoints = _spawnPointParent.GetComponentsInChildren<SpawnPoint>();
        }

        /// <summary>
        /// Returns a random spawn point.
        /// </summary>
        /// <returns></returns>
        public Transform ReturnSpawnPoint()
        {
            if (_spawnPoints.Length == 0)
                return null;

            //Shuffle spawn points first.
            _spawnPoints.Shuffle();

            for (int i = 0; i < _spawnPoints.Length; i++)
            {
                if (!_spawnPoints[i].gameObject.activeInHierarchy)
                    continue;
                //Make sure there are no players within vicinity of the spawn point.
                Collider[] hits = Physics.OverlapSphere(_spawnPoints[i].transform.position, _spawnPoints[i].Radius, GlobalManager.LayerManager.CharacterLayer);
                //No hits, spawn point is open.
                if (hits.Length == 0)
                    return _spawnPoints[i].transform;
            }

            //If here no valid spawn points found. Pick first one.
            return _spawnPoints[0].transform;
        }

    }


}