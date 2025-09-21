using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(Toggle))]
public class RgbToggleController : MonoBehaviour
{
    public CatController catController;

    private Toggle toggle;
    private const string RgbSaveKey = "RgbCatEnabled"; 

    private void Start()
    {
        toggle = GetComponent<Toggle>();

        if (catController == null)
        {
            Debug.LogError("На RgbToggleController не назначена ссылка на CatController!");
            toggle.interactable = false;
            return;
        }

        toggle.onValueChanged.AddListener(UpdateAndSaveState);

        Invoke(nameof(LoadState), 0f);
    }

    private void LoadState()
    {
        int savedValue = PlayerPrefs.GetInt(RgbSaveKey, 0);
        bool isEnabled = (savedValue == 1);

        toggle.SetIsOnWithoutNotify(isEnabled);
        catController.ToggleRgbEffect(isEnabled);
    }

    private void UpdateAndSaveState(bool isEnabled)
    {
        catController.ToggleRgbEffect(isEnabled);
        int valueToSave = isEnabled ? 1 : 0;
        PlayerPrefs.SetInt(RgbSaveKey, valueToSave);
    }
}