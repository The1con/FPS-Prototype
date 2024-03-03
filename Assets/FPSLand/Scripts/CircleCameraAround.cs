using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleCameraAround : MonoBehaviour
{
    [System.Serializable]
    private class FOVChanges
    {
        public float FOV;
        public float Time;
    }

    [SerializeField]
    private AudioSource _audio;
    [SerializeField]
    private float _moveRate = 20f;
    [SerializeField]
    private float _fovRate = 2f;

    [SerializeField]
    private List<FOVChanges> _fovChanges = new List<FOVChanges>();
    private int _fovChangeIndex = 0;
    private Camera _camera;
    private float _targetFov = 40f;
    private void Start()
    {
        _camera = GetComponent<Camera>();
        if (_camera != null)
        {
            //Vector3 original = _camera.transform.position;
            //_camera.transform.position = transform.position - (Vector3.forward * 20f);
            //_camera.transform.position = new Vector3(_camera.transform.position.x, original.y, _camera.transform.position.z);
        }
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (_audio != null)
                _audio.gameObject.SetActive(true);
            Time.timeScale = 1f;
        }
        if (Time.timeScale == 0f)
            return;

        if (_camera == null)
            return;

        _camera.transform.RotateAround(transform.position, Vector3.up, _moveRate * Time.deltaTime);

        for (int i = _fovChangeIndex; i < _fovChanges.Count; i++)
        {
            if (Time.time > _fovChanges[i].Time)
            {
                _targetFov = _fovChanges[i].FOV;
                _fovChangeIndex++;
                break;
            }
        }
        float dist = Mathf.Abs(_targetFov - _camera.fieldOfView);
        _camera.fieldOfView = Mathf.MoveTowards(_camera.fieldOfView, _targetFov, _fovRate * dist * Time.deltaTime);
    }
}
