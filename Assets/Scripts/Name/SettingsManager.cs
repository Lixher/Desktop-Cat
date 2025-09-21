using TMPro; 
using UnityEngine;

public class SettingsManager : MonoBehaviour
{

    [Header("�������� UI")]
    [Tooltip("���� ��� ����� ����� ����")]
    public TMP_InputField nameInputField;

    [Header("������ �� ������� �������")]
    [Tooltip("���������� ���� ������ ���� �� �������� �����")]
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
            Debug.LogError("������! � SettingsManager �� �������� 'catController'.");
            return; 
        }

        PlayerPrefs.SetString(CatNameKey, newName);
        PlayerPrefs.Save();

        catController.UpdateDisplayName(newName);

        Debug.Log("��� ���� ��������� � ��������� ���: " + newName);
    }

    private void OnDestroy()
    {
        if (nameInputField != null)
        {
            nameInputField.onEndEdit.RemoveListener(ApplyName);
        }
    }
}