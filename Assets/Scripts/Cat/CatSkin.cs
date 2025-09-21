using UnityEngine;

[CreateAssetMenu(fileName = "NewCatSkin", menuName = "Screen Cat/Cat Skin")]
public class CatSkin : ScriptableObject
{
    [Tooltip("�������� ����� (��� ����������� � UI, ���� �����)")]
    public string skinName;

    [Tooltip("������ ����� ��� ����������� �� ������ ������")]
    public Sprite icon;

    [Tooltip("���������������� �������� ��� ����� �����")]
    public AnimatorOverrideController overrideController;
}