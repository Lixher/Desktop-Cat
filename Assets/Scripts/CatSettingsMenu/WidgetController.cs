using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Globalization;
using System.Linq;

// Новая структура для удобного сопоставления кодов погоды со спрайтами в инспекторе
[System.Serializable]
public class WeatherIconMap
{
    public string name; // Например: "Солнечно", "Облачно", "Дождь"
    public GameObject weatherSprite; // Сюда перетаскивается объект спрайта
    public List<int> weatherCodes; // Коды погоды от API для этого состояния
}

public class WidgetController : MonoBehaviour
{
    [Header("UI Элементы")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI weatherText;

    [Header("Настройки")]
    [Tooltip("Как часто обновлять погоду (в секундах). 900 = 15 минут.")]
    public float weatherUpdateInterval = 900f;

    [Header("Иконки погоды")]
    [Tooltip("Список сопоставлений кодов погоды и спрайтов")]
    public List<WeatherIconMap> weatherIcons;
    [Tooltip("Спрайт по умолчанию, если код погоды не найден")]
    public GameObject defaultWeatherIcon;

    private const string IpApiUrl = "https://api.ipify.org";
    private const string GeolocationApiUrl = "https://ipinfo.io/";
    // URL для погоды не изменился, так как current_weather=true уже включает weathercode
    private const string WeatherApiUrlFormat = "https://api.open-meteo.com/v1/forecast?latitude={0}&longitude={1}&current_weather=true";

    void Start()
    {
        // При запуске скрыть все иконки
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
        weatherText.text = "1/3: Определение сети...";
        UnityWebRequest ipRequest = UnityWebRequest.Get(IpApiUrl);
        yield return ipRequest.SendWebRequest();

        if (ipRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[WeatherWidget] Ошибка получения IP: {ipRequest.error}");
            weatherText.text = "Ошибка сети";
            yield break;
        }
        string publicIp = ipRequest.downloadHandler.text;
        Debug.Log($"[WeatherWidget] Получен IP: {publicIp}");

        weatherText.text = "2/3: Поиск города...";
        UnityWebRequest locationRequest = UnityWebRequest.Get(GeolocationApiUrl + publicIp + "/json");
        locationRequest.SetRequestHeader("User-Agent", "MyWeatherWidget/1.0");
        yield return locationRequest.SendWebRequest();

        if (locationRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[WeatherWidget] Ошибка получения геолокации: {locationRequest.error}");
            weatherText.text = "Ошибка геолокации";
            yield break;
        }

        IpInfoData locationData = JsonUtility.FromJson<IpInfoData>(locationRequest.downloadHandler.text);
        if (locationData == null || string.IsNullOrEmpty(locationData.loc))
        {
            Debug.LogError($"[WeatherWidget] Не удалось определить местоположение. Ответ сервера: {locationRequest.downloadHandler.text}");
            weatherText.text = "Город не найден";
            yield break;
        }

        string[] coords = locationData.loc.Split(',');
        if (coords.Length != 2)
        {
            weatherText.text = "Ошибка координат";
            yield break;
        }

        float latitude = float.Parse(coords[0], CultureInfo.InvariantCulture);
        float longitude = float.Parse(coords[1], CultureInfo.InvariantCulture);

        Debug.Log($"[WeatherWidget] Получено местоположение: {locationData.city} (Lat: {latitude}, Lon: {longitude})");

        weatherText.text = "3/3: Загрузка погоды...";
        string weatherUrl = string.Format(CultureInfo.InvariantCulture, WeatherApiUrlFormat, latitude, longitude);
        UnityWebRequest weatherRequest = UnityWebRequest.Get(weatherUrl);
        yield return weatherRequest.SendWebRequest();

        if (weatherRequest.result == UnityWebRequest.Result.Success)
        {
            WeatherInfo weatherInfo = JsonUtility.FromJson<WeatherInfo>(weatherRequest.downloadHandler.text);
            string temperature = Mathf.Round(weatherInfo.current_weather.temperature).ToString();
            weatherText.text = $"{temperature}°C";
            Debug.Log($"[WeatherWidget] Погода в {locationData.city}: {weatherText.text}");

            // Новая функция для обновления иконки погоды
            UpdateWeatherIcon(weatherInfo.current_weather.weathercode);
        }
        else
        {
            Debug.LogError($"[WeatherWidget] Ошибка получения погоды: {weatherRequest.error}");
            weatherText.text = "Ошибка погоды";
        }
    }

    // Новая функция для управления видимостью спрайтов
    void UpdateWeatherIcon(int weatherCode)
    {
        HideAllWeatherIcons();

        bool iconFound = false;
        // Ищем подходящий спрайт в списке
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

        // Если подходящий спрайт не найден, показываем спрайт по умолчанию
        if (!iconFound && defaultWeatherIcon != null)
        {
            defaultWeatherIcon.SetActive(true);
        }
    }

    void HideAllWeatherIcons()
    {
        // Скрываем все спрайты из списка
        foreach (var iconMap in weatherIcons)
        {
            if (iconMap.weatherSprite != null)
            {
                iconMap.weatherSprite.SetActive(false);
            }
        }
        // Скрываем спрайт по умолчанию
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
    public int weathercode; // Добавлено поле для кода погоды
}