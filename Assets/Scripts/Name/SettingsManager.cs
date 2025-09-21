using TMPro; 
using UnityEngine;

public class SettingsManager : MonoBehaviour
{

    [Header("Элементы UI")]
    [Tooltip("Поле для ввода имени кота")]
    public TMP_InputField nameInputField;

    [Header("Ссылки на игровые объекты")]
    [Tooltip("Перетащите сюда объект кота из иерархии сцены")]
    public CatController catController; 

    private const string CatNameKey = "CatName";

    void Start()
    {

        if (nameInputField != null)
        {
            nameInputField.onEndEdit.AddListener(ApplyName);
        }
        LoadNameToInputField();
    }

    private void LoadNameToInputField()
    {
        if (nameInputField != null)
        {
            nameInputField.text = PlayerPrefs.GetString(CatNameKey, "Name");
        }
    }

    private void ApplyName(string newName)
    {
        if (catController == null)
        {
            Debug.LogError("Ошибка! В SettingsManager не назначен 'catController'.");
            return; 
        }

        PlayerPrefs.SetString(CatNameKey, newName);
        PlayerPrefs.Save();

        catController.UpdateDisplayName(newName);

        Debug.Log("Имя кота обновлено и сохранено как: " + newName);
    }

    private void OnDestroy()
    {
        if (nameInputField != null)
        {
            nameInputField.onEndEdit.RemoveListener(ApplyName);
        }
    }
}