using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBarsPool : MonoBehaviour
{
    [Header("Pool Settings")]
    public GameObject healthBarPrefab;
    public int poolSize = 15;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        InitializePool();
    }

    private void OnEnable()
    {
        EventManager.Subscribe<Transform>(GlobalEvents.SpawnEnemy, OnEnemySpawned);
        EventManager.Subscribe<Transform>(GlobalEvents.EnemyDied, OnEnemyDied);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<Transform>(GlobalEvents.SpawnEnemy, OnEnemySpawned);
        EventManager.Unsubscribe<Transform>(GlobalEvents.EnemyDied, OnEnemyDied);
    }

    private void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bar = Instantiate(healthBarPrefab, transform);
            bar.SetActive(false);
            pool.Enqueue(bar);
        }
    }

    private GameObject GetHealthBar()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        // si se acaba el pool, crea uno nuevo
        GameObject bar = Instantiate(healthBarPrefab, transform);
        return bar;
    }

    private void ReturnHealthBar(GameObject bar)
    {
        bar.SetActive(false);
        bar.transform.SetParent(transform);
        pool.Enqueue(bar);
    }

    private void OnEnemySpawned(Transform enemyTransform)
    {
        GameObject bar = GetHealthBar();
        bar.SetActive(true);

        var controller = bar.GetComponent<EnemyHealthBarController>();
        controller.AttachToEnemy(enemyTransform);

        // Asignarlo al enemy para actualizar vida
        enemyTransform.GetComponent<Enemy>().AssignHealthBar(controller);

        Debug.Log("SpawnEvent recibido para: " + enemyTransform.name);
    }


    private void OnEnemyDied(Transform enemyTransform)
    {
        // Busca la barra que le pertenece
        EnemyHealthBarController[] bars = GetComponentsInChildren<EnemyHealthBarController>(true);

        foreach (var bar in bars)
        {
            if (bar.Target == enemyTransform)
            {
                bar.Detach();
                ReturnHealthBar(bar.gameObject);
                break;
            }

        }
        Debug.Log("DeathEvent recibido para: " + enemyTransform.name);

    }
}
