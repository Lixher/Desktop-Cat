using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI; 
using Image = UnityEngine.UI.Image;
using image = UnityEngine.UI.Image; 
using Debug = UnityEngine.Debug; 

public class KittenMenuHungerBar : MonoBehaviour
{
    [SerializeField]
    private Image barImage;

    private CatController cat;

    void Start()
    {
        cat = FindObjectOfType<CatController>();

        if (cat == null)
        {
            Debug.LogError("ÏÎËÎÑÊÀ ÃÎËÎÄÀ Â ÌÅÍÞ ÊÎÒÅÍÊÀ ÍÅ ÍÀØËÀ ÊÎÒÀ! Óáåäèòåñü, ÷òî îáúåêò ñ CatController åñòü íà ñöåíå.");
            this.enabled = false;
            return;
        }

        if (barImage == null)
        {
            Debug.LogError("ÏÎËÎÑÊÀ ÃÎËÎÄÀ Â ÌÅÍÞ ÊÎÒÅÍÊÀ ÍÅ ÍÀØËÀ IMAGE! Ïåðåòàùèòå îáúåêò 'Bar' â ïîëå 'Bar Image' â èíñïåêòîðå.");
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