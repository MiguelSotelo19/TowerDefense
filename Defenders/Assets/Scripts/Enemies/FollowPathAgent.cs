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
    // Permite configurar el progreso normalizado sin usar reflection
    public void SetProgress(float t)
    {
        _t = Mathf.Clamp01(t);
    }

    // Devuelve la posición en mundo del spline para un progreso dado
    public Vector3 GetWorldPositionAtProgress(float t)
    {
        if (_currentPath == null || splineContainer == null)
            return transform.position;

        t = Mathf.Clamp01(t);
        Vector3 localPos = _currentPath.EvaluatePosition(t);
        return splineContainer.transform.TransformPoint(localPos);
    }

    // Devuelve la rotación (mundo) alineada con la tangente en el progreso t
    public Quaternion GetWorldRotationAtProgress(float t)
    {
        if (_currentPath == null || splineContainer == null)
            return transform.rotation;

        t = Mathf.Clamp01(t);
        Vector3 tangent = _currentPath.EvaluateTangent(t); // tangente en espacio local del spline
        Vector3 worldTangent = splineContainer.transform.TransformDirection(tangent);
        if (worldTangent.sqrMagnitude <= 0.0001f) return transform.rotation;
        return Quaternion.LookRotation(worldTangent);
    }

    // Método comodín que restaura posición + rotación + progreso en un solo llamado
    public void RestoreProgressAndSnap(float t)
    {
        SetProgress(t);

        if (_currentPath == null || splineContainer == null) return;

        Vector3 worldPos = GetWorldPositionAtProgress(t);
        Quaternion worldRot = GetWorldRotationAtProgress(t);

        // Pone al agente en la posición exacta del spline y alinea rotación
        transform.position = worldPos;
        transform.rotation = worldRot;

        if (_rb != null) _rb.linearVelocity = Vector3.zero;
    }

}
