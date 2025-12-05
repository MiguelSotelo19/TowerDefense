using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 20f;
    public float maxRange = 10f;
    public Vector3 direction;
    public bool followTarget = false;
    public Transform target;

    [Header("Daño")]
    public int damage = 1;
    public float explosionRadius = 0f;
    public bool canPierce = false;

    [Header("Eventos")]
    public UnityEvent onHit;
    public UnityEvent onRangeEnd;

    private Vector3 startPosition;
    private bool isDespawning;

    private void OnEnable()
    {
        // Reiniciar estado al activarse (pooling)
        startPosition = transform.position;
        isDespawning = false;
    }

    private void Update()
    {
        if (followTarget && target != null)
        {
            direction = (target.position - transform.position).normalized;
        }

        transform.position += direction * (speed * Time.deltaTime);

        if (!isDespawning && Vector3.Distance(startPosition, transform.position) >= maxRange)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Solo golpea hurtboxes
        if (other.gameObject.layer != LayerMask.NameToLayer("Hurtbox"))
            return;

        ApplyDamage(other);

        onHit?.Invoke();

        if (!canPierce)
            Despawn();
    }

    // ---------------------------
    // APLICACIÓN DEL DAÑO
    // ---------------------------
    private void ApplyDamage(Collider col)
    {
        if (explosionRadius <= 0f)
        {
            DealToEnemy(col);
            return;
        }

        // Explosión: dañar todos los Hurtbox dentro del radio
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (Collider c in hits)
        {
            if (c.gameObject.layer == LayerMask.NameToLayer("Hurtbox"))
                DealToEnemy(c);
        }
    }

    private void DealToEnemy(Collider col)
    {
        // Usamos GetComponentInParent por si el Hurtbox está en un child
        Health health = col.GetComponentInParent<Health>();
        if (health != null)
            health.TakeDamage(damage);
    }

    // ---------------------------
    // DESPAWN SEGURO
    // ---------------------------
    private void Despawn()
    {
        if (isDespawning)
            return;

        isDespawning = true;
        onRangeEnd?.Invoke();

        // Desactivar al final del frame sin coroutine
        followTarget = false;
        target = null;

        gameObject.SetActive(false);
    }


    private System.Collections.IEnumerator DelayedDisable()
    {
        // Espera 0 frames para permitir ejecuciones posteriores si hay listeners.
        yield return null;
        // Reset opcional: limpiar target/followTarget para evitar comportamientos residuales
        followTarget = false;
        target = null;
        isDespawning = false; // permitir reuso correcto en el pool (se volverá a setear en OnEnable)
        gameObject.SetActive(false);
    }

    public void SetDamage(int dmg) => damage = dmg;
    public void SetSpeed(float s) => speed = s;
    public void SetMaxRange(float r) => maxRange = r;
    public void SetPierce(bool pierce) => canPierce = pierce;
    public void SetExplosionRadius(float r) => explosionRadius = r;
    public void SetFollowTarget(bool follow, Transform t = null)
    {
        followTarget = follow;
        target = t;
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (explosionRadius > 0f)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f); // Naranja translúcido
            Gizmos.DrawSphere(transform.position, explosionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
#endif

}
