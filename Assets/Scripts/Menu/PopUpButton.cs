using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PopupButton : MonoBehaviour
{
    public Button myButton;
    public TextMeshProUGUI popupText;
    public float fadeDuration = 2f; // ������������ ������������
    public float displayDuration = 3f; // ������� ������� ����� ����� ������� �� ������������

    void Start()
    {
        // ��������� ��������� � ������� ������� ������.
        myButton.onClick.AddListener(ShowPopup);
    }

    void ShowPopup()
    {
        // ��������� �������� ��� ����������� � �������� ������� ������
        StartCoroutine(FadeText());
    }

    private IEnumerator FadeText()
    {
        // ������������� ��������� ���� (��������������)
        popupText.color = new Color(popupText.color.r, popupText.color.g, popupText.color.b, 0.7f);
        popupText.gameObject.SetActive(true);

        // ���� displayDuration ������
        yield return new WaitForSeconds(displayDuration);

        // ������ ��������� ������������ �� ����
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

        // ����������, ��� ����� ����� 0
        popupText.color = new Color(popupText.color.r, popupText.color.g, popupText.color.b, 0);
        popupText.gameObject.SetActive(false);
    }
}