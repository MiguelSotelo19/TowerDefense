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
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Core dañado! Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        Debug.Log("GAME OVER - El núcleo ha sido destruido");
        // Aquí llamarás al GameManager más adelante
    }

    void OnDrawGizmos()
    {
        Gizmos.color = coreColor;
        Gizmos.DrawWireSphere(transform.position, coreSize);

        Gizmos.DrawLine(transform.position + Vector3.left * coreSize, transform.position + Vector3.right * coreSize);
        Gizmos.DrawLine(transform.position + Vector3.forward * coreSize, transform.position + Vector3.back * coreSize);
    }
}
