using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI; 
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(Image))]
public class LoveBarImageUI : MonoBehaviour
{
    [Header("Ссылки")]
    [Tooltip("Перетащите сюда объект Кота, у которого есть скрипт CatNeeds")]
    public CatNeeds catNeeds; 

    private Image loveBarImage; 

    void Awake()
    {
        loveBarImage = GetComponent<Image>();

        if (loveBarImage.type != Image.Type.Filled)
        {
            Debug.LogWarning("Тип Image на объекте " + gameObject.name + " должен быть 'Filled', чтобы бар работал.", this);
        }
    }



    void Update()
    {
        if (catNeeds == null)
        {
            return;
        }

        loveBarImage.fillAmount = catNeeds.GetLovePercentage();
    }
}