using TMPro; 
using UnityEngine;

public class CatNameTag : MonoBehaviour
{
    [Tooltip("���������� ���� �������� ������ � ������� ����� (NameText)")]
    public TextMeshProUGUI nameTextComponent;

    public void UpdateName(string newName)
    {
        if (nameTextComponent != null)
        {
            nameTextComponent.text = newName;
        }
        else
        {
            Debug.LogError("������! �� ������� '" + gameObject.name + "' � ������� CatNameTag �� �������� 'nameTextComponent'.");
        }
    }
}