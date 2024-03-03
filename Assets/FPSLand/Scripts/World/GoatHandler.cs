using FirstGearGames.FPSLand.Managers.Gameplay;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;

public class GoatHandler : NetworkBehaviour
{
    /// <summary>
    /// Prefab used for goats.
    /// </summary>
    [Tooltip("Prefab used for goats.")]
    [SerializeField]
    private GameObject _goatPrefab;
    /// <summary>
    /// Maximum number of goats at once.
    /// </summary>
    [Tooltip("Maximum number of goats at once.")]
    [SerializeField]
    private int _maximumGoats = 2;
    /// <summary>
    /// Object which holds spawn points. Spawn points will also be used as navigation points.
    /// </summary>
    [Tooltip("Object which holds spawn points. Spawn points will also be used as navigation points.")]
    [SerializeField]
    private Transform _spawnPointsParent;

    /// <summary>
    /// Currently spawned goats.
    /// </summary>
    private List<GameObject> _spawnedGoats = new List<GameObject>();
    /// <summary>
    /// Found spawn points.
    /// </summary>
    private SpawnPoint[] _spawnPoints = new SpawnPoint[0];

    public override void OnStartServer()
    {
        base.OnStartServer();
        _spawnPoints = _spawnPointsParent.GetComponentsInChildren<SpawnPoint>();
    }

    private void FixedUpdate()
    {
        if (base.IsServer)
        {
            CheckSpawnGoats();
        }
    }

    /// <summary>
    /// Checks if goats need to be spawned.
    /// </summary>
    private void CheckSpawnGoats()
    {
        if (_spawnPoints.Length == 0)
            return;

        for (int i = 0; i < _spawnedGoats.Count; i++)
        {
            if (_spawnedGoats[i] == null)
            {
                _spawnedGoats.RemoveAt(i);
                i--;
            }
        }
        int spawnCount = _maximumGoats - _spawnedGoats.Count;
        if (spawnCount <= 0)
            return;
        
        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawn = _spawnPoints[Random.Range(0, _spawnPoints.Length - 1)].transform;
            GameObject go = Instantiate(_goatPrefab, spawn.position, spawn.rotation);
            Goat goat = go.GetComponent<Goat>();
            goat.FirstInitialize(_spawnPoints);
            base.Spawn(go);

            _spawnedGoats.Add(go);
        }
    }
}
