using UnityEngine;
using UnityEngine.UI;

public class BedChangeMenuCloseButtonController : MonoBehaviour
{
    [Tooltip("Reference to the menu controller that will be closed.")]
    [SerializeField] private BedChangeMenuController menuController;

    private Button button;

    private void Awake()
    {
        // Cache Button component and hook up click event
        button = GetComponent<Button>();
        if (button != null)
            button.onClick.AddListener(OnCloseClicked);
        else
            Debug.LogWarning("[BedChangeMenuCloseButton] No Button component found.");
    }

    /// <summary>
    /// Called when the close button is clicked.
    /// </summary>
    private void OnCloseClicked()
    {
        if (menuController != null)
            menuController.CloseMenu();
        else
            Debug.LogWarning("[BedChangeMenuCloseButton] MenuController is null.");
    }
}