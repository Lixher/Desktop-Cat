using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI; 
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(Image))]
public class LoveBarImageUI : MonoBehaviour
{
    [Header("������")]
    [Tooltip("���������� ���� ������ ����, � �������� ���� ������ CatNeeds")]
    public CatNeeds catNeeds; 

    private Image loveBarImage; 

    void Awake()
    {
        loveBarImage = GetComponent<Image>();

        if (loveBarImage.type != Image.Type.Filled)
        {
            Debug.LogWarning("��� Image �� ������� " + gameObject.name + " ������ ���� 'Filled', ����� ��� �������.", this);
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