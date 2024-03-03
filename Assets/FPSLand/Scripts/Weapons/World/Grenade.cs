using FirstGearGames.FPSLand.Managers.Gameplay;
using FirstGearGames.Managers.Global;
using FishNet.Managing.Logging;
using FishNet.Managing.Timing;
using FishNet.Object;
using GameKit.Utilities;
using UnityEngine;

namespace FirstGearGames.FPSLand.Weapons
{


    public class Grenade : NetworkBehaviour, IThrowable
    {
        #region Serialized.
        /// <summary>
        /// How long after spawn to detonate.
        /// </summary>
        [Tooltip("How long after spawn to detonate.")]
        [SerializeField]
        private float _detonationDelay = 3f;
        /// <summary>
        /// How long to ignore character layer after being thrown. This is to prevent the grenade from bouncing off throwing character.
        /// </summary>
        [Tooltip("How long to ignore character layer after being thrown. This is to prevent the grenade from bouncing off throwing character.")]
        [SerializeField]
        private float _ignoreCharacterLayerDuration = 0.1f;
        /// <summary>
        /// Audio to play when grenade bounces.
        /// </summary>
        [Tooltip("Audio to play when grenade bounces.")]
        [SerializeField]
        private GameObject _bounceAudioPrefab;
        /// <summary>
        /// How often bounce audio may play.
        /// </summary>
        [Tooltip("How often bounce audio may play.")]
        [SerializeField]
        private float _audioInterval = 0.2f;
        /// <summary>
        /// Minimum amount of velocity required for bounce audio to play.
        /// </summary>
        [Tooltip("Minimum amount of velocity required for bounce audio to play.")]
        [SerializeField]
        private float _minimumAudioVelocity = 2f;

        [Header("Physics")]
        /// <summary>
        /// Dampening rate.
        /// </summary>
        [Tooltip("Dampening rate.")]
        [SerializeField]
        private float _dampening = 0.05f;
        /// <summary>
        /// How much to bounce off default layer.
        /// </summary>
        [Tooltip("How much to bounce off default layer.")]
        [SerializeField]
        private float _defaultBounciness = 0.5f;
        /// <summary>
        /// How much to bounce off characters.
        /// </summary>
        [Tooltip("How much to bounce off characters.")]
        [SerializeField]
        private float _characterBounciness = 0.1f;
        #endregion

        #region Private.
        /// <summary>
        /// When to detonate.
        /// </summary>
        private float _detonationTime = -1f;
        /// <summary>
        /// Next time bounce audio is allowed to play.
        /// </summary>
        private float _nextAllowedBounceAudioTime = 0f;
        /// <summary>
        /// Time when to set layers back to normal.
        /// </summary>
        private float _reapplyLayerTime = -1f;
        /// <summary>
        /// Velocity of the grenade.
        /// </summary>
        private Vector3 _currentVelocity = Vector3.zero;
        /// <summary>
        /// Radius of collider on this object.
        /// </summary>
        private float _colliderRadius;
        #endregion

        public override void OnStartServer()
        {
            base.OnStartServer();
            base.TimeManager.OnTick += TimeManager_OnTick;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            base.TimeManager.OnTick -= TimeManager_OnTick;
        }

        protected virtual void Update()
        {
            PerformUpdate(false);
        }

        private void TimeManager_OnTick()
        {
            PerformUpdate(true);
        }

        [Server(Logging = LoggingType.Off)]
        private void PerformUpdate(bool onTick)
        {
            /* If a tick but also host then do not
             * update. The update will occur outside of
             * OnTick, using the update loop. */
            if (onTick && base.IsHost)
                return;
            /* If not called from OnTick and is server
             * only then exit. OnTick will handle movements. */
            else if (!onTick && base.IsServerOnly)
                return;

            float delta = (onTick) ? (float)base.TimeManager.TickDelta : Time.deltaTime;
            CheckReapplyLayer();
            //If host move every update for smooth movement. Otherwise move OnTick.
            Move(delta);
            CheckDetonate();
        }

