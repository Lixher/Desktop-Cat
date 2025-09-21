using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class SettingsManaged : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    [Tooltip("Перетащите сюда объект кота, чей размер будем менять")]
    public Transform catTransform;

    [Header("UI для настройки размера")]
    [Tooltip("Перетащите сюда ваш UI Slider")]
    public Slider sizeSlider;

    [Tooltip("Перетащите сюда текстовый элемент для отображения значения")]
    public TextMeshProUGUI sizeValueText;

    private const string CatSizeKey = "CatSize";

    void Start()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        float savedSize = PlayerPrefs.GetFloat(CatSizeKey, 1.0f);

        ApplyCatSize(savedSize);

        if (sizeSlider != null)
        {
            sizeSlider.value = savedSize;
        }
    }

    public void OnSizeSliderChanged(float newSize)
    {
        ApplyCatSize(newSize);
        //SaveSettings(); из за этого слайдер не передвигается
    }

    private void ApplyCatSize(float size)
    {
        if (catTransform != null)
        {
            catTransform.localScale = new Vector3(size, size, 1f);
        }

        if (sizeValueText != null)
        {
            sizeValueText.text = $"Размер: {size:F1}x"; 
        }
    }

    private void SaveSettings()
    {
        if (sizeSlider != null)
        {
            PlayerPrefs.SetFloat(CatSizeKey, sizeSlider.value);
            PlayerPrefs.Save(); 
        }
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
    }
}