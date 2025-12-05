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
        Debug.Log($"Core colisionó con: {other.name}, Layer: {LayerMask.LayerToName(other.gameObject.layer)}");
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Hurtbox"))
        {
            Debug.Log("Es layer Hurtbox!");
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log($"Enemigo encontrado: {enemy.name}, llamando TakeDamage y ReachCore");
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
