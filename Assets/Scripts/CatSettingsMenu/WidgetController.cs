using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Globalization;
using System.Linq;

// ����� ��������� ��� �������� ������������� ����� ������ �� ��������� � ����������
[System.Serializable]
public class WeatherIconMap
{
    public string name; // ��������: "��������", "�������", "�����"
    public GameObject weatherSprite; // ���� ��������������� ������ �������
    public List<int> weatherCodes; // ���� ������ �� API ��� ����� ���������
}

public class WidgetController : MonoBehaviour
{
    [Header("UI ��������")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI weatherText;

    [Header("���������")]
    [Tooltip("��� ����� ��������� ������ (� ��������). 900 = 15 �����.")]
    public float weatherUpdateInterval = 900f;

    [Header("������ ������")]
    [Tooltip("������ ������������� ����� ������ � ��������")]
    public List<WeatherIconMap> weatherIcons;
    [Tooltip("������ �� ���������, ���� ��� ������ �� ������")]
    public GameObject defaultWeatherIcon;

    private const string IpApiUrl = "https://api.ipify.org";
    private const string GeolocationApiUrl = "https://ipinfo.io/";
    // URL ��� ������ �� ���������, ��� ��� current_weather=true ��� �������� weathercode
    private const string WeatherApiUrlFormat = "https://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}&current_weather=true";

    void Start()
    {
        // ��� ������� ������ ��� ������
        HideAllWeatherIcons();
        InvokeRepeating(nameof(UpdateTime), 0f, 1f);
        InvokeRepeating(nameof(StartWeatherUpdateSequence), 0f, weatherUpdateInterval);
    }

    void UpdateTime()
    {
        if (timeText != null)
        {
            timeText.text = DateTime.Now.ToString("HH:mm");
        }
    }

    public void StartWeatherUpdateSequence()
    {
        if (weatherText != null)
        {
            StartCoroutine(GetWeatherRoutine());
        }
    }

    private IEnumerator GetWeatherRoutine()
    {
        weatherText.text = "1/3: ����������� ����...";
        UnityWebRequest ipRequest = UnityWebRequest.Get(IpApiUrl);
        yield return ipRequest.SendWebRequest();

        if (ipRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[WeatherWidget] ������ ��������� IP: {ipRequest.error}");
            weatherText.text = "������ ����";
            yield break;
        }
        string publicIp = ipRequest.downloadHandler.text;
        Debug.Log($"[WeatherWidget] ������� IP: {publicIp}");

        weatherText.text = "2/3: ����� ������...";
        UnityWebRequest locationRequest = UnityWebRequest.Get(GeolocationApiUrl + publicIp + "/json");
        locationRequest.SetRequestHeader("User-Agent", "MyWeatherWidget/1.0");
        yield return locationRequest.SendWebRequest();

        if (locationRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[WeatherWidget] ������ ��������� ����������: {locationRequest.error}");
            weatherText.text = "������ ����������";
            yield break;
        }

        IpInfoData locationData = JsonUtility.FromJson<IpInfoData>(locationRequest.downloadHandler.text);
        if (locationData == null || string.IsNullOrEmpty(locationData.loc))
        {
            Debug.LogError($"[WeatherWidget] �� ������� ���������� ��������������. ����� �������: {locationRequest.downloadHandler.text}");
            weatherText.text = "����� �� ������";
            yield break;
        }

        string[] coords = locationData.loc.Split(',');
        if (coords.Length != 2)
        {
            weatherText.text = "������ ���������";
            yield break;
        }

        float latitude = float.Parse(coords[0], CultureInfo.InvariantCulture);
        float longitude = float.Parse(coords[1], CultureInfo.InvariantCulture);

        Debug.Log($"[WeatherWidget] �������� ��������������: {locationData.city} (Lat: {latitude}, Lon: {longitude})");

        weatherText.text = "3/3: �������� ������...";
        string weatherUrl = string.Format(CultureInfo.InvariantCulture, WeatherApiUrlFormat, latitude, longitude);
        UnityWebRequest weatherRequest = UnityWebRequest.Get(weatherUrl);
        yield return weatherRequest.SendWebRequest();

        if (weatherRequest.result == UnityWebRequest.Result.Success)
        {
            WeatherInfo weatherInfo = JsonUtility.FromJson<WeatherInfo>(weatherRequest.downloadHandler.text);
            string temperature = Mathf.Round(weatherInfo.current_weather.temperature).ToString();
            weatherText.text = $"{temperature}�C";
            Debug.Log($"[WeatherWidget] ������ � {locationData.city}: {weatherText.text}");

            // ����� ������� ��� ���������� ������ ������
            UpdateWeatherIcon(weatherInfo.current_weather.weathercode);
        }
        else
        {
            Debug.LogError($"[WeatherWidget] ������ ��������� ������: {weatherRequest.error}");
            weatherText.text = "������ ������";
        }
    }

    // ����� ������� ��� ���������� ���������� ��������
    void UpdateWeatherIcon(int weatherCode)
    {
        HideAllWeatherIcons();

        bool iconFound = false;
        // ���� ���������� ������ � ������
        foreach (var iconMap in weatherIcons)
        {
            if (iconMap.weatherCodes.Contains(weatherCode))
            {
                if (iconMap.weatherSprite != null)
                {
                    iconMap.weatherSprite.SetActive(true);
                    iconFound = true;
                    break;
                }
            }
        }

        // ���� ���������� ������ �� ������, ���������� ������ �� ���������
        if (!iconFound && defaultWeatherIcon != null)
        {
            defaultWeatherIcon.SetActive(true);
        }
    }

    void HideAllWeatherIcons()
    {
        // �������� ��� ������� �� ������
        foreach (var iconMap in weatherIcons)
        {
            if (iconMap.weatherSprite != null)
            {
                iconMap.weatherSprite.SetActive(false);
            }
        }
        // �������� ������ �� ���������
        if (defaultWeatherIcon != null)
        {
            defaultWeatherIcon.SetActive(false);
        }
    }
}

[System.Serializable]
public class IpInfoData
{
    public string ip;
    public string city;
    public string region;
    public string country;
    public string loc;
}

[System.Serializable]
public class WeatherInfo
{
    public CurrentWeather current_weather;
}

[System.Serializable]
public class CurrentWeather
{
    public float temperature;
    public int weathercode; // ��������� ���� ��� ���� ������
}