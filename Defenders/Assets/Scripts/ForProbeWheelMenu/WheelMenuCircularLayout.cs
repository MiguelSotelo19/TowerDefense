using UnityEngine;

public class WheelMenuCircularLayout : MonoBehaviour
{
    [Header("Layout Settings")]
    public float radius = 120f;
    public float startAngle = 90f; // Comenzar desde arriba
    public bool clockwise = true;
    
    [Header("Visual Settings")]
    public bool rotateIcons = false; // Si quieres que los iconos miren hacia afuera

    void Start()
    {
        Arrange();
    }

    public void Arrange()
    {
        int childCount = transform.childCount;
        if (childCount == 0) return;

        float angleStep = 360f / childCount;
        int direction = clockwise ? 1 : -1;

        for (int i = 0; i < childCount; i++)
        {
            // Calcular ángulo con offset inicial
            float angle = startAngle + (angleStep * i * direction);
            float rad = angle * Mathf.Deg2Rad;

            // Posición circular perfecta
            Vector2 pos = new Vector2(
                Mathf.Cos(rad) * radius,
                Mathf.Sin(rad) * radius
            );

            // Obtener y configurar el hijo
            RectTransform child = transform.GetChild(i).GetComponent<RectTransform>();
            if (child != null)
            {
                child.anchoredPosition = pos;

                // Opcional: rotar los iconos para que miren hacia afuera
                if (rotateIcons)
                {
                    child.localRotation = Quaternion.Euler(0, 0, angle - 90f);
                }
            }
        }
    }

    // Para visualizar en el editor
    private void OnValidate()
    {
        if (Application.isPlaying)
            Arrange();
    }
}