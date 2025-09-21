using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BedOptionButtonController : MonoBehaviour
{
    [Tooltip("Text component displaying the bed name.")]
    [SerializeField] private TextMeshProUGUI label;

    private string bedName;
    private BedChangeMenuController menuController;
    private Button button;

    private void Awake()
    {
        // Cache Button component and hook up click event
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnClick);
        else
            Debug.LogWarning("[BedOptionButton] No Button component found.");
    }

    /// <summary>
    /// Initializes this button with the given bed name and parent menu controller.
    /// </summary>
    /// <param name="bedName">Display name of the bed.</param>
    /// <param name="controller">Reference to the menu controller.</param>
    public void Initialize(string bedName, BedChangeMenuController controller)
    {
        this.bedName = bedName;
        menuController = controller;

        if (label != null)
            label.text = bedName;
        else
            Debug.LogWarning("[BedOptionButton] No TextMeshProUGUI assigned to label.");
    }

    /// <summary>
    /// Called when the user clicks this button.
    /// </summary>
    private void OnClick()
    {
        if (menuController != null)
            menuController.OnBedOptionSelected(bedName);
        else
            Debug.LogWarning("[BedOptionButton] MenuController is null.");
    }
}