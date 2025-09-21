using UnityEngine;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(Bowl))]
public class BowlDragHandler : MonoBehaviour
{
    private Camera mainCamera;
    private Vector3 offset;
    private bool isDragging;

    private Bowl bowl;

    void Awake()
    {
        mainCamera = Camera.main;
        bowl = GetComponent<Bowl>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.transform == this.transform)
            {
                if (bowl != null && bowl.IsInUse)
                {
                    Debug.Log("Нельзя двигать миску из нее ест котик");
                    return; 
                }

                isDragging = true;
                offset = transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, transform.position.z);
        }

        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
}