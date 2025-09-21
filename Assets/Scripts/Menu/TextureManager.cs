using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;

public class TextureManager : MonoBehaviour
{
    [Header("Ссылки на объекты сцены")]
    [Tooltip("Изображение кота, текстуру которого будем менять (UI Image)")]
    public Image catImage;

    [Tooltip("Панель меню для выбора текстуры")]
    public GameObject textureMenuPanel;

    [Header("Настройки текстур")]
    [Tooltip("Список всех доступных спрайтов (текстур) для кота")]
    public List<Sprite> catTextures;

    [Header("Шаблон и контейнер для кнопок")]
    [Tooltip("Объект-шаблон кнопки ПРЯМО ИЗ СЦЕНЫ. Он должен быть выключен.")]
    public GameObject textureButtonTemplate;

    [Tooltip("Контейнер, куда будут добавляться кнопки выбора текстуры (объект с Layout Group)")]
    public Transform buttonContainer;

    void Start()
    {
        if (textureMenuPanel != null)
        {
            textureMenuPanel.SetActive(false);
        }

        if (textureButtonTemplate != null)
        {
            textureButtonTemplate.SetActive(false);
        }

        PopulateTextureMenu();
    }

    void PopulateTextureMenu()
    {
        if (textureButtonTemplate == null || buttonContainer == null || catTextures.Count == 0)
        {
            Debug.LogError("Ошибка: Не все ссылки настроены в TextureManager! Проверьте шаблон кнопки, контейнер и список текстур.");
            return;
        }

        foreach (Sprite textureSprite in catTextures)
        {
            GameObject newButtonObject = Instantiate(textureButtonTemplate, buttonContainer);

            newButtonObject.SetActive(true);

            Button newButton = newButtonObject.GetComponent<Button>();
            Image buttonImage = newButtonObject.GetComponent<Image>();

            if (newButton != null && buttonImage != null)
            {
                buttonImage.sprite = textureSprite;

                Sprite currentSprite = textureSprite;
                newButton.onClick.AddListener(() => ChangeCatTexture(currentSprite));
            }
            else
            {
                Debug.LogError("В объекте-шаблоне кнопки отсутствует компонент 'Button' или 'Image'!");
            }
        }
    }

    public void ChangeCatTexture(Sprite newTexture)
    {
        if (catImage != null && newTexture != null)
        {
            catImage.sprite = newTexture;
        }
        CloseTextureMenu();
    }

    public void OpenTextureMenu()
    {
        if (textureMenuPanel != null)
        {
            textureMenuPanel.SetActive(true);
        }
    }

    public void CloseTextureMenu()
    {
        if (textureMenuPanel != null)
        {
            textureMenuPanel.SetActive(false);
        }
    }
}