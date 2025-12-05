using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    public int spawnID = 0;
    public SplineContainer associatedSpline;
    public GameObject enemyPrefab;
    public int poolSize = 10;
    public float spawnInterval = 2f;

    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private Dictionary<GameObject, Queue<GameObject>> enemyPools = new Dictionary<GameObject, Queue<GameObject>>();
    private float spawnTimer;

    [Header("Gizmos Settings")]
    public Color spawnColor = Color.red;
    public float iconSize = 1f;

    private void Start()
    {
        InitializePool();
    }

    /*private void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnEnemy();
            spawnTimer = 0f;
        }
    }*/

    private void InitializePool()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"SpawnPoint {name} no tiene asignado un prefab de enemigo.");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, transform.position, transform.rotation);

            enemy.SetActive(false);

            enemy.transform.SetParent(transform, worldPositionStays: true);

            enemyPool.Enqueue(enemy);
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || associatedSpline == null)
        {
            Debug.LogWarning($"SpawnPoint {name} no puede generar enemigo: falta prefab o spline.");
            return;
        }

        GameObject enemy = GetEnemyFromPool();

        enemy.transform.SetParent(null, true); // Por si hay offsets
        enemy.transform.position = transform.position;
        enemy.transform.rotation = transform.rotation;

        // Reasignar spline y resetear path
        var pathAgent = enemy.GetComponent<FollowPathAgent>();
        if (pathAgent != null)
        {
            pathAgent.AssignSplineContainer(associatedSpline);
            pathAgent.ResetProgress(keepWorldPosition: true);

            pathAgent.enabled = true;
        }

        enemy.SetActive(true);
       


        enemy.transform.SetParent(transform, true);

        EventManager.Invoke(GlobalEvents.SpawnEnemy, enemy.transform);
    }


    private GameObject GetEnemyFromPool()
    {
        GameObject enemy;
        if (enemyPool.Count > 0)
        {
            enemy = enemyPool.Dequeue();
            if (enemy == null)
            {
                enemy = Instantiate(enemyPrefab);
            }
        }
        else
        {
            enemy = Instantiate(enemyPrefab);
        }

        return enemy;
    }

    // M�todo para retornar el enemigo al pool
    public void ReturnToPool(GameObject enemy)
    {
        // Reset state
        var pathAgent = enemy.GetComponent<FollowPathAgent>();
        if (pathAgent != null)
        {
            pathAgent.enabled = false;
        }

        enemy.SetActive(false);
        enemyPool.Enqueue(enemy);
        // Optionally parent it back to spawnpoint
        enemy.transform.SetParent(transform);
    }

    public Vector3 GetSpawnPosition()
    {
        return transform.position;
    }

    private void InitializePoolForPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError($"SpawnPoint {name} no puede inicializar pool: prefab es null.");
            return;
        }

        Queue<GameObject> pool = new Queue<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(prefab, transform.position, transform.rotation);
            enemy.SetActive(false);
            enemy.transform.SetParent(transform, worldPositionStays: true);
            pool.Enqueue(enemy);
        }

        enemyPools[prefab] = pool;
        Debug.Log($"Pool inicializado para {prefab.name} con {poolSize} instancias");
    }

    private GameObject GetEnemyFromPool(GameObject prefab)
    {
        GameObject enemy;
        Queue<GameObject> pool = enemyPools[prefab];

        if (pool.Count > 0)
        {
            enemy = pool.Dequeue();
            if (enemy == null)
            {
                enemy = Instantiate(prefab);
            }
        }
        else
        {
            // Si el pool está vacío, crear uno nuevo
            enemy = Instantiate(prefab);
            Debug.LogWarning($"Pool de {prefab.name} agotado, creando nueva instancia");
        }

        return enemy;
    }

    // Método llamado por WaveManager para spawear enemigos
    /*public void SpawnEnemyFromPool(GameObject enemyPrefab)
    {
        if (enemyPrefab == null || associatedSpline == null)
        {
            Debug.LogWarning($"SpawnPoint {name} no puede generar enemigo: falta prefab o spline.");
            return;
        }

        // Inicializar pool si no existe
        if (!enemyPools.ContainsKey(enemyPrefab))
        {
            InitializePoolForPrefab(enemyPrefab);
        }

        GameObject enemy = GetEnemyFromPool(enemyPrefab);

        // Posicionar enemigo
        enemy.transform.SetParent(null, true);
        enemy.transform.position = transform.position;
        enemy.transform.rotation = transform.rotation;

        // Configurar pathfinding
        var pathAgent = enemy.GetComponent<FollowPathAgent>();
        if (pathAgent != null)
        {
            pathAgent.AssignSplineContainer(associatedSpline);
            pathAgent.ResetProgress(keepWorldPosition: true);
            pathAgent.enabled = true;
        }

        // Configurar referencia al spawner
        var enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.Initialize(this);
        }

        // Resetear salud
        var health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.ResetHealth();
        }

        enemy.SetActive(true);
    }*/

    public void SpawnEnemyFromPool(GameObject enemyPrefab)
    {
        if (enemyPrefab == null || associatedSpline == null)
        {
            Debug.LogWarning($"SpawnPoint {name} no puede generar enemigo: falta prefab o spline.");
            return;
        }

        // Inicializar pool si no existe
        if (!enemyPools.ContainsKey(enemyPrefab))
        {
            InitializePoolForPrefab(enemyPrefab);
        }

        GameObject enemy = GetEnemyFromPool(enemyPrefab);

        // IMPORTANTE: Desactivar primero para resetear posición
        enemy.SetActive(false);
        
        // Posicionar enemigo SIN parent
        enemy.transform.SetParent(null);
        enemy.transform.position = transform.position;
        enemy.transform.rotation = transform.rotation;

        // Configurar pathfinding ANTES de activar
        var pathAgent = enemy.GetComponent<FollowPathAgent>();
        if (pathAgent != null)
        {
            pathAgent.enabled = false; // Desactivar primero
            pathAgent.AssignSplineContainer(associatedSpline);
            pathAgent.ResetProgress(keepWorldPosition: false); // FALSE es clave
            pathAgent.enabled = true; // Reactivar después
        }

        // Configurar referencia al spawner
        var enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.Initialize(this);
        }

        // Resetear salud
        var health = enemy.GetComponent<Health>();
        if (health != null)
        {
            health.ResetHealth();
        }

        // Activar AL FINAL
        enemy.SetActive(true);
    }

    
}
