using UnityEngine;
using UnityEngine.UI;

public class ColorPickerController : MonoBehaviour
{
    [Header("UI Ёлементы ѕикера")]
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;
    public Slider alphaSlider;
    public Image colorPreview;

    private Image targetImage;

    void Start()
    {
        redSlider.onValueChanged.AddListener(OnColorSliderChanged);
        greenSlider.onValueChanged.AddListener(OnColorSliderChanged);
        blueSlider.onValueChanged.AddListener(OnColorSliderChanged);
        alphaSlider.onValueChanged.AddListener(OnColorSliderChanged);
    }

    public void OpenPicker(Image imageToControl)
    {
        targetImage = imageToControl;

        redSlider.value = targetImage.color.r;
        greenSlider.value = targetImage.color.g;
        blueSlider.value = targetImage.color.b;
        alphaSlider.value = targetImage.color.a;

        UpdateColorPreview();

        gameObject.SetActive(true);
    }

    private void OnColorSliderChanged(float value)
    {
        if (targetImage == null) return;

        Color newColor = new Color(redSlider.value, greenSlider.value, blueSlider.value, alphaSlider.value);

        targetImage.color = newColor;

        UpdateColorPreview();
    }

    private void UpdateColorPreview()
    {
        colorPreview.color = new Color(redSlider.value, greenSlider.value, blueSlider.value, 1f); 
    }

    public void ClosePicker()
    {
        gameObject.SetActive(false);
    }
}