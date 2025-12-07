using UnityEngine;
using System.Collections;

/// <summary>
/// Fragmento del Troyano. Se queda quieto girando.
/// Si no es destruido en 5 segundos, se convierte en un Troyano completo.
/// Hereda de Enemy para que las torres puedan atacarlo.
/// </summary>
public class TrojanFragment : Enemy // ← Cambiado a heredar de Enemy
{
    [Header("Fragment Settings")]
    [SerializeField] private float evolutionTime = 5f; // Tiempo para evolucionar
    [SerializeField] private float rotationSpeed = 180f; // Velocidad de giro

    [Header("Evolution")]
    [SerializeField] private GameObject trojanPrefab; // Prefab del Troyano completo

    [Header("Visual Feedback")]
    [SerializeField] private Renderer fragmentRenderer; // Para cambiar color

    private float remainingTime;
    private SpawnPoint parentSpawner;
    private Vector3 spawnPosition;
    private bool hasEvolved = false;
    private float savedSplineProgress = 0f; // ← NUEVO: Progreso heredado del Troyano padre

    private void Awake()
    {
        // No necesitamos gestionar Health manualmente, Enemy ya lo hace

        if (fragmentRenderer == null)
        {
            fragmentRenderer = GetComponentInChildren<Renderer>();
        }

        // NO intentar modificar materiales en Awake, esperar a Start o cuando se active

        // Desactivar el FollowPathAgent si existe (los fragmentos no se mueven por spline)
        var followPath = GetComponent<FollowPathAgent>();
        if (followPath != null)
        {
            followPath.enabled = false;
        }
    }

    private void Start()
    {
        // No necesitamos clonar materiales.
        // MaterialPropertyBlock evita modificar materiales del prefab.
        if (fragmentRenderer == null)
        {
            Debug.LogWarning("⚠ Renderer del fragmento es nulo en Start()");
        }
    }



    public void Initialize(SpawnPoint spawner, Vector3 position, float splineProgress)
    {
        parentSpawner = spawner;
        ownerSpawner = spawner; // Para Enemy
        spawnPosition = position;
        savedSplineProgress = splineProgress; // ← Guardar progreso
        remainingTime = evolutionTime;
        hasEvolved = false;

        Debug.Log($"🔺 Fragmento inicializado. Evolucionará en {evolutionTime}s (progreso: {splineProgress:F2})");

        StartCoroutine(CountdownToEvolution());
    }

    private void Update()
    {
        if (hasEvolved) return;

        // Girar sobre su propio eje
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    private IEnumerator CountdownToEvolution()
    {
        while (remainingTime > 0 && gameObject.activeInHierarchy)
        {
            remainingTime -= Time.deltaTime;

            // Actualizar indicador visual
            UpdateVisualFeedback();

            yield return null;
        }

        // Si llegó a 0 y sigue activo, evolucionar
        if (gameObject.activeInHierarchy && !hasEvolved)
        {
            EvolveToTrojan();
        }
    }

    private void UpdateVisualFeedback()
    {
        if (fragmentRenderer == null)
        {
            Debug.LogWarning("⚠️ No hay Renderer asignado en el fragmento");
            return;
        }

        try
        {
            // Cambiar color: de rojo (peligro) a amarillo (seguro)
            float t = remainingTime / evolutionTime;
            Color color = Color.Lerp(Color.yellow, Color.red, t);

            // Usar un PropertyBlock para cambiar el color sin crear nuevos materiales
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            fragmentRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", color);
            fragmentRenderer.SetPropertyBlock(propBlock);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Error al actualizar visual del fragmento: {e.Message}");
        }
    }

    // Enemy llama a Die() cuando la vida llega a 0
    // Sobrescribimos para evitar la evolución si muere
    public new void Die(bool giveReward = true)
    {
        hasEvolved = true; // Marcar como evolucionado para detener la coroutine
        Debug.Log($"💀 Fragmento destruido");

        // Llamar al Die del Enemy base (da recompensa y maneja el pool)
        base.Die(giveReward);
    }

    private void EvolveToTrojan()
    {
        hasEvolved = true;
        Debug.Log("🔄 Fragmento evolucionando a Troyano completo!");

        if (trojanPrefab == null)
        {
            Debug.LogError("❌ No hay trojanPrefab asignado!");
            base.Die(giveReward: false);
            return;
        }

        // Crear nuevo Troyano en la posición del fragmento
        GameObject newTrojanObj = Instantiate(trojanPrefab, transform.position, Quaternion.identity);

        // Configurar el nuevo Troyano
        TrojanEnemy newTrojan = newTrojanObj.GetComponent<TrojanEnemy>();
        if (newTrojan != null && parentSpawner != null)
        {
            newTrojan.Initialize(parentSpawner);
            Debug.Log("✅ Nuevo Troyano creado desde fragmento");
        }

        // Asignar spline (helper y FollowPathAgent)
        var followPathHelper = newTrojanObj.GetComponent<FollowPathAgentHelper>();
        if (followPathHelper != null && parentSpawner != null)
        {
            var splineContainer = parentSpawner.associatedSpline;
            if (splineContainer != null)
            {
                followPathHelper.SetSplineContainer(splineContainer);
                Debug.Log("✅ SplineContainer asignado al nuevo Troyano");
            }
            else
            {
                Debug.LogError("❌ El SpawnPoint no tiene associatedSpline configurado!");
            }
        }
        else if (followPathHelper == null)
        {
            Debug.LogError("❌ El prefab del Troyano no tiene FollowPathAgentHelper!");
        }

        var followPath = newTrojanObj.GetComponent<FollowPathAgent>();
        if (followPath != null)
        {
            if (parentSpawner != null && parentSpawner.associatedSpline != null)
            {
                // Asigna el SplineContainer (esto normalmente reinicia el progreso)
                followPath.AssignSplineContainer(parentSpawner.associatedSpline);

                // Restaurar progreso y "snap" a la posición/rotación correcta del spline
                followPath.RestoreProgressAndSnap(savedSplineProgress);

                Debug.Log($"✅ Troyano continúa desde progreso {savedSplineProgress:F2}");
            }
            followPath.enabled = true;
        }

        // Destruir este fragmento (sin dar recompensa)
        Destroy(gameObject);
    }


    /// <summary>
    /// Establece el progreso del spline en el FollowPathAgent
    /// </summary>
    private void SetSplineProgress(FollowPathAgent agent, float progress)
    {
        if (agent == null) return;

        try
        {
            // Intentar establecer el progreso mediante reflection
            var progressField = typeof(FollowPathAgent).GetField("_t",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);

            if (progressField != null)
            {
                progressField.SetValue(agent, progress);
                Debug.Log($"✅ Progreso establecido a {progress:F2}");
            }
            else
            {
                Debug.LogWarning("⚠️ No se pudo establecer el progreso. El Troyano empezará desde el inicio.");
                Debug.LogWarning("⚠️ No se pudo establecer el progreso. El Troyano empezará desde el inicio.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"⚠️ Error al establecer progreso: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        // Enemy ya maneja la limpieza de eventos
    }
}