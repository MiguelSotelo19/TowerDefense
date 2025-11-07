using System;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class FollowPathAgent : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float interpolationSpeed = 20f;
    private Vector3 _currentPosition;
    private Vector3 _tangent;
    private Spline _currentPath;
    private Rigidbody _rb;
    private float _t;

    private void Start()
    {

        _rb = GetComponent<Rigidbody>();
        _rb.maxLinearVelocity = movementSpeed;
        _currentPath = splineContainer.Splines[0];

    }

    private void Update()
    {
        _t += Time.deltaTime * movementSpeed / _currentPath.GetLength();
        _tangent = _currentPath.EvaluateTangent(_t);
        var targetRotation = Quaternion.LookRotation(_tangent);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, interpolationSpeed * Time.deltaTime);
        _rb.AddForce(transform.forward * movementSpeed, ForceMode.VelocityChange);
    }
}
