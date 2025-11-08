using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int damageToCore = 1;
    [HideInInspector] public SpawnPoint ownerSpawner;
    public int bytes = 0;

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
        EconomyManager.Instance.AddBytes(bytes);
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