        /// <summary>
        /// Initializes this script for use.
        /// </summary>
        [Server(Logging = LoggingType.Off)]
        public virtual void Initialize(PreciseTick pt, Vector3 force)
        {
            //No ignore time.
            if (_ignoreCharacterLayerDuration <= 0f)
                gameObject.layer = Layers.LayerMaskToLayerNumber(GlobalManager.LayerManager.DefaultLayer);
            //Has ignore time.
            else
                _reapplyLayerTime = Time.time + _ignoreCharacterLayerDuration;

            SphereCollider sc = GetComponent<SphereCollider>();
            _colliderRadius = sc.radius;
            _currentVelocity = force;
            /* Detonation time technically should be accelerated based on ping
             * of thrower, but this could cause grenades to detonate earlier than
             * expected on spectators, which isn't ideal. Rather than this, have the
             * grenade detonate slightly slower on thrower. */
            _detonationTime = Time.time + _detonationDelay;

            //Move ellapsed time from when grenade was 'thrown' on thrower.
            float timePassed = (float)base.TimeManager.TimePassed(pt);
            if (timePassed > Weapon.MAXIMUM_LATENCY_COMPENSATION)
                timePassed = Weapon.MAXIMUM_LATENCY_COMPENSATION;

            Move(timePassed);
        }

        /// <summary>
        /// Moves the grenade using CurrentVelocity.
        /// </summary>
        [Server(Logging = LoggingType.Off)]
        private void Move(float deltaTime)
        {
            //Apply gravity to velocity.
            _currentVelocity += (Physics.gravity * deltaTime);
            //Dampen velocity.
            _currentVelocity *= (1f - (_dampening * deltaTime));

            //Determine how far object should travel this frame.
            float travelDistance = (_currentVelocity.magnitude * Time.deltaTime);
            //Set trace distance to be travel distance + collider radius.
            float traceDistance = travelDistance + _colliderRadius;

            //Setup layermask to hit, and ray.
            LayerMask lm = (Time.time > _reapplyLayerTime) ? (GlobalManager.LayerManager.DefaultLayer | GlobalManager.LayerManager.CharacterLayer) : Layers.LayerMaskToLayerNumber(GlobalManager.LayerManager.DefaultLayer);
            Ray ray = new Ray(transform.position, _currentVelocity.normalized);
            RaycastHit hit;

            //If object is hit.
            if (Physics.Raycast(ray, out hit, traceDistance, lm))
            {
                float bounce;
                //If hit layer is a character.
                if (hit.collider.gameObject.layer == Layers.LayerMaskToLayerNumber(GlobalManager.LayerManager.CharacterLayer))
                    bounce = _characterBounciness;
                else
                    bounce = _defaultBounciness;

                _currentVelocity = Vector3.Reflect(_currentVelocity.normalized, hit.normal) * (bounce * _currentVelocity.magnitude);

                if (_currentVelocity.magnitude >= _minimumAudioVelocity)
                {
                    //Play audio.
                    if (Time.time < _nextAllowedBounceAudioTime)
                        return;
                    _nextAllowedBounceAudioTime = Time.time + _audioInterval;
                    ObserversPlayCollisionAudio();
                }
            }

            transform.position += (_currentVelocity * Time.deltaTime);
        }

        /// <summary>
        /// Checks if detonation should occur.
        /// </summary>
        [Server(Logging = LoggingType.Off)]
        protected void CheckDetonate()
        {
            if (_detonationTime == -1f)
                return;
            if (Time.time < _detonationTime)
                return;

            /* If here then detonate. */
            _detonationTime = -1f;

            Detonate();
        }

        /// <summary>
        /// Detonates the grenade.
        /// </summary>
        [Server(Logging = LoggingType.Off)]
        protected virtual void Detonate() { }

        /// <summary>
        /// Checks if it is time to reapply layers to starting.
        /// </summary>
        [Server(Logging = LoggingType.Off)]
        private void CheckReapplyLayer()
        {
            if (_reapplyLayerTime == -1f)
                return;
            if (Time.time < _reapplyLayerTime)
                return;

            _reapplyLayerTime = -1f;
            gameObject.layer = Layers.LayerMaskToLayerNumber(GlobalManager.LayerManager.DefaultLayer);
        }


        /// <summary>
        /// Players the collision audio.
        /// </summary>
        private void PlayCollisionAudio()
        {
            OfflineGameplayDependencies.AudioManager.PlayAtPoint(_bounceAudioPrefab, transform.position);
        }

        /// <summary>
        /// Plays collision audio on clients.
        /// </summary>
        [ObserversRpc]
        private void ObserversPlayCollisionAudio()
        {
            PlayCollisionAudio();
        }
    }


}