using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ChangeBedButtonController : MonoBehaviour
{
    [Tooltip("Assign the BedChangeMenuController to open the change-bed menu.")]
    [SerializeField] private BedChangeMenuController bedChangeMenuController;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogWarning("[ChangeBedButtonController] No Button component found on this GameObject.");
            return;
        }
        // Subscribe to the click event
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(OnButtonClicked);
    }

    /// <summary>
    /// Called when the Change Bed button is clicked. Opens the bed change menu.
    /// </summary>
    private void OnButtonClicked()
    {
        if (bedChangeMenuController != null)
        {
            bedChangeMenuController.OpenMenu();
        }
        else
        {
            Debug.LogWarning("[ChangeBedButtonController] BedChangeMenuController reference is not assigned.");
        }
    }
}