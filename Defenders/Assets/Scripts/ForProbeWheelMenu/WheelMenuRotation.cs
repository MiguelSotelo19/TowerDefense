using UnityEngine;

public class WheelMenuRotation : MonoBehaviour
{
    [Header("Rotation Settings")]
    public bool enableRotation = false;
    public float rotationSpeed = 20f;
    public bool rotateClockwise = false;

    void Update()
    {
        if (!enableRotation) return;

        float direction = rotateClockwise ? -1f : 1f;
        transform.Rotate(0, 0, rotationSpeed * direction * Time.deltaTime);
    }
}