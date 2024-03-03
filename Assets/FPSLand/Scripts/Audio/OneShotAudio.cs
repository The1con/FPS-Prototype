
using GameKit.Utilities.ObjectPooling;
using UnityEngine;

namespace FirstGearGames.FPSLand.Audios
{ 


public class OneShotAudio : MonoBehaviour
{
        #region Serialized.
        /// <summary>
        /// AudioSource on this object.
        /// </summary>
        [Tooltip("AudioSource on this object.")]
        [SerializeField]
        private AudioSource _audioSource;
        #endregion

        #region Private.
        /// <summary>
        /// Unscaled time when audio will end.
        /// </summary>
        private float _audioEndTime = -1f;
        #endregion

        private void Update()
        {
            //Audio end time is not set.
            if (_audioEndTime == -1f || Time.unscaledTime > _audioEndTime)
            {
                ObjectPool.Store(gameObject);
                return;
            }
        }

        /// <summary>
        /// Plays an audio clip once.
        /// </summary>
        /// <param name="clip"></param>
        public void Play(AudioClip clip)
        {
            if (clip == null)
                return;

            //Set when the clip should end.
            _audioEndTime = Time.unscaledTime + clip.length;

            _audioSource.clip = clip;
            _audioSource.Play();
        }


}


}