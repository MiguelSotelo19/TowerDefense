using System;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class FollowPathAgent : MonoBehaviour
{
    private SplineContainer splineContainer;
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

        // Evitar errores si no se asign� splineContainer
        if (splineContainer != null && splineContainer.Splines.Count > 0)
            _currentPath = splineContainer.Splines[0];
        else
            Debug.LogWarning($"{name} no tiene SplineContainer asignado en FollowPathAgent.");
    }

    private void Update()
    {
        if (_currentPath == null) return;

        _t += Time.deltaTime * movementSpeed / _currentPath.GetLength();
        _tangent = _currentPath.EvaluateTangent(_t);

        var targetRotation = Quaternion.LookRotation(_tangent);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, interpolationSpeed * Time.deltaTime);

        _rb.AddForce(transform.forward * movementSpeed, ForceMode.VelocityChange);
    }

    public void AssignSplineContainer(SplineContainer container)
    {
        splineContainer = container;
        if (splineContainer != null && splineContainer.Splines.Count > 0)
            _currentPath = splineContainer.Splines[0];
        else
            _currentPath = null;

        _t = 0f; // reinicia el progreso del movimiento
    }

    public void ResetProgress(bool keepWorldPosition = true)
    {
        _t = 0f;

        if (_currentPath != null)
        {
            if (!keepWorldPosition)
            {
                // ← CAMBIO AQUÍ ←
                // Obtener posición local del spline
                Vector3 localPosition = _currentPath.EvaluatePosition(0f);
                
                // Convertir a posición mundial usando el transform del SplineContainer
                if (splineContainer != null)
                {
                    transform.position = splineContainer.transform.TransformPoint(localPosition);
                }
                else
                {
                    transform.position = localPosition;
                }
            }

            _tangent = _currentPath.EvaluateTangent(0f);
            transform.rotation = Quaternion.LookRotation(_tangent);
        }

        if (_rb != null)
            _rb.linearVelocity = Vector3.zero;
    }

}
