using GameKit.Utilities.ObjectPooling;
using UnityEngine;

namespace FirstGearGames.FPSLand.Weapons
{

    public class DestroyAfterDelay : MonoBehaviour
    {
        [SerializeField]
        private bool _usePool = false;
        [SerializeField]
        private float _delay = 0.5f;

        private float _destroyTime;

        private void OnEnable()
        {
            _destroyTime = Time.time + _delay;
        }

        private void OnDisable()
        {
            _destroyTime = -1f;
        }
        private void Update()
        {
            if (_destroyTime == -1f)
                return;
            if (Time.time < _destroyTime)
                return;

            if (_usePool)
                ObjectPool.Store(gameObject);
            else
                Destroy(gameObject);
        }


    }


}