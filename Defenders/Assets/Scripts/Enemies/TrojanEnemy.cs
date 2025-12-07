using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Troyano: Enemigo que se divide en 3 fragmentos al morir.
/// Cada fragmento puede convertirse en un Troyano completo si no es destruido.
/// </summary>
public class TrojanEnemy : Enemy
{
    [Header("Trojan Specific Settings")]
    [SerializeField] private GameObject fragmentPrefab; // Prefab del fragmento
    [SerializeField] private int fragmentCount = 3;
    [SerializeField] private float spawnRadius = 2f; // Radio de dispersión

    [Header("Visual Settings")]
    [SerializeField] private GameObject normalModel; // Modelo del Troyano grande

    private bool hasSplit = false;
    private float splineProgressAtDeath = 0f; // ← NUEVO: Guardar progreso al morir

    private void Awake()
    {
        // Limpiar listeners del Inspector y configurar el nuestro
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.onDeath.RemoveAllListeners();
            health.onDeath.AddListener(OnTrojanDeath);
            Debug.Log("✅ TrojanEnemy: Listeners configurados");
        }
    }

    private void Start()
    {
        // Re-verificar listeners
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.onDeath.RemoveAllListeners();
            health.onDeath.AddListener(OnTrojanDeath);
        }
    }

    private void OnTrojanDeath()
    {
        Debug.Log("💀 Troyano murió - dividiéndose");

        if (hasSplit)
        {
            Debug.Log("⚠️ Ya se había dividido antes, morir definitivamente");
            ReturnToPoolOrDisable();
            return;
        }

        // Primera muerte: dividirse en fragmentos
        SplitIntoFragments();
    }

    private void SplitIntoFragments()
    {
        hasSplit = true;
        Vector3 splitPosition = transform.position;

        // ✅ GUARDAR el progreso actual del spline
        var followPath = GetComponent<FollowPathAgent>();
        if (followPath != null)
        {
            // Asumiendo que FollowPathAgent tiene un campo progress o similar
            // Si no, podrías calcularlo basándote en la distancia recorrida
            splineProgressAtDeath = GetCurrentSplineProgress();
            Debug.Log($"📍 Progreso guardado: {splineProgressAtDeath:F2}");
        }

        Debug.Log($"🔺 Troyano dividiéndose en {fragmentCount} fragmentos");

        if (fragmentPrefab == null)
        {
            Debug.LogError("❌ No hay fragmentPrefab asignado!");
            ReturnToPoolOrDisable();
            return;
        }

        // Crear fragmentos
        for (int i = 0; i < fragmentCount; i++)
        {
            SpawnFragment(i, splitPosition);
        }

        Debug.Log($"✅ {fragmentCount} fragmentos creados");

        // El Troyano original muere inmediatamente (da recompensa)
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.AddBytes(bytes);

        // Desactivar y devolver al pool
        ReturnToPoolOrDisable();
    }

    /// <summary>
    /// Obtiene el progreso actual en el spline (0 = inicio, 1 = final)
    /// </summary>
    private float GetCurrentSplineProgress()
    {
        var followPath = GetComponent<FollowPathAgent>();
        if (followPath == null) return 0f;

        // Intentar acceder al progreso mediante reflection
        var progressField = typeof(FollowPathAgent).GetField("_t",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);

        if (progressField != null)
        {
            object value = progressField.GetValue(followPath);
            if (value is float progress)
            {
                return progress;
            }
        }

        Debug.LogWarning("⚠️ No se pudo leer el progreso del spline");
        return 0f;
    }

    /// <summary>
    /// Obtiene el progreso guardado del Troyano original
    /// </summary>
    public float GetSavedProgress()
    {
        return splineProgressAtDeath;
    }

    private void SpawnFragment(int index, Vector3 centerPosition)
    {
        // Calcular posición en círculo
        float angle = (360f / fragmentCount) * index;
        Vector3 offset = Quaternion.Euler(0, angle, 0) * Vector3.forward * spawnRadius;
        Vector3 spawnPos = centerPosition + offset;
        spawnPos.y = centerPosition.y;

        Debug.Log($"🔹 Creando fragmento {index + 1} en posición {spawnPos}");

        // Instanciar fragmento
        GameObject fragmentObj = Instantiate(fragmentPrefab, spawnPos, Quaternion.identity);
        TrojanFragment fragment = fragmentObj.GetComponent<TrojanFragment>();

        if (fragment != null)
        {
            // ✅ Pasar el progreso del spline al fragmento
            fragment.Initialize(ownerSpawner, spawnPos, splineProgressAtDeath);
            Debug.Log($"✅ Fragmento {index + 1} inicializado con progreso {splineProgressAtDeath:F2}");
        }
        else
        {
            Debug.LogError($"❌ El prefab '{fragmentPrefab.name}' no tiene TrojanFragment!");
            Destroy(fragmentObj);
        }
    }

    private void ReturnToPoolOrDisable()
    {
        Debug.Log("🔙 Troyano retornando al pool");

        // Restaurar estado
        hasSplit = false;

        if (normalModel != null)
            normalModel.SetActive(true);

        var collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;

        var followPath = GetComponent<FollowPathAgent>();
        if (followPath != null)
        {
            followPath.enabled = true;
            followPath.ResetProgress(false);
        }

        if (ownerSpawner != null)
            ownerSpawner.ReturnToPool(gameObject);
        else
            gameObject.SetActive(false);
    }

    private new void OnDisable()
    {
        hasSplit = false;

        if (normalModel != null)
            normalModel.SetActive(true);
    }
}