using UnityEngine;
using UnityEngine.Splines;

/// <summary>
/// Helper para configurar FollowPathAgent dinámicamente
/// Agrega este script al prefab del Troyano grande junto a FollowPathAgent
/// </summary>
[RequireComponent(typeof(FollowPathAgent))]
public class FollowPathAgentHelper : MonoBehaviour
{
    private FollowPathAgent followPathAgent;

    private void Awake()
    {
        followPathAgent = GetComponent<FollowPathAgent>();
    }

    /// <summary>
    /// Asigna el SplineContainer al FollowPathAgent
    /// Llamado desde TrojanFragment al evolucionar
    /// </summary>
    public void SetSplineContainer(SplineContainer container)
    {
        if (followPathAgent == null)
        {
            Debug.LogError("❌ No se encontró FollowPathAgent!");
            return;
        }

        // Acceder al campo mediante reflection si es privado
        var field = typeof(FollowPathAgent).GetField("splineContainer",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);

        if (field != null)
        {
            field.SetValue(followPathAgent, container);
            Debug.Log("✅ SplineContainer asignado correctamente");
        }
        else
        {
            Debug.LogError("❌ No se pudo acceder al campo splineContainer de FollowPathAgent");
        }
    }
}