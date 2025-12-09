using UnityEngine;
using UnityEngine.Splines;

// Helper para configurar FollowPathAgent dinámicamente (para el troyano chiquito)
[RequireComponent(typeof(FollowPathAgent))]
public class FollowPathAgentHelper : MonoBehaviour
{
    private FollowPathAgent followPathAgent;

    private void Awake()
    {
        followPathAgent = GetComponent<FollowPathAgent>();
    }
   
    public void SetSplineContainer(SplineContainer container)
    {
        if (followPathAgent == null)
        {
            return;
        }

        //No le sé a la reflexion
        var field = typeof(FollowPathAgent).GetField("splineContainer",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public);

        if (field != null)
        {
            field.SetValue(followPathAgent, container);
        }
        else
        {
            Debug.LogError("No se pudo acceder al campo splineContainer de FollowPathAgent");
        }
    }
}