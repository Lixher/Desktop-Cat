using UnityEngine;
using UnityEngine.UI; 

public class BedManager : MonoBehaviour
{
    [Header("������ �� �������")]
    [Tooltip("���������� ���� ������ � ��������� ������� (��������, CatBedPurple_0)")]
    public SpriteRenderer bedSpriteRenderer; 

    [Header("������ � ��������")]
    [Tooltip("�������� ���� ��� ������� �������� ������ ������")]
    public Sprite[] bedSprites;

    private int currentSpriteIndex = 0; 

    private const string BedColorSaveKey = "SelectedBedColorIndex";

    void Start()
    {
        if (bedSpriteRenderer == null)
        {
            Debug.LogError("������: �� �������� SpriteRenderer ��� ������� � ������� BedManager!");
            return;
        }
        if (bedSprites == null || bedSprites.Length == 0)
        {
            Debug.LogError("������: ������ �������� �������� ���� � ������� BedManager!");
            return;
        }

        currentSpriteIndex = PlayerPrefs.GetInt(BedColorSaveKey, 0);

        UpdateBedSprite();
    }

    public void NextColor()
    {
        Debug.Log("������ ����� NextColor()"); 

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
        Debug.Log("������ ����� PreviousColor()"); 

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
            Debug.Log("������ ������� ������� ��: " + bedSprites[currentSpriteIndex].name);
        }
    }

    private void SaveBedColor()
    {
        PlayerPrefs.SetInt(BedColorSaveKey, currentSpriteIndex);
        PlayerPrefs.Save(); 
        Debug.Log("������ " + currentSpriteIndex + " ��������.");
    }
}