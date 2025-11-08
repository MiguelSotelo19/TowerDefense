using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float maxRange = 10f;
    public bool canPierce = false;

    public UnityEvent onHit;
    public UnityEvent onRangeEnd;

    public Vector3 direction;
    private Vector3 startPosition;

    private void OnEnable()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.position += direction * (speed * Time.deltaTime);

        if (Vector3.Distance(startPosition, transform.position) >= maxRange)
        {
            onRangeEnd?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hurtbox"))
        {
            onHit?.Invoke();

            if (!canPierce)
            {
                onRangeEnd?.Invoke();
            }
        }
    }
}
