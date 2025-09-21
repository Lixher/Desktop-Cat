using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PettingManager : MonoBehaviour
{
    [Header("������ ��� ������")]
    [Tooltip("���������� ���� ������ � �������� �������� (Particle System)")]
    public GameObject heartEffectPrefab;

    [Header("��������� �������")]
    [Tooltip("��������� ������ ��� ����� ����� ���������� ��������")]
    public Vector3 effectOffset = new Vector3(0, 0.5f, 0);

    [Header("��������� ������")]
    [Tooltip("��� ����� ����� ������� ���� (� ��������). ������ �� ����� �������.")]
    public float petCooldown = 0.5f;

    [Header("������� �� ������")]
    [Tooltip("������� ����� ������ �� ���� ������������")]
    public float loveGainedPerPet = 5f;

    private CatNeeds catNeeds;

    [HideInInspector]
    public bool isPettingModeActive = false;

    private float lastPetTime = -1f;

    void Start()
    {
        if (heartEffectPrefab == null)
        {
            Debug.LogError("������: ������ ������� �������� �� �������� � PettingManager!", this.gameObject);
        }

        catNeeds = GetComponent<CatNeeds>();
        if (catNeeds == null)
        {
            Debug.LogError("�� ���� �� ������ ��������� CatNeeds! ��� ����� �� ����� ��������.", this.gameObject);
        }
    }

    private void OnMouseOver()
    {
        if (!isPettingModeActive)
        {
            return;
        }

        if (Input.GetMouseButton(0))
        {
            if (Time.time >= lastPetTime + petCooldown)
            {
                ShowHeartEffect();
                lastPetTime = Time.time;

                if (catNeeds != null)
                {
                    catNeeds.IncreaseLove(loveGainedPerPet);
                }
            }
        }
    }

    private void ShowHeartEffect()
    {
        if (heartEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position + effectOffset;
            Instantiate(heartEffectPrefab, spawnPosition, Quaternion.identity);
        }
    }
}