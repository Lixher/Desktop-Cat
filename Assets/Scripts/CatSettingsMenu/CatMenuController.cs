using UnityEngine;
using UnityEngine.UI; 

public class CatMenuController : MonoBehaviour
{
    [Header("Элементы меню")]
    public Toggle widgetToggle; 

    public GameObject weatherWidget;

    private const string WidgetEnabledKey = "WeatherWidgetEnabled";

    void Start()
    {
        if (widgetToggle == null || weatherWidget == null)
        {
            Debug.LogError("Не все ссылки назначены в CatMenuController!");
            return;
        }

        bool isEnabled = PlayerPrefs.GetInt(WidgetEnabledKey, 0) == 1;
        widgetToggle.isOn = isEnabled;
        weatherWidget.SetActive(isEnabled);

        widgetToggle.onValueChanged.AddListener(OnWidgetToggleChanged);
    }

    private void OnWidgetToggleChanged(bool isEnabled)
    {
        weatherWidget.SetActive(isEnabled);

        PlayerPrefs.SetInt(WidgetEnabledKey, isEnabled ? 1 : 0);
        PlayerPrefs.Save(); 
        Debug.Log("Состояние виджета сохранено: " + isEnabled);
    }
}