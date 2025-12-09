using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

/// <summary>
/// Genera un mesh plano (como una carretera) a lo largo de un Spline.
/// Soluciona el problema de las curvaturas verticales.
/// Este si no le se, lo hizo chat 100%
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FlatSplinePathMesh : MonoBehaviour
{
    [Header("Spline Reference")]
    [SerializeField] private SplineContainer splineContainer;

    [Header("Path Settings")]
    [SerializeField] private float pathWidth = 2f;
    [SerializeField] private int segmentsPerUnit = 10;
    [SerializeField] private float heightOffset = 0.1f; // Altura sobre el suelo

    [Header("UV Settings")]
    [SerializeField] private float uvTiling = 1f; // Repetición de textura

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        GenerateFlatPath();
    }

    public void GenerateFlatPath()
    {
        if (splineContainer == null)
        {
            Debug.LogError("No hay SplineContainer asignado!");
            return;
        }

        Spline spline = splineContainer.Spline;
        float splineLength = spline.GetLength();
        int totalSegments = Mathf.Max(10, Mathf.CeilToInt(splineLength * segmentsPerUnit));

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        float halfWidth = pathWidth * 0.5f;

        // Generar vértices a lo largo del spline
        for (int i = 0; i <= totalSegments; i++)
        {
            float t = i / (float)totalSegments;

            // Posición en el spline
            float3 splinePos = spline.EvaluatePosition(t);
            Vector3 worldPos = splineContainer.transform.TransformPoint(splinePos);

            // Tangente (dirección del spline)
            float3 splineTangent = spline.EvaluateTangent(t);
            Vector3 forward = splineContainer.transform.TransformDirection(splineTangent).normalized;

            // ✅ CLAVE: Usar Vector3.up como referencia fija para que sea plano
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            // Forzar altura fija
            worldPos.y = heightOffset;

            // Crear dos vértices (izquierda y derecha)
            Vector3 leftVertex = worldPos - right * halfWidth;
            Vector3 rightVertex = worldPos + right * halfWidth;

            // Forzar Y en ambos vértices
            leftVertex.y = heightOffset;
            rightVertex.y = heightOffset;

            vertices.Add(leftVertex);
            vertices.Add(rightVertex);

            // UVs
            float uvY = t * splineLength * uvTiling;
            uvs.Add(new Vector2(0, uvY));
            uvs.Add(new Vector2(1, uvY));

            // Triángulos (conectar con el segmento anterior)
            if (i > 0)
            {
                int baseIndex = (i - 1) * 2;

                // Primer triángulo
                triangles.Add(baseIndex);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 1);

                // Segundo triángulo
                triangles.Add(baseIndex + 1);
                triangles.Add(baseIndex + 2);
                triangles.Add(baseIndex + 3);
            }
        }

        // Crear el mesh
        Mesh mesh = new Mesh();
        mesh.name = "FlatPathMesh";
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;

        Debug.Log($"Camino plano generado: {vertices.Count} vértices, {triangles.Count / 3} triángulos");
    }

    // Regenerar cuando cambien valores en el editor
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            GenerateFlatPath();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (splineContainer == null) return;

        Gizmos.color = Color.cyan;
        Spline spline = splineContainer.Spline;

        // Dibujar preview del ancho
        int previewSegments = 20;
        float halfWidth = pathWidth * 0.5f;

        for (int i = 0; i <= previewSegments; i++)
        {
            float t = i / (float)previewSegments;
            float3 splinePos = splineContainer.EvaluatePosition(spline, t);
            Vector3 worldPos = splineContainer.transform.TransformPoint(splinePos);
            worldPos.y = heightOffset;

            float3 splineTangent = splineContainer.EvaluateTangent(spline, t);
            Vector3 forward = splineContainer.transform.TransformDirection(splineTangent).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

            Vector3 left = worldPos - right * halfWidth;
            Vector3 right2 = worldPos + right * halfWidth;

            left.y = heightOffset;
            right2.y = heightOffset;

            Gizmos.DrawLine(left, right2);
        }
    }
#endif
}