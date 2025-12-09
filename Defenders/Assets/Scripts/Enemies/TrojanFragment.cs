using UnityEngine;
using System.Collections;

public class TrojanFragment : Enemy
{
    [Header("Fragment Settings")]
    [SerializeField] private float evolutionTime = 5f; 
    [SerializeField] private float rotationSpeed = 180f; 

    [Header("Evolution")]
    [SerializeField] private GameObject trojanPrefab; 

    [Header("Visual Feedback")]
    [SerializeField] private Renderer fragmentRenderer;

    private float remainingTime;
    private SpawnPoint parentSpawner;
    private Vector3 spawnPosition;
    private bool hasEvolved = false;
    private float savedSplineProgress = 0f;

    private void Awake()
    {
        if (fragmentRenderer == null)
        {
            fragmentRenderer = GetComponentInChildren<Renderer>();
        }

        var followPath = GetComponent<FollowPathAgent>();
        if (followPath != null)
        {
            followPath.enabled = false;
        }
    }

    private void Start()
    {
        if (fragmentRenderer == null)
        {
            Debug.LogWarning("Renderer del fragmento es nulo en Start()");
        }
    }



    public void Initialize(SpawnPoint spawner, Vector3 position, float splineProgress)
    {
        parentSpawner = spawner;
        ownerSpawner = spawner; // Para Enemy
        spawnPosition = position;
        savedSplineProgress = splineProgress; // Guardar progreso del spline
        remainingTime = evolutionTime;
        hasEvolved = false;

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

            UpdateVisualFeedback();

            yield return null;
        }

        // Si remainingTime llegó a 0 evoluciona
        if (gameObject.activeInHierarchy && !hasEvolved)
        {
            EvolveToTrojan();
        }
    }

    private void UpdateVisualFeedback()
    {
        if (fragmentRenderer == null)
        {
            Debug.LogWarning("No hay Renderer asignado");
            return;
        }

        try
        {
            float t = remainingTime / evolutionTime;
            Color color = Color.Lerp(Color.yellow, Color.red, t);

            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            fragmentRenderer.GetPropertyBlock(propBlock);
            propBlock.SetColor("_Color", color);
            fragmentRenderer.SetPropertyBlock(propBlock);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error al actualizar visual"+e);
        }
    }

    public new void Die(bool giveReward = true)
    {
        hasEvolved = true; 
        base.Die(giveReward);
    }

    private void EvolveToTrojan()
    {
        hasEvolved = true;

        if (trojanPrefab == null)
        {
            Debug.LogError("No hay prefab asignado");
            base.Die(giveReward: false);
            return;
        }

        GameObject newTrojanObj = Instantiate(trojanPrefab, transform.position, Quaternion.identity);

        // Configurar nuevo
        TrojanEnemy newTrojan = newTrojanObj.GetComponent<TrojanEnemy>();
        if (newTrojan != null && parentSpawner != null)
        {
            newTrojan.Initialize(parentSpawner);
        }
        //Configurar ruta
        var followPathHelper = newTrojanObj.GetComponent<FollowPathAgentHelper>();
        if (followPathHelper != null && parentSpawner != null)
        {
            var splineContainer = parentSpawner.associatedSpline;
            if (splineContainer != null)
            {
                followPathHelper.SetSplineContainer(splineContainer);
            }
            else
            {
                Debug.LogError("El SpawnPoint no tiene Spline");
            }
        }
        else if (followPathHelper == null)
        {
            Debug.LogError("Troyano no tiene FollowPathAgentHelper");
        }

        var followPath = newTrojanObj.GetComponent<FollowPathAgent>();
        if (followPath != null)
        {
            if (parentSpawner != null && parentSpawner.associatedSpline != null)
            {
                followPath.AssignSplineContainer(parentSpawner.associatedSpline);
                //Restaura progreso del spline para que no inicie en 0 (el de arriba lo reinicia xd)
                followPath.RestoreProgressAndSnap(savedSplineProgress);
            }
            followPath.enabled = true;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        
    }
}