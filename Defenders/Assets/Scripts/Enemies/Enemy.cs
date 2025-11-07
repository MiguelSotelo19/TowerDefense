using System;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int damageToCore = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Core"))
        {
            Console.WriteLine("Llego a core");
            ReachCore();
        }
    }

    public void ReachCore()
    {
        // Buscar el núcleo y aplicar daño
        CoreHealth core = FindFirstObjectByType<CoreHealth>();
        if (core != null)
        {
            core.TakeDamage(damageToCore);
        }

        // Eliminar o devolver a la pool
        Destroy(gameObject);
    }
}
