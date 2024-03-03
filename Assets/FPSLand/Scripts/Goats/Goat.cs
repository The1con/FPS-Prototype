using FirstGearGames.FPSLand.Characters.Vitals;
using FirstGearGames.FPSLand.Managers.Gameplay;
using FirstGearGames.Managers.Global;
using FishNet.Object;
using GameKit.Utilities;
using GameKit.Utilities.Types;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Goat : NetworkBehaviour
{
    /// <summary>
    /// Audio to play before detonation.
    /// </summary>
    [Tooltip("Audio to play before detonation.")]
    [SerializeField]
    private AudioClip _screamAudio;
    /// <summary>
    /// How close the goat must be within a player to persue them.
    /// </summary>
    [Tooltip("How close the goat must be within a player to persue them.")]
    [SerializeField]
    private float _aggroRange = 7f;
    /// <summary>
    /// How close the goat must be before detonation.
    /// </summary>
    [Tooltip("How close the goat must be before detonation.")]
    [SerializeField]
    private float _detonationProximity = 3f;
    /// <summary>
    /// Radius of damage.
    /// </summary>
    [Tooltip("Radius of damage.")]
    [SerializeField]
    private float _damageRadius = 5f;
    /// <summary>
    /// Damage which may be dealt based on vicinity of detonation.
    /// </summary>
    [Tooltip("Damage which may be dealt based on vicinity of detonation.")]
    [SerializeField]
    private FloatRange _damageRange = new FloatRange(1, 2);
    /// <summary>
    /// Object to show when detonating.
    /// </summary>
    [Tooltip("Object to show for detonating.")]
    [SerializeField]
    private GameObject _detonateObject;
    /// <summary>
    /// Various goat models to use.
    /// </summary>
    [Tooltip("Various goat models to use.")]
    [SerializeField]
    private GameObject[] _models;
    /// <summary>
    /// Index of which goat to show.
    /// </summary>
    private int _goatIndex = -1;

    /// <summary>
    /// Next time goat should search for a player.
    /// </summary>
    private float _nextSearchTime = 0f;
    /// <summary>
    /// NavAgent on this object.
    /// </summary>
    private NavMeshAgent _navAgent;
    /// <summary>
    /// NavMeshPath to use.
    /// </summary>
    private NavMeshPath _path;
    /// <summary>
    /// Position to move towards.
    /// </summary>
    private SpawnPoint[] _wayPoints = new SpawnPoint[0];
    /// <summary>
    /// Next time goat can roam.
    /// </summary>
    private float _nextRoamTime = 0f;
    /// <summary>
    /// Next time goat call can occur.
    /// </summary>
    private float _nextGoatCall = -1f;
    /// <summary>
    /// AudioSource on this object.
    /// </summary>
    private AudioSource _audioSource;
    /// <summary>
    /// Target moving towards.
    /// </summary>
    private Transform _target = null;
    /// <summary>
    /// True if initialized.
    /// </summary>
    private bool _active = false;
    /// <summary>
    /// True to send goat index to clients this game.
    /// </summary>
    private bool _sendGoatIndex = false;

    private const float SEARCH_INTERVAL = 0.5f;
    private const float ROAM_INTERVAL = 7f;
    private const float GOAT_CALL_INTERVAL = 5f;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _nextGoatCall = Time.time + Random.Range(0f, GOAT_CALL_INTERVAL);
        _active = true;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        _navAgent = GetComponent<NavMeshAgent>();
        //Setup goat to show.
        SetGoatIndex(Random.Range(0, _models.Length));

        _path = new NavMeshPath();
    }

    private void Update()
    {
        if (!_active || _goatIndex == -1)
            return;

        if (base.IsServer)
        {
            SearchForCharacters();
            CheckRoam();
            CheckDetonate();
            CheckSendGoatIndex();
        }
        if (base.IsClient)
        {
            CheckGoatCall();
        }
    }

    /// <summary>
    /// Initializes this script for use. Should only be completed once.
    /// </summary>
    public void FirstInitialize(SpawnPoint[] waypoints)
    {
        _wayPoints = waypoints;
    }

    /// <summary>
    /// Called when GoatIndex changes.
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="next"></param>
    private void SetGoatIndex(int next)
    {
        _goatIndex = next;
        if (next >= 0 && next < _models.Length)
        {
            for (int i = 0; i < _models.Length; i++)
                _models[i].SetActive(next == i);
        }

        if (base.IsServer)
            _sendGoatIndex = true;
    }

    /// <summary>
    /// Searches for characters to follow.
    /// </summary>
    private void SearchForCharacters()
    {
        if (_path == null)
            return;
        if (Time.time < _nextSearchTime)
            return;

        _path.ClearCorners();
        _nextSearchTime = Time.time + SEARCH_INTERVAL;
        Collider[] hits = Physics.OverlapSphere(transform.position, _aggroRange, GlobalManager.LayerManager.CharacterLayer);
        if (hits.Length > 0)
        {
            hits = hits.OrderBy(x => Vector3.SqrMagnitude(transform.position - x.transform.position)).ToArray();
#pragma warning disable CS0162 // Unreachable code detected
            for (int i = 0; i < hits.Length; i++)
#pragma warning restore CS0162 // Unreachable code detected
            {
                _target = hits[i].transform;
                _navAgent.SetDestination(_target.position);
                _nextRoamTime = Time.time + ROAM_INTERVAL + SEARCH_INTERVAL;
                return;
            }
        }

        //No viable hits.
        //If target existed previously then reset next roam time.
        if (_target != null)
            _nextRoamTime = 0f;

        _target = null;
    }

    /// <summary>
    /// Checks if goat can roam.
    /// </summary>
    private void CheckRoam()
    {
        if (_wayPoints.Length == 0)
            return;
        if (Time.time < _nextRoamTime)
            return;
        _nextRoamTime = Time.time + ROAM_INTERVAL;

        _wayPoints.Shuffle();
        for (int i = 0; i < _wayPoints.Length; i++)
        {
            //Don't goto spawn point if close.
            if (Vector3.Distance(transform.position, _wayPoints[i].transform.position) < 20f)
            {
                continue;
            }
            //Far enough to roam towards.
            else
            {
                _navAgent.SetDestination(_wayPoints[i].transform.position);
                return;
            }
        }
    }

    /// <summary>
    /// Checks if server needs to send goat index.
    /// </summary>
    private void CheckSendGoatIndex()
    {
        if (!_sendGoatIndex)
            return;

        _sendGoatIndex = false;
        ObserversSetGoatIndex(_goatIndex);
    }

    /// <summary>
    /// Checks if a goat call can be made.
    /// </summary>
    private void CheckGoatCall()
    {
        if (_audioSource == null)
            return;
        if (Time.time < _nextGoatCall)
            return;

        _nextGoatCall = Time.time + GOAT_CALL_INTERVAL.Variance(0.25f);
        _audioSource.pitch = 1f.Variance(0.25f);
        _audioSource.Play();
    }

    /// <summary>
    /// Checks to detonate the goat.
    /// </summary>
    private void CheckDetonate()
    {
        if (_target == null)
            return;
        if (!_active)
            return;
        //Out of range.
        if (Vector3.Distance(transform.position, _target.position) > _detonationProximity)
            return;

        _active = false;

        ObserversStartDetonate();
        StartCoroutine(__Detonate());
    }


    /// <summary>
    /// Starts detonation effects on client.
    /// </summary>
    [ObserversRpc]
    private void ObserversStartDetonate()
    {
        if (base.IsServer)
            return;

        StartCoroutine(__Detonate());
    }

    /// <summary>
    /// Detonates the goat.
    /// </summary>
    /// <returns></returns>
    private IEnumerator __Detonate()
    {
        //Lazily stop future goat calls.
        _nextGoatCall = Time.time + 999f;
        //Play scream audio.
        _audioSource.clip = _screamAudio;
        _audioSource.Play();

        //Increase move speed.
        if (base.IsServer)
        _navAgent.speed *= 2f;

        //Wait a set amount of time for scream audio to finish.
        yield return new WaitForSeconds(1.75f);

        if (base.IsServer)
            _navAgent.isStopped = true;

        _detonateObject.SetActive(true);

        //Hide all renderers.
        for (int i = 0; i < _models.Length; i++)
            _models[i].SetActive(false);

        //If server wait a moment before destroying the goat.
        if (base.IsServer)
        {
            //Trace for players 
            Collider[] hits = Physics.OverlapSphere(transform.position, _damageRadius, GlobalManager.LayerManager.CharacterLayer);
            for (int i = 0; i < hits.Length; i++)
            {
                Health h = hits[i].GetComponent<Health>();
                if (h != null)
                {
                    //Get damage based on distance from explosion.
                    float percent = 1f - Mathf.InverseLerp(0f, _damageRadius, Vector3.Distance(transform.position, hits[i].transform.position));
                    int damage = Mathf.CeilToInt(
                        Mathf.Lerp(_damageRange.Minimum, _damageRange.Maximum, percent));

                    h.RemoveHealth(damage);
                }
            }

            yield return new WaitForSeconds(3f);
            base.Despawn();
        }
    }


    /// <summary>
    /// Sets the goat index on all clients.
    /// </summary>
    /// <param name="index"></param>
    [ObserversRpc(BufferLast = true)]
    private void ObserversSetGoatIndex(int index)
    {
        SetGoatIndex(index);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, _damageRadius);
    }
}
