using UnityEngine;
using UnityEngine.UI; 

public class VsyncManager : MonoBehaviour 
{

    [Tooltip("Чекбокс для управления VSync")]
    public Toggle vsyncToggle;

    private const string VSyncKey = "VSyncEnabled";

    void Awake()
    {
        if (PlayerPrefs.HasKey(VSyncKey))
        {
            LoadSettings();
        }
        else
        {
            SetVSync(true);
            vsyncToggle.isOn = true;
            PlayerPrefs.SetInt(VSyncKey, 1); 
            PlayerPrefs.Save(); 
        }
    }

    void Start()
    {
        vsyncToggle.isOn = (QualitySettings.vSyncCount > 0);

        vsyncToggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        SetVSync(isOn);
        SaveSettings();
    }

    private void SetVSync(bool vsyncIsOn)
    {
        QualitySettings.vSyncCount = vsyncIsOn ? 1 : 0;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt(VSyncKey, vsyncToggle.isOn ? 1 : 0);

        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        int vsyncValue = PlayerPrefs.GetInt(VSyncKey);

        SetVSync(vsyncValue == 1);
    }
}