using FirstGearGames.FPSLand.Audios;
using GameKit.Utilities.ObjectPooling;
using UnityEngine;

namespace FirstGearGames.FPSLand.Managers
{

    public class AudioManager : MonoBehaviour
    {
        #region Serialized.
        /// <summary>
        /// Prefab to spawn for OneShotAudio.
        /// </summary>
        [Tooltip("Prefab to spawn for OneShotAudio.")]
        [SerializeField]
        private OneShotAudio _oneShotAudioPrefab;
        #endregion

        #region Private.
        /// <summary>
        /// Reference to the camera used for first person.
        /// </summary>
        private Transform _firstPersonCamera;
        /// <summary>
        /// Sets reference to the camera used for first person.
        /// </summary>
        /// <param name="t"></param>
        public void SetFirstPersonCamera(Transform t)
        {
            _firstPersonCamera = t;
        }
        #endregion

        /// <summary>
        /// Plays a clip for first person using OneShotAudioPrefab.
        /// </summary>
        /// <param name="clip"></param>
        public OneShotAudio PlayFirstPerson(AudioClip clip)
        {
            if (_firstPersonCamera == null)
                return null;
            if (clip == null)
                return null;

            //Instantiate the audio and play it.
            OneShotAudio osa = ObjectPool.Retrieve<OneShotAudio>(_oneShotAudioPrefab.gameObject);
            osa.transform.SetParent(_firstPersonCamera);
            osa.transform.localPosition = Vector3.zero;
            osa.Play(clip);

            return osa;
        }

        /// <summary>
        /// Plays an audio at point using OneShotAudioPrefab.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="position"></param>
        public OneShotAudio PlayAtPoint(AudioClip clip, Vector3 position)
        {
            OneShotAudio osa = ObjectPool.Retrieve<OneShotAudio>(_oneShotAudioPrefab.gameObject, position, Quaternion.identity);
            osa.Play(clip);

            return osa;
        }

        /// <summary>
        /// Spawns an audio prefab on the first person camera.
        /// </summary>
        /// <param name="prefab"></param>
        public GameObject PlayFirstPerson(GameObject prefab)
        {
            if (_firstPersonCamera == null)
                return null;
            if (prefab == null)
                return null;

            GameObject r = ObjectPool.Retrieve(prefab);
            r.transform.SetParent(_firstPersonCamera);
            r.transform.localPosition = Vector3.zero;

            return r;
        }


        /// <summary>
        /// Spawns an audio prefab at point.
        /// </summary>
        /// <param name="prefab"></param>
        public GameObject PlayAtPoint(GameObject prefab, Vector3 position)
        {
            if (prefab == null)
                return null;

            return ObjectPool.Retrieve(prefab, position, Quaternion.identity);
        }

    }

}
