using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PettingManager : MonoBehaviour
{
    [Header("Эффект при глажке")]
    [Tooltip("Перетащите сюда префаб с эффектом сердечек (Particle System)")]
    public GameObject heartEffectPrefab;

    [Header("Настройки эффекта")]
    [Tooltip("Насколько высоко над котом будут появляться сердечки")]
    public Vector3 effectOffset = new Vector3(0, 0.5f, 0);

    [Header("Настройки глажки")]
    [Tooltip("Как часто можно гладить кота (в секундах). Защита от спама кликами.")]
    public float petCooldown = 0.5f;

    [Header("Награда за глажку")]
    [Tooltip("Сколько любви дается за одно поглаживание")]
    public float loveGainedPerPet = 5f;

    private CatNeeds catNeeds;

    [HideInInspector]
    public bool isPettingModeActive = false;

    private float lastPetTime = -1f;

    void Start()
    {
        if (heartEffectPrefab == null)
        {
            Debug.LogError("Ошибка: Префаб эффекта сердечек не назначен в PettingManager!", this.gameObject);
        }

        catNeeds = GetComponent<CatNeeds>();
        if (catNeeds == null)
        {
            Debug.LogError("На коте не найден компонент CatNeeds! Бар любви не будет работать.", this.gameObject);
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