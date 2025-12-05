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
        EventManager.Invoke(GlobalEvents.CoreHealthUpdated,currentHealth);  
        Debug.Log($"Core da�ado! Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("GAME OVER - El n�cleo ha sido destruido");
        // Aqu� llamar�s al GameManager m�s adelante
    }

    void OnDrawGizmos()
    {
        Gizmos.color = coreColor;
        Gizmos.DrawWireSphere(transform.position, coreSize);

        Gizmos.DrawLine(transform.position + Vector3.left * coreSize, transform.position + Vector3.right * coreSize);
        Gizmos.DrawLine(transform.position + Vector3.forward * coreSize, transform.position + Vector3.back * coreSize);
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
        }
    }

}
