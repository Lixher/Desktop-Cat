using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ContextMenuManager : MonoBehaviour
{
    public static ContextMenuManager instance;

    [Header("Ссылки на Меню")]
    [Tooltip("Панель с контекстом для кровати")]
    public GameObject bedContextMenu;
    [Tooltip("Панель с контекстом для кота")]
    public GameObject catContextMenu;
    [Tooltip("Главная панель настроек")]
    public GameObject settingsMenuPanel;

    [Tooltip("Панель настроек КОТА (с RGB)")]
    public GameObject catSettingsPanel;


    [Header("Ссылки на UI Элементы")]
    [Tooltip("Ссылка на полоску голода в меню кота")]
    public HungerBar catHungerBarInMenu;
    public Button feedButton;

    private Transform activeTarget;
    private GameObject lastActiveMenu;

    public TextMeshProUGUI catTirednessText;

    void Awake()
    {
        instance = this;
        if (bedContextMenu != null) bedContextMenu.SetActive(false);
        if (catContextMenu != null) catContextMenu.SetActive(false);
        if (settingsMenuPanel != null) settingsMenuPanel.SetActive(false);

        if (catSettingsPanel != null) catSettingsPanel.SetActive(false);
    }

    void Update()
    {
        if (activeTarget != null)
        {
            if (bedContextMenu.activeSelf)
            {
                bedContextMenu.transform.position = activeTarget.position;
            }

            if (catContextMenu.activeSelf)
            {
                catContextMenu.transform.position = activeTarget.position;

                if (activeTarget.TryGetComponent<CatController>(out var cat))
                {
                    if (catHungerBarInMenu != null)
                    {
                        catHungerBarInMenu.UpdateBar(cat.GetCurrentHunger(), cat.GetMaxHunger());
                    }
                    if (catTirednessText != null)
                    {
                        catTirednessText.text = $"Усталость: {cat.GetCurrentTiredness():F1}";
                    }
                }
            }
        }
        if ((bedContextMenu.activeSelf || catContextMenu.activeSelf) && Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider == null)
            {
                HideAllMenus();
                return;
            }
            if (hit.transform == activeTarget || hit.transform.IsChildOf(activeTarget))
            {
            }
            else
            {
                HideAllMenus();
            }
        }
    }

    public void ShowBedMenu(Transform target)
    {
        HideAllMenus();
        activeTarget = target;
        lastActiveMenu = bedContextMenu;
        bedContextMenu.transform.position = target.position;
        bedContextMenu.SetActive(true);
    }
    public void ShowCatMenu(Transform target)
    {
        HideAllMenus();
        activeTarget = target;
        lastActiveMenu = catContextMenu;
        catContextMenu.transform.position = target.position;
        catContextMenu.SetActive(true);
    }
    public void HideAllMenus()
    {
        if (activeTarget != null)
        {
            if (activeTarget.TryGetComponent<DraggableBed>(out var bed))
            {
                bed.OnMenuClosed();
            }
            if (activeTarget.TryGetComponent<CatController>(out var cat))
            {
                cat.ForceCloseMenu();
            }
        }
        if (bedContextMenu != null) bedContextMenu.SetActive(false);
        if (catContextMenu != null) catContextMenu.SetActive(false);
        activeTarget = null;
        lastActiveMenu = null;
    }

    public void OpenSettingsMenu()
    {
        if (lastActiveMenu != null)
        {
            lastActiveMenu.SetActive(false);
        }
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(true);
        }
    }
    public void HideSettingsMenu()
    {
        if (settingsMenuPanel != null)
        {
            settingsMenuPanel.SetActive(false);
        }
        if (lastActiveMenu != null)
        {
            lastActiveMenu.SetActive(true);
        }
    }

    public void OpenCatSettingsMenu()
    {
        if (lastActiveMenu != null)
        {
            lastActiveMenu.SetActive(false);
        }
        if (catSettingsPanel != null)
        {
            catSettingsPanel.SetActive(true);
        }
    }

    public void HideCatSettingsMenu()
    {
        if (catSettingsPanel != null)
        {
            catSettingsPanel.SetActive(false);
        }
        if (lastActiveMenu != null)
        {
            lastActiveMenu.SetActive(true);
        }
    }

    public bool IsCatMenuOpen()
    {
        return catContextMenu != null && catContextMenu.activeSelf;
    }
}