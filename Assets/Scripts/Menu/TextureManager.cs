using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Debug = UnityEngine.Debug;
using Image = UnityEngine.UI.Image;

public class TextureManager : MonoBehaviour
{
    [Header("������ �� ������� �����")]
    [Tooltip("����������� ����, �������� �������� ����� ������ (UI Image)")]
    public Image catImage;

    [Tooltip("������ ���� ��� ������ ��������")]
    public GameObject textureMenuPanel;

    [Header("��������� �������")]
    [Tooltip("������ ���� ��������� �������� (�������) ��� ����")]
    public List<Sprite> catTextures;

    [Header("������ � ��������� ��� ������")]
    [Tooltip("������-������ ������ ����� �� �����. �� ������ ���� ��������.")]
    public GameObject textureButtonTemplate;

    [Tooltip("���������, ���� ����� ����������� ������ ������ �������� (������ � Layout Group)")]
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
            Debug.LogError("������: �� ��� ������ ��������� � TextureManager! ��������� ������ ������, ��������� � ������ �������.");
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
                Debug.LogError("� �������-������� ������ ����������� ��������� 'Button' ��� 'Image'!");
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