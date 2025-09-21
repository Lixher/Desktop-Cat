using UnityEngine;

public class BedChangeMenuInputListener : MonoBehaviour
{
    [Tooltip("Reference to the BedChangeMenuController to open the bed change menu.")]
    [SerializeField] private BedChangeMenuController bedChangeMenuController;

    private void Update()
    {
        // Listen for the "M" key press to open the bed change menu
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (bedChangeMenuController != null)
            {
                bedChangeMenuController.OpenMenu();
            }
            else
            {
                Debug.LogWarning("[BedChangeMenuInputListener] BedChangeMenuController reference is not assigned.");
            }
        }
    }
}