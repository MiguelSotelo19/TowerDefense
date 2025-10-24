using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform destination;
    public int health = 100;
    public int bytes = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = destination.position;
    }

    // Update is called once per frame
    void Update()
    {
        //para probar una muerte de enemigo xd
        if (Input.GetKeyDown(KeyCode.F))
        {
            Die();
        }
    }

    //nomas de ejemplo
    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        EconomyManager.Instance.AddBytes(bytes);
        Destroy(gameObject);
    }
}
