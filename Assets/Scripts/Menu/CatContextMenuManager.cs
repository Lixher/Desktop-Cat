using UnityEngine;
using Debug = UnityEngine.Debug;

public class CatContextMenuManager : MonoBehaviour
{
    public static CatContextMenuManager instance;

    [Header("Панели меню КОТА")]
    public GameObject catContextMenuObject;
    public GameObject catSettingsMenuPanel;

    private Transform targetToFollow;

    void Awake()
    {
        instance = this;
        if (catContextMenuObject != null) catContextMenuObject.SetActive(false);
        if (catSettingsMenuPanel != null) catSettingsMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (targetToFollow != null && catContextMenuObject.activeSelf)
        {
            catContextMenuObject.transform.position = targetToFollow.position;
        }

        if (catContextMenuObject.activeSelf && Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider == null || hit.transform != targetToFollow)
            {
                HideCatMenu();
            }
        }
    }

    public void ShowCatMenu(Transform target)
    {
        if (catContextMenuObject == null) return;

        targetToFollow = target;
        catContextMenuObject.transform.position = target.position;
        catContextMenuObject.SetActive(true);
    }

    public void HideCatMenu()
    {
        if (targetToFollow != null)
        {
            CatController cat = targetToFollow.GetComponent<CatController>();
            if (cat != null)
            {
                cat.ResumeFromMenu();
            }
        }

        if (catContextMenuObject != null) catContextMenuObject.SetActive(false);
        targetToFollow = null;
    }

    public void OpenCatSettingsMenu()
    {
        HideCatMenu();
        if (catSettingsMenuPanel != null)
        {
            catSettingsMenuPanel.SetActive(true);
        }
    }

    public void HideCatSettingsMenu()
    {
        if (catSettingsMenuPanel != null)
        {
            catSettingsMenuPanel.SetActive(false);
        }
    }
}