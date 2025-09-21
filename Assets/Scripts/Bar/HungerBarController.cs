using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;

public class HungerBarController : MonoBehaviour
{
    [SerializeField]
    private Image barImage;

    private CatController cat;

    void Start()
    {
        cat = FindObjectOfType<CatController>();

        if (cat == null)
        {
            Debug.LogError("������� ������ �� ����� ����! ���������, ��� ������ � CatController ���� �� �����.");
            this.enabled = false;
            return;
        }

        if (barImage == null)
        {
            Debug.LogError("������� ������ �� ����� IMAGE! ���������� ������ 'Bar' � ���� 'Bar Image' � ����������.");
            this.enabled = false;
            return;
        }
    }

    void Update()
    {
        float currentHunger = cat.GetCurrentHunger();
        float maxHunger = cat.GetMaxHunger();

        if (maxHunger > 0)
        {
            barImage.fillAmount = Mathf.Clamp01(currentHunger / maxHunger);
        }
    }
}