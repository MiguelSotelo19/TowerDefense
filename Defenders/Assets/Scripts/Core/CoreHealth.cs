using UnityEngine;

public class CoreHealth : MonoBehaviour
{

    public int maxHealth = 20;
    public int currentHealth;
    public Color coreColor = Color.blue;
    public float coreSize = 2f;

    void Start()
    {
        currentHealth = maxHealth;
        EventManager.Invoke(GlobalEvents.CoreHealthUpdated, currentHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        EventManager.Invoke(GlobalEvents.CoreHealthUpdated, currentHealth);
        Debug.Log($"Core dañado! Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDefeat();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = coreColor;
        Gizmos.DrawWireSphere(transform.position, coreSize);

        Gizmos.DrawLine(transform.position + Vector3.left * coreSize, transform.position + Vector3.right * coreSize);
        Gizmos.DrawLine(transform.position + Vector3.forward * coreSize, transform.position + Vector3.back * coreSize);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            TakeDamage(enemy.DamageToCore);
            enemy.ReachCore();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Hurtbox"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                TakeDamage(enemy.DamageToCore);
                enemy.ReachCore();
            }
            else
            {
                Debug.LogWarning("No se encontró componente Enemy!");
            }
        }
    }

}
