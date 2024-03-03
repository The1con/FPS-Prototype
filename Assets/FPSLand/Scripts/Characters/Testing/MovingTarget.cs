//
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class MovingTarget : NetworkBehaviour
//{
//    [SerializeField]
//    private float _moveRate = 3f;

//    private Vector3[] _goals;
//    private bool _movingRight;

//    private Coroutine _pause = null;


//    public override void OnStartServer()
//    {
//        base.OnStartServer();
//        _goals = new Vector3[2]
//        {
//            transform.position + new Vector3(-1f, 0f, 0f),
//            transform.position + new Vector3(1f, 0f, 0f)
//        };
//    }


//    private void FixedUpdate()
//    {
//        if (base.IsServer)
//            RpcSetTargetPosition(transform.position);
//    }

//    private Vector3? _targetPosition = null;

//    private void Update()
//    {
//        if (base.IsServer)
//        {
//            Vector3 goal = (_movingRight) ? _goals[1] : _goals[0];
//            transform.position = Vector3.MoveTowards(transform.position, goal, _moveRate * Time.deltaTime);
//            if (transform.position == goal)
//                _movingRight = !_movingRight;
//        }
//        if (base.IsClient && _targetPosition != null)
//        {
//            float distance = Vector3.Distance(transform.position, _targetPosition.Value);
//            transform.position = Vector3.MoveTowards(transform.position, _targetPosition.Value, Time.deltaTime / Time.fixedDeltaTime * distance);
//        }
//    }

//    [ObserversRpc]
//    private void RpcSetTargetPosition(Vector3 target)
//    {
//        //Don't set position if server.
//        if (base.IsServer)
//            return;
//        _targetPosition = target;
//    }

//    public void PauseMoving(float value)
//    {
//        if (_pause != null)
//            return;

//        _pause = StartCoroutine(__PauseMoving(value));
//    }
//    public IEnumerator __PauseMoving(float value)
//    {
//        this.enabled = false;
//        yield return new WaitForSeconds(value);

//        this.enabled = true;
//        _pause = null;
//    }
//}
