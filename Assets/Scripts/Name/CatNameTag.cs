using TMPro; 
using UnityEngine;

public class CatNameTag : MonoBehaviour
{
    [Tooltip("Перетащите сюда дочерний объект с текстом имени (NameText)")]
    public TextMeshProUGUI nameTextComponent;

    public void UpdateName(string newName)
    {
        if (nameTextComponent != null)
        {
            nameTextComponent.text = newName;
        }
        else
        {
            Debug.LogError("Ошибка! На объекте '" + gameObject.name + "' в скрипте CatNameTag не назначен 'nameTextComponent'.");
        }
    }
}