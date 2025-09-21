using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DraggableBed : MonoBehaviour
{
    [Header("Настройки кровати")]
    public Transform sleepPoint;
    public string bedID = "DefaultBed";

    [Header("Настройки перетаскивания")]
    [SerializeField] private float dragThreshold = 10f;

    private Camera mainCamera;
    private Vector3 offset;
    private float zCoordinate;
    private bool isDragging = false;
    private bool isPreparingToDrag = false;
    private Vector3 initialScreenMousePosition;
    private Collider2D bedCollider;
    private bool isMenuOpen = false; 


    void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            UnityEngine.Debug.LogError("Камера не найдена! Убедитесь, что у вас есть камера с тегом 'MainCamera'.");
            enabled = false;
            return;
        }

        bedCollider = GetComponent<Collider2D>();
        LoadPosition();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.collider == bedCollider)
            {
                if (isMenuOpen)
                {
                    if (ContextMenuManager.instance != null) ContextMenuManager.instance.HideAllMenus();
                }
                else
                {
                    if (ContextMenuManager.instance != null) ContextMenuManager.instance.ShowBedMenu(this.transform);
                    isMenuOpen = true;
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {

            }
            else
            {
                RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null && hit.collider == bedCollider)
                {
                    isPreparingToDrag = true;
                    initialScreenMousePosition = Input.mousePosition;
                    zCoordinate = mainCamera.WorldToScreenPoint(transform.position).z;
                    offset = transform.position - GetMouseWorldPosition();
                }
            }
        }

        if (isPreparingToDrag && Input.GetMouseButton(0))
        {
            if (!isDragging && Vector3.Distance(Input.mousePosition, initialScreenMousePosition) > dragThreshold)
            {
                isDragging = true;
            }

            if (isDragging)
            {
                transform.position = ClampToScreen(GetMouseWorldPosition() + offset);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                SavePosition();
            }
            isPreparingToDrag = false;
            isDragging = false;
        }
    }

    public void OnMenuClosed()
    {
        isMenuOpen = false; 
        // Debug.Log($"Меню для кровати {bedID} было закрыто.");
    }

    #region Вспомогательные функции

    private void OnEnable()
    {
        if (CatController.AllBeds != null && !CatController.AllBeds.Contains(this))
        {
            CatController.AllBeds.Add(this);
        }
    }

    private void OnDisable()
    {
        if (CatController.AllBeds != null)
        {
            CatController.AllBeds.Remove(this);
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoordinate;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    private void SavePosition()
    {
        if (string.IsNullOrEmpty(bedID)) return;

        PlayerPrefs.SetFloat(bedID + "_x", transform.position.x);
        PlayerPrefs.SetFloat(bedID + "_y", transform.position.y);
        PlayerPrefs.Save();
    }

    private void LoadPosition()
    {
        if (string.IsNullOrEmpty(bedID)) return;

        if (PlayerPrefs.HasKey(bedID + "_x"))
        {
            float x = PlayerPrefs.GetFloat(bedID + "_x");
            float y = PlayerPrefs.GetFloat(bedID + "_y");
            transform.position = new Vector3(x, y, transform.position.z);
        }
    }

    private Vector3 ClampToScreen(Vector3 pos)
    {
        if (mainCamera.orthographic)
        {
            Vector2 extents = bedCollider.bounds.extents;
            float camHeight = mainCamera.orthographicSize;
            float camWidth = mainCamera.aspect * camHeight;

            float minX = mainCamera.transform.position.x - camWidth + extents.x;
            float maxX = mainCamera.transform.position.x + camWidth - extents.x;
            float minY = mainCamera.transform.position.y - camHeight + extents.y;
            float maxY = mainCamera.transform.position.y + camHeight - extents.y;

            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }
        return pos;
    }

    #endregion
}