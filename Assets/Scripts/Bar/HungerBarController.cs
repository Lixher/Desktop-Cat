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
            Debug.LogError("ÏÎËÎÑÊÀ ÃÎËÎÄÀ ÍÅ ÍÀØËÀ ÊÎÒÀ! Óáåäèòåñü, ÷òî îáúåêò ñ CatController åñòü íà ñöåíå.");
            this.enabled = false;
            return;
        }

        if (barImage == null)
        {
            Debug.LogError("ÏÎËÎÑÊÀ ÃÎËÎÄÀ ÍÅ ÍÀØËÀ IMAGE! Ïåðåòàùèòå îáúåêò 'Bar' â ïîëå 'Bar Image' â èíñïåêòîðå.");
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