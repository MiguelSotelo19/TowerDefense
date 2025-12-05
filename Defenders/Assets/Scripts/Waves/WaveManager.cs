using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Configuration")]
    [SerializeField] private List<WaveData> waves = new List<WaveData>();
    
    [Header("Spawn Points")]
    [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    [Header("Current State")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private bool isWaveActive = false;
    
    private int enemiesSpawnedThisWave = 0;
    private int enemiesAliveThisWave = 0;
    private bool isSpawning = false;

    public int CurrentWave => currentWaveIndex + 1;
    public int TotalWaves => waves.Count;
    public bool IsWaveActive => isWaveActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        EventManager.Subscribe<Enemy>(GlobalEvents.EnemyDied, OnEnemyDied);
        EventManager.Subscribe<Enemy>(GlobalEvents.EnemyReachedCore, OnEnemyReachedCore);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<Enemy>(GlobalEvents.EnemyDied, OnEnemyDied);
        EventManager.Unsubscribe<Enemy>(GlobalEvents.EnemyReachedCore, OnEnemyReachedCore);
    }

    private void Start()
    {
        // Buscar spawn points si no están asignados
        if (spawnPoints.Count == 0)
        {
            spawnPoints.AddRange(FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None));
        }

        // Deshabilitar spawn automático
        foreach (var sp in spawnPoints)
        {
            sp.enabled = false;
        }

        // Iniciar primera oleada después de un delay
        StartCoroutine(StartFirstWaveDelayed(3f));
    }

    private IEnumerator StartFirstWaveDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (currentWaveIndex >= waves.Count)
        {
            Debug.Log("¡Todas las oleadas completadas!");
            EventManager.Invoke(GlobalEvents.AllWavesCompleted);
            return;
        }

        WaveData currentWave = waves[currentWaveIndex];
        
        isWaveActive = true;
        enemiesSpawnedThisWave = 0;
        enemiesAliveThisWave = 0;

        Debug.Log($"Iniciando Oleada {CurrentWave}/{TotalWaves}");
        EventManager.Invoke<int>(GlobalEvents.WaveStarted, CurrentWave);

        StartCoroutine(SpawnWave(currentWave));
    }

    private IEnumerator SpawnWave(WaveData wave)
    {
        // Esperar antes de iniciar la oleada
        yield return new WaitForSeconds(wave.delayBeforeWave);

        isSpawning = true;

        // Spawear enemigos
        for (int i = 0; i < wave.enemyCount; i++)
        {
            SpawnEnemy(wave.enemyPrefab);
            enemiesSpawnedThisWave++;
            enemiesAliveThisWave++;

            // Esperar antes del siguiente spawn
            if (i < wave.enemyCount - 1)
            {
                yield return new WaitForSeconds(wave.spawnInterval);
            }
        }

        isSpawning = false;
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No hay spawn points configurados!");
            return;
        }

        // Elegir spawn point aleatorio
        SpawnPoint selectedSpawn = spawnPoints[Random.Range(0, spawnPoints.Count)];
        
        Debug.Log($"Spawneando en: {selectedSpawn.transform.position}");

        // Hacer spawn usando el pool del spawn point
        selectedSpawn.SpawnEnemyFromPool(enemyPrefab);
    }

    private void OnEnemyDied(Enemy enemy)
    {
        enemiesAliveThisWave--;
        CheckWaveCompletion();
    }

    private void OnEnemyReachedCore(Enemy enemy)
    {
        enemiesAliveThisWave--;
        CheckWaveCompletion();
    }

    private void CheckWaveCompletion()
    {
        Debug.Log($"Check: isSpawning={isSpawning}, enemiesAlive={enemiesAliveThisWave}, isActive={isWaveActive}");
        
        if (!isSpawning && enemiesAliveThisWave <= 0 && isWaveActive)
        {
            CompleteWave();
        }
    }

    private void CompleteWave()
    {
        isWaveActive = false;
        
        Debug.Log($"¡Oleada {CurrentWave} completada!");
        EventManager.Invoke<int>(GlobalEvents.WaveCompleted, CurrentWave);

        currentWaveIndex++;

        // Iniciar siguiente oleada
        if (currentWaveIndex < waves.Count)
        {
            StartCoroutine(StartNextWaveDelayed());
        }
        else
        {
            Debug.Log("¡Juego completado! Todas las oleadas superadas.");
            EventManager.Invoke(GlobalEvents.AllWavesCompleted);
        }
    }

    private IEnumerator StartNextWaveDelayed()
    {
        // Pequeño delay entre oleadas
        yield return new WaitForSeconds(2f);
        StartNextWave();
    }

    // Método público para forzar inicio de oleada (útil para botones)
    public void ForceStartNextWave()
    {
        if (!isWaveActive && currentWaveIndex < waves.Count)
        {
            StartNextWave();
        }
    }
}