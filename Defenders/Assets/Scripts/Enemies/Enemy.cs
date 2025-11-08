using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int damageToCore = 1;

    // El SpawnPoint que creó/gestiona este enemigo
    [HideInInspector] public SpawnPoint ownerSpawner;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Core"))
        {
            Debug.Log("Llego a core");
            ReachCore();
        }
    }

    public void ReachCore()
    {
        CoreHealth core = FindFirstObjectByType<CoreHealth>();
        if (core != null)
        {
            core.TakeDamage(damageToCore);
        }

        ReturnToPoolOrDisable();
    }

    public void Die()
    {
        //Aqui vas a hacer lo de animaciones por si mueren o alguna locochoneria asi
        ReturnToPoolOrDisable();
    }

    private void ReturnToPoolOrDisable()
    {
        if (ownerSpawner != null)
        {
            ownerSpawner.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void Initialize(SpawnPoint spawner)
    {
        ownerSpawner = spawner;
    }
}
