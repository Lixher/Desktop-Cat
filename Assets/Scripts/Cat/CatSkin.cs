using UnityEngine;

[CreateAssetMenu(fileName = "NewCatSkin", menuName = "Screen Cat/Cat Skin")]
public class CatSkin : ScriptableObject
{
    [Tooltip("Название скина (для отображения в UI, если нужно)")]
    public string skinName;

    [Tooltip("Иконка скина для отображения на кнопке выбора")]
    public Sprite icon;

    [Tooltip("Переопределитель анимаций для этого скина")]
    public AnimatorOverrideController overrideController;
}