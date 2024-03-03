using GameKit.Utilities;
using GameKit.Utilities.ObjectPooling;
using GameKit.Utilities.Types;
using UnityEngine;


namespace FirstGearGames.FPSLand.Audios
{


    [RequireComponent(typeof(AudioSource))]
    public class EnhancedAudio : MonoBehaviour
    {
        #region Types.
        /// <summary>
        /// Order options for audio clips.
        /// </summary>
        private enum PlayOrders
        {
            First = 0,
            Random = 1,
            InOrder = 2
        }
        #endregion

        #region Serialized.
        /// <summary>
        /// True to automatically destroy this object once clip has played.
        /// </summary>
        [Tooltip("True to automatically destroy this object once clip has played.")]
        [SerializeField]
        private bool _autoDestroy = true;
        /// <summary>
        /// Order to play possible audio clips.
        /// </summary>
        [Tooltip("Order to play possible audio clips.")]
        [SerializeField]
        private PlayOrders _playOrder = PlayOrders.Random;
        /// <summary>
        /// Percent values to vary starting pitch by per play. Use 0f or leave unset to disable this feature.
        /// </summary>
        [Tooltip("Percent values to vary starting pitch by per play. Use 0f or leave unset to disable this feature.")]
        [SerializeField]
        private FloatRange _pitchRange = new FloatRange(1f, 1f);
        /// <summary>
        /// Collection of audio clips which may play.
        /// </summary>
        [Tooltip("Collection of audio clips which may play.")]
        [SerializeField]
        private AudioClip[] _audioClips = new AudioClip[0];
        #endregion

        #region Private.
        /// <summary>
        /// AudioSource component on this object.
        /// </summary>
        private AudioSource _audioSource;
        /// <summary>
        /// Next AudioClip index to play.
        /// </summary>
        private int _nextAudioIndex = -1;
        /// <summary>
        /// Unscaled time when audio ends.
        /// </summary>
        private float _audioEndTime = -1f;
        /// <summary>
        /// Pitch of audio source when instantiated.
        /// </summary>
        private float _startPitch;
        /// <summary>
        /// Play on awake state of audio source when instantiated.
        /// </summary>
        private bool _playOnAwake;
        #endregion

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _startPitch = _audioSource.pitch;

            _playOnAwake = _audioSource.playOnAwake;
            _audioSource.playOnAwake = false;
            _audioSource.Stop();

            CheckAutoPopulateAudioClips();
        }

        private void OnEnable()
        {
            if (_playOnAwake)
                Play();
        }

        private void Update()
        {
            if (_autoDestroy && Time.unscaledTime > _audioEndTime)
            {
                ObjectPool.Store(gameObject);
                return;
            }
        }

        /// <summary>
        /// Checks if it's possible to auto populate available AudioClips. Populates AudioClips if AudioSource has a clip, and AudioClips is empty.
        /// </summary>
        private void CheckAutoPopulateAudioClips()
        {
            if (_audioSource.clip != null && _audioClips.Length == 0)
                _audioClips = new AudioClip[1] { _audioSource.clip };
        }

        /// <summary>
        /// Plays the audio source.
        /// </summary>
        /// <returns>Played clip length.</returns>
        public void Play()
        {
            //Assigns an audio clip using play order setting.
            AssignAudioClip();
            //No audio to play.
            if (_audioSource.clip == null)
                return;

            _audioEndTime = Time.unscaledTime + _audioSource.clip.length;

            VaryPitch();
            _audioSource.Play();
        }

        /// <summary>
        /// Varies the pitch using the configured settings.
        /// </summary>
        private void VaryPitch()
        {
            //Unset.
            if (_pitchRange.Minimum == 1f && _pitchRange.Maximum == 1f)
                return;

            float pitch = _startPitch * Floats.RandomInclusiveRange(_pitchRange.Minimum, _pitchRange.Maximum);
            _audioSource.pitch = pitch;
        }

        /// <summary>
        /// Assigns an audio clip to the AudioSource before playing it.
        /// </summary>
        private void AssignAudioClip()
        {
            //Assign next play index.
            switch (_playOrder)
            {
                case PlayOrders.First:
                    _nextAudioIndex = 0;
                    break;
                case PlayOrders.InOrder:
                    _nextAudioIndex++;
                    break;
                case PlayOrders.Random:
                    _nextAudioIndex = Ints.RandomExclusiveRange(0, _audioClips.Length);
                    break;
                default:
                    Debug.LogWarning("EnhancedAudio -> AssignAudioClip -> Unhandled PlayOrder.");
                    _nextAudioIndex = 0;
                    break;
            }

            //Reset index if out of bounds.
            if (_nextAudioIndex >= _audioClips.Length || _nextAudioIndex < 0)
                _nextAudioIndex = 0;

            _audioSource.clip = _audioClips[_nextAudioIndex];
        }
    }


}