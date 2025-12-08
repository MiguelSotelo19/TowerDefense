using UnityEngine;

public class WheelMenuCircularLayout : MonoBehaviour
{
    [Header("Layout Settings")]
    public float radius = 120f;
    public float startAngle = 90f;
    public bool clockwise = true;
    
    [Header("Visual Settings")]
    public bool rotateIcons = false;

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
            float angle = startAngle + (angleStep * i * direction);
            float rad = angle * Mathf.Deg2Rad;

            Vector2 pos = new Vector2(
                Mathf.Cos(rad) * radius,
                Mathf.Sin(rad) * radius
            );

            RectTransform child = transform.GetChild(i).GetComponent<RectTransform>();
            if (child != null)
            {
                child.anchoredPosition = pos;

                if (rotateIcons)
                {
                    child.localRotation = Quaternion.Euler(0, 0, angle - 90f);
                }
            }
        }
    }
    private void OnValidate()
    {
        if (Application.isPlaying)
            Arrange();
    }
}