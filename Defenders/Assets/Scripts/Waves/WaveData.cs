using System;
using UnityEngine;

[Serializable]
public class WaveData
{
    [Header("Wave Configuration")]
    public int waveNumber = 1;
    
    [Tooltip("Cantidad de enemigos a generar en esta oleada")]
    public int enemyCount = 5;
    
    [Tooltip("Tiempo entre cada enemigo (segundos)")]
    public float spawnInterval = 2f;
    
    [Tooltip("Tiempo de espera antes de iniciar esta oleada (segundos)")]
    public float delayBeforeWave = 5f;
    
    [Header("Enemy Settings")]
    [Tooltip("Prefab del enemigo para esta oleada")]
    public GameObject enemyPrefab;
}