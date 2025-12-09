using UnityEngine;
using UnityEngine.EventSystems;

public class HideWheelMenuOnClick : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;

            if (WheelMenuController.Instance != null && 
                WheelMenuController.Instance.gameObject.activeInHierarchy)
            {
                WheelMenuController.Instance.HideMenu();
            }
        }
    }
}