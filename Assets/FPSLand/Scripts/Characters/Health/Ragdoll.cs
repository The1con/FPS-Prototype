using FirstGearGames.FPSLand.Characters.Bodies;
using UnityEngine;

namespace FirstGearGames.FPSLand.Characters.Vitals
{



    public class Ragdoll : MonoBehaviour
    {
        #region Types.
        /// <summary>
        /// Stores local space information of a rigidbody.
        /// </summary>
        private class RigidbodySpace
        {
            public RigidbodySpace(Rigidbody rigidbody)
            {
                Rigidbody = rigidbody;
                LocalPosition = rigidbody.transform.localPosition;
                LocalRotation = rigidbody.transform.localRotation;
            }

            public readonly Rigidbody Rigidbody;
            public readonly Vector3 LocalPosition;
            public readonly Quaternion LocalRotation;
        }
        #endregion

        #region Serialized.
        /// <summary>
        /// Rigidbodies which are used for the ragdoll effect.
        /// </summary>
        [Tooltip("Rigidbodies which are used for the ragdoll effect.")]
        [SerializeField]
        private Rigidbody[] _rigidbodies = new Rigidbody[0];
        #endregion

        #region Private.
        /// <summary>
        /// Localspace values for rigidbodies on Start.
        /// </summary>
        private RigidbodySpace[] _rigidbodySpaces = new RigidbodySpace[0];
        #endregion

        private void Awake()
        {
            FirstInitialize();    
        }

        private void Start()
        {
            SecondInitialize();
        }

        /// <summary>
        /// Initializes this script for use. Should only be completed once.
        /// </summary>
        private void FirstInitialize()
        {
            Health health = GetComponent<Health>();
            health.OnDeath += Health_OnDeath;
            health.OnRespawned += Health_OnRespawn;
        }

        /// <summary>
        /// Initializes this script for use. Should onyl be completed once.
        /// </summary>
        private void SecondInitialize()
        {
            _rigidbodySpaces = new RigidbodySpace[_rigidbodies.Length];
            for (int i = 0; i < _rigidbodies.Length; i++)
                _rigidbodySpaces[i] = new RigidbodySpace(_rigidbodies[i]);
        }

        /// <summary>
        /// Received when character is respawned.
        /// </summary>
        private void Health_OnRespawn()
        {
            SetRigidbodiesKinematic(true);
            //Also set rigidbodies back to original space.
            for (int i = 0; i < _rigidbodySpaces.Length; i++)
            {
                _rigidbodySpaces[i].Rigidbody.transform.localPosition = _rigidbodySpaces[i].LocalPosition;
                _rigidbodySpaces[i].Rigidbody.transform.localRotation = _rigidbodySpaces[i].LocalRotation;
            }
        }

        /// <summary>
        /// Received when character is dead.
        /// </summary>
        private void Health_OnDeath()
        {
            SetRigidbodiesKinematic(false);
        }

        /// <summary>
        /// Activities rigidbodies for ragdoll.
        /// </summary>
        public void SetRigidbodiesKinematic(bool kinematic)
        {
            for (int i = 0; i < _rigidbodies.Length; i++)
                _rigidbodies[i].isKinematic = kinematic;
        }

    }


}