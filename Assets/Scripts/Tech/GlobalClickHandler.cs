using UnityEngine;

public class GlobalClickHandler : MonoBehaviour
{
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider == null) return;

            if (hit.collider.GetComponent<CatController>() != null)
            {
                if (ContextMenuManager.instance.catContextMenu.activeSelf)
                {
                    ContextMenuManager.instance.HideAllMenus();
                }
                else
                {
                    ContextMenuManager.instance.ShowCatMenu(hit.transform);
                }
            }
            else if (hit.collider.GetComponent<DraggableBed>() != null)
            {
                if (ContextMenuManager.instance.bedContextMenu.activeSelf)
                {
                    ContextMenuManager.instance.HideAllMenus();
                }
                else
                {
                    ContextMenuManager.instance.ShowBedMenu(hit.transform);
                }
            }
        }
    }
}