using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarController : MonoBehaviour
{
    public Transform Target;
    public Image fillImage;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    public void AttachToEnemy(Transform enemy)
    {
        Target = enemy;
    }

    public void Detach()
    {
        Target = null;
        gameObject.SetActive(false);
    }

    public void SetHealth(float current, float max)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = current / max;
        }
    }

    private void LateUpdate()
    {
        if (Target == null) return;

        Vector3 worldPos = Target.position + Vector3.up * 2f;
        transform.position = cam.WorldToScreenPoint(worldPos);
    }
}
