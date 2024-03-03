using GameKit.CameraShakers;
using UnityEngine;

namespace FirstGearGames.FPSLand.Characters.Weapons
{

    public class RecoilIK : MonoBehaviour
    {
        #region Public.
        /// <summary>
        /// ObjectShaker on this object.
        /// </summary>
        public ObjectShaker ObjectShaker { get; private set; }
        #endregion

        #region Private.
        /// <summary>
        /// Rightvhand bone for the avatar.
        /// </summary>
        private Transform _rightHandBone;
        /// <summary>
        /// Left hand bone for the avatar.
        /// </summary>
        private Transform _leftHandBone;
        /// <summary>
        /// Animator component on this object.
        /// </summary>
        private Animator _animator;
        /// <summary>
        /// IKWeight to use.
        /// </summary>
        private float _ikWeight = 0f;
        /// <summary>
        /// Sets IKWeight to use.
        /// </summary>
        /// <param name="value"></param>
        public void SetIKWeight(float value)
        {
            _ikWeight = value;
        }
        #endregion

        private ShakeUpdate _lastShakeUpdate;

        private void Awake()
        {
            FirstInitialize();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            //Only set for UpperBody
            if (layerIndex != 1)
                return;

            SetIKWeights(_ikWeight);
            UpdateRecoil(ref _lastShakeUpdate);            
        }

        /// <summary>
        /// Sets weight to use for IK.
        /// </summary>
        /// <param name="value"></param>
        private void SetIKWeights(float value)
        {
            //Right hand.
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, value);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, value);
            //Left hand.
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, value);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, value);
        }

        /// <summary>
        /// Initializes this script for use. Should only be completed once.
        /// </summary>
        private void FirstInitialize()
        {
            ObjectShaker = GetComponent<ObjectShaker>();
            //Get bones.
            _animator = GetComponent<Animator>();
            _rightHandBone = _animator.GetBoneTransform(HumanBodyBones.RightHand);
            _leftHandBone = _animator.GetBoneTransform(HumanBodyBones.LeftHand);

            //If each hand is found enable weights.
            if (_leftHandBone != null && _rightHandBone != null)
            {
                //Also listen to ObjectShaker.
                ObjectShaker shaker = GetComponent<ObjectShaker>();
                shaker.OnShakeUpdate += Shaker_OnShakeUpdate;
            }
            //Else disable script.
            else
            {
                this.enabled = false;
            }
        }

        /// <summary>
        /// Received when a shake update occurs.
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        private void Shaker_OnShakeUpdate(ObjectShaker shaker, ShakeUpdate update)
        {
            _lastShakeUpdate = update;
        }

        /// <summary>
        /// Updates right and left springs to their next value.
        /// </summary>
        private void UpdateRecoil(ref ShakeUpdate update)
        {
            if (update == null)
                return;

            AvatarIKGoal[] ikGoals = new AvatarIKGoal[] { AvatarIKGoal.LeftHand, AvatarIKGoal.RightHand };
            for (int i = 0; i < ikGoals.Length; i++)
            {
                Vector3 pos;
                Quaternion rot;

                //Position.
                if (update == null)
                    pos = _animator.GetIKPosition(ikGoals[i]);
                else
                    pos = _animator.GetIKPosition(ikGoals[i]) + transform.TransformDirection(update.Objects.Position);
                //Rotation.
                if (update == null)
                    rot = _animator.GetIKRotation(ikGoals[i]);
                else
                    rot = _animator.GetIKRotation(ikGoals[i]) * Quaternion.Euler(update.Objects.Rotation);

                _animator.SetIKPosition(ikGoals[i], pos);
                _animator.SetIKRotation(ikGoals[i], rot);
            }

            //No offset.
            if (update.Objects.Position == Vector3.zero && update.Objects.Rotation == Vector3.zero)
                update = null;
        }
    }


}