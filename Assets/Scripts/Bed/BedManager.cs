using UnityEngine;
using UnityEngine.UI; 

public class BedManager : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    [Tooltip("Перетащите сюда объект с картинкой кровати (например, CatBedPurple_0)")]
    public SpriteRenderer bedSpriteRenderer; 

    [Header("Данные о кроватях")]
    [Tooltip("Добавьте сюда все спрайты кроватей разных цветов")]
    public Sprite[] bedSprites;

    private int currentSpriteIndex = 0; 

    private const string BedColorSaveKey = "SelectedBedColorIndex";

    void Start()
    {
        if (bedSpriteRenderer == null)
        {
            Debug.LogError("Ошибка: Не назначен SpriteRenderer для кровати в скрипте BedManager!");
            return;
        }
        if (bedSprites == null || bedSprites.Length == 0)
        {
            Debug.LogError("Ошибка: Массив спрайтов кроватей пуст в скрипте BedManager!");
            return;
        }

        currentSpriteIndex = PlayerPrefs.GetInt(BedColorSaveKey, 0);

        UpdateBedSprite();
    }

    public void NextColor()
    {
        Debug.Log("Вызван метод NextColor()"); 

        currentSpriteIndex++;

        if (currentSpriteIndex >= bedSprites.Length)
        {
            currentSpriteIndex = 0;
        }

        UpdateBedSprite();
        SaveBedColor();
    }

    public void PreviousColor()
    {
        Debug.Log("Вызван метод PreviousColor()"); 

        currentSpriteIndex--;

        if (currentSpriteIndex < 0)
        {
            currentSpriteIndex = bedSprites.Length - 1; 
        }

        UpdateBedSprite();
        SaveBedColor();
    }

    private void UpdateBedSprite()
    {
        if (currentSpriteIndex >= 0 && currentSpriteIndex < bedSprites.Length)
        {
            bedSpriteRenderer.sprite = bedSprites[currentSpriteIndex];
            Debug.Log("Спрайт кровати изменен на: " + bedSprites[currentSpriteIndex].name);
        }
    }

    private void SaveBedColor()
    {
        PlayerPrefs.SetInt(BedColorSaveKey, currentSpriteIndex);
        PlayerPrefs.Save(); 
        Debug.Log("Индекс " + currentSpriteIndex + " сохранен.");
    }
}