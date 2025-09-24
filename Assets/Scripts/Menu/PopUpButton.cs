using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PopupButton : MonoBehaviour
{
    public Button myButton;
    public TextMeshProUGUI popupText;
    public float fadeDuration = 2f; // Длительность исчезновения
    public float displayDuration = 3f; // Сколько времени текст будет видимым до исчезновения

    void Start()
    {
        // Добавляем слушателя к событию нажатия кнопки.
        myButton.onClick.AddListener(ShowPopup);
    }

    void ShowPopup()
    {
        // Запускаем корутину для отображения и плавного скрытия текста
        StartCoroutine(FadeText());
    }

    private IEnumerator FadeText()
    {
        // Устанавливаем начальный цвет (полупрозрачный)
        popupText.color = new Color(popupText.color.r, popupText.color.g, popupText.color.b, 0.7f);
        popupText.gameObject.SetActive(true);

        // Ждем displayDuration секунд
        yield return new WaitForSeconds(displayDuration);

        // Плавно уменьшаем прозрачность до нуля
        float startAlpha = popupText.color.a;
        float rate = 1.0f / fadeDuration;
        float progress = 0.0f;

        while (progress < 1.0)
        {
            Color tmpColor = popupText.color;
            popupText.color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, Mathf.Lerp(startAlpha, 0, progress));

            progress += rate * Time.deltaTime;

            yield return null;
        }

        // Убеждаемся, что альфа точно 0
        popupText.color = new Color(popupText.color.r, popupText.color.g, popupText.color.b, 0);
        popupText.gameObject.SetActive(false);
    }
}