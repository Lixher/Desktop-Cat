using UnityEngine;
using UnityEngine.UI; 

public class CheckboxSaver : MonoBehaviour
{
    public Toggle toggle;

    private const string SaveKey = "AutosaveEnabled";

    void Start()
    {
        if (toggle == null)
        {
            Debug.LogError("Ошибка! Чекбокс (Toggle) не назначен в инспекторе для скрипта CheckboxSaver.");
            return;
        }

        LoadState();

        toggle.onValueChanged.AddListener(SaveState);
    }

    void LoadState()
    {
        int savedValue = PlayerPrefs.GetInt(SaveKey, 0);

        toggle.isOn = (savedValue == 1);
    }

    void SaveState(bool isOn)
    {
        int valueToSave = isOn ? 1 : 0;

        PlayerPrefs.SetInt(SaveKey, valueToSave);
        Debug.Log("Состояние чекбокса сохранено: " + (isOn ? "ВКЛЮЧЕНО" : "ВЫКЛЮЧЕНО"));

        SystemAutostart.SetAutostart(isOn);
    }
}