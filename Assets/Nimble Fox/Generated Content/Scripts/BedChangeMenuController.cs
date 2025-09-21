using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BedChangeMenuController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Root GameObject of the menu Canvas (disable to hide).")]
    [SerializeField] private GameObject menuRoot;

    [Tooltip("Container (e.g., a Vertical Layout Group) where bed buttons will be spawned.")]
    [SerializeField] private Transform optionsContainer;

    [Tooltip("Prefab of the BedOptionButtonController to instantiate per bed.")]
    [SerializeField] private BedOptionButtonController bedOptionButtonPrefab;

    [Header("Bed Data")]
    [Tooltip("List of bed names to display in the menu. Fill in inspector.")]
    [SerializeField] private List<string> bedNames = new List<string>();

    // Holds references to instantiated buttons so we can clear them later
    private readonly List<BedOptionButtonController> optionButtons = new List<BedOptionButtonController>();

    private void Awake()
    {
        if (menuRoot != null)
            menuRoot.SetActive(false);
    }

    private void Update()
    {
        // Close menu on Escape key
        if (menuRoot != null && menuRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseMenu();
        }
    }

    /// <summary>
    /// Opens the bed change menu and populates it with buttons.
    /// </summary>
    public void OpenMenu()
    {
        if (menuRoot == null || optionsContainer == null || bedOptionButtonPrefab == null)
        {
            Debug.LogWarning("[BedChangeMenu] Missing UI references. Cannot open menu.");
            return;
        }

        GenerateOptions();
        menuRoot.SetActive(true);
    }

    /// <summary>
    /// Closes the bed change menu and clears instantiated buttons.
    /// </summary>
    public void CloseMenu()
    {
        if (menuRoot != null)
            menuRoot.SetActive(false);

        ClearOptions();
    }

    /// <summary>
    /// Instantiates one button per bed name.
    /// </summary>
    private void GenerateOptions()
    {
        ClearOptions();
        for (int i = 0; i < bedNames.Count; i++)
        {
            string nameToShow = bedNames[i];
            var btn = Instantiate(bedOptionButtonPrefab, optionsContainer);
            btn.Initialize(nameToShow, this);
            optionButtons.Add(btn);
        }
    }

    /// <summary>
    /// Destroys all instantiated option buttons.
    /// </summary>
    private void ClearOptions()
    {
        foreach (var btn in optionButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        optionButtons.Clear();
    }

    /// <summary>
    /// Called by BedOptionButtonController when a bed is selected.
    /// </summary>
    /// <param name="bedName">Name of the selected bed.</param>
    public void OnBedOptionSelected(string bedName)
    {
        Debug.Log($"[BedChangeMenu] Selected bed: {bedName}");
        // Integration point: call into existing BedManager or system here.
        CloseMenu();
    }
}