using System;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class FollowPathAgent : MonoBehaviour
{
    private SplineContainer splineContainer;
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float interpolationSpeed = 20f;
    
    [Header("Obstacle Detection")]
    [SerializeField] private bool detectObstacles = true;
    [SerializeField] private float obstacleCheckDistance = 0.5f;
    [SerializeField] private LayerMask obstacleLayer = -1; // -1 = Everything
    [SerializeField] private float obstacleCheckRadius = 0.25f;

    private Vector3 _currentPosition;
    private Vector3 _tangent;
    private Spline _currentPath;
    private Rigidbody _rb;
    private float _t;
    
    private bool isBlockedByObstacle = false;
    private float blockedTime = 0f;
    private const float MAX_BLOCKED_TIME = 3f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.maxLinearVelocity = movementSpeed;

        // Evitar errores si no se asignó splineContainer
        if (splineContainer != null && splineContainer.Splines.Count > 0)
            _currentPath = splineContainer.Splines[0];
        else
            Debug.LogWarning($"{name} no tiene SplineContainer asignado en FollowPathAgent.");
    }

    private void Update()
    {
        if (_currentPath == null) return;

        // Verificar si hay obstáculos adelante
        if (detectObstacles && CheckForObstacles())
        {
            // Hay un obstáculo, detener el movimiento
            isBlockedByObstacle = true;
            blockedTime += Time.deltaTime;
            
            // Detener completamente
            _rb.linearVelocity = Vector3.zero;
            
            // Si lleva mucho tiempo bloqueado, intentar empujar hacia los lados
            if (blockedTime > MAX_BLOCKED_TIME)
            {
                TryMoveAroundObstacle();
            }
            
            return;
        }
        else
        {
            // No hay obstáculos, moverse normalmente
            isBlockedByObstacle = false;
            blockedTime = 0f;
        }
        
        _t = (_t + Time.deltaTime * movementSpeed / _currentPath.GetLength()) % 1f;
        _tangent = _currentPath.EvaluateTangent(_t);

        var targetRotation = Quaternion.LookRotation(_tangent);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, interpolationSpeed * Time.deltaTime);

        _rb.AddForce(transform.forward * movementSpeed, ForceMode.VelocityChange);
    }
    
    private bool CheckForObstacles()
    {
        // Raycast adelante para detectar obstáculos
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        
        // Usar SphereCast para mejor detección
        if (Physics.SphereCast(origin, obstacleCheckRadius, transform.forward, out hit, obstacleCheckDistance, obstacleLayer))
        {
            if (hit.collider.CompareTag("Firewall") || hit.collider.name.Contains("Firewall"))
            {
                return true;
            }
        }
        
        // Raycast adicional a nivel del suelo
        if (Physics.Raycast(transform.position, transform.forward, out hit, obstacleCheckDistance, obstacleLayer))
        {
            if (hit.collider.CompareTag("Firewall") || hit.collider.name.Contains("Firewall"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private void TryMoveAroundObstacle()
    {
        // Intentar moverse ligeramente hacia los lados para desatascarse
        Vector3 sideDirection = transform.right * Mathf.Sin(Time.time * 2f);
        _rb.AddForce(sideDirection * movementSpeed * 0.3f, ForceMode.Force);
        
        blockedTime = 0f;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Detección adicional por colisión física
        if (collision.gameObject.CompareTag("Firewall") || collision.gameObject.name.Contains("Firewall"))
        {
            _rb.linearVelocity = Vector3.zero;
            isBlockedByObstacle = true;
        }
    }
    
    private void OnCollisionStay(Collision collision)
    {
        // Mantener detenido mientras esté en contacto con el firewall
        if (collision.gameObject.CompareTag("Firewall") || collision.gameObject.name.Contains("Firewall"))
        {
            _rb.linearVelocity = Vector3.zero;
            isBlockedByObstacle = true;
        }
    }
    
    private void OnCollisionExit(Collision collision)
    {
        // Cuando deje de tocar el firewall, permitir movimiento
        if (collision.gameObject.CompareTag("Firewall") || collision.gameObject.name.Contains("Firewall"))
        {
            isBlockedByObstacle = false;
            blockedTime = 0f;
        }
    }

    public void AssignSplineContainer(SplineContainer container)
    {
        splineContainer = container;
        if (splineContainer != null && splineContainer.Splines.Count > 0)
            _currentPath = splineContainer.Splines[0];
        else
            _currentPath = null;

        _t = 0f; 
    }

    public void ResetProgress(bool keepWorldPosition = true)
    {
        _t = 0f;

        if (_currentPath != null)
        {
            if (!keepWorldPosition)
            {
                // Obtener posición local del spline
                //Pasa de posicion local del spline
                Vector3 localPosition = _currentPath.EvaluatePosition(0f);

                //A posicion global
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
            
        isBlockedByObstacle = false;
        blockedTime = 0f;
    }
    
    // Permite configurar el progreso normalizado sin usar reflection
    public void SetProgress(float t)
    {
        _t = Mathf.Clamp01(t);
    }

    public Vector3 GetWorldPositionAtProgress(float t)
    {
        if (_currentPath == null || splineContainer == null)
            return transform.position;

        t = Mathf.Clamp01(t);
        Vector3 localPos = _currentPath.EvaluatePosition(t);
        return splineContainer.transform.TransformPoint(localPos);
    }

    public Quaternion GetWorldRotationAtProgress(float t)
    {
        if (_currentPath == null || splineContainer == null)
            return transform.rotation;

        t = Mathf.Clamp01(t);
        Vector3 tangent = _currentPath.EvaluateTangent(t);
        Vector3 worldTangent = splineContainer.transform.TransformDirection(tangent);
        if (worldTangent.sqrMagnitude <= 0.0001f) return transform.rotation;
        return Quaternion.LookRotation(worldTangent);
    }

    public void RestoreProgressAndSnap(float t)
    {
        SetProgress(t);

        if (_currentPath == null || splineContainer == null) return;

        Vector3 worldPos = GetWorldPositionAtProgress(t);
        Quaternion worldRot = GetWorldRotationAtProgress(t);

        transform.position = worldPos;
        transform.rotation = worldRot;

        if (_rb != null) _rb.linearVelocity = Vector3.zero;
        
        isBlockedByObstacle = false;
        blockedTime = 0f;
    }
    
    // Método para visualizar el raycast en el editor (solo cuando está seleccionado)
    private void OnDrawGizmosSelected()
    {
        if (!detectObstacles) return;
        
        Gizmos.color = isBlockedByObstacle ? Color.red : Color.green;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Gizmos.DrawLine(origin, origin + transform.forward * obstacleCheckDistance);
        Gizmos.DrawWireSphere(origin + transform.forward * obstacleCheckDistance, obstacleCheckRadius);
    }
}