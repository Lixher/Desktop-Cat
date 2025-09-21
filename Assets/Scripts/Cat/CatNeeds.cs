using UnityEngine;
using Debug = UnityEngine.Debug;

public class CatNeeds : MonoBehaviour
{
    public enum CatState
    {
        Content,        // Всем доволен
        VeryHungry,     // Очень голоден
        NeedsLove,      // Хочет ласки
        Tired           // Устал
    }

    [Header("Настройки Любви")]
    public float maxLove = 100f;
    public float loveDecayPerSecond = 0.5f;
    public float needsLoveThreshold = 40f;
    private float currentLove;

    [Header("Настройки Голода")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hungerDrainPerSecond = 1f;
    [SerializeField] private float fullnessThreshold = 99f;
    [SerializeField] private float veryHungryThreshold = 25f;

    [Header("Настройки Усталости")]
    [SerializeField] private float tirednessThreshold = 8.0f;
    private float currentTiredness = 0f;

    [Header("UI Компоненты")]
    [Tooltip("Перетащите сюда объект HungerBarCanvas из иерархии вашей сцены")]
    public GameObject hungerBarCanvas; 

    private HungerBar hungerBarInstance;
    private float currentHunger;

    [Header("Связи с другими системами")]
    public PettingModeController pettingModeController;

    private bool isEating = false;

    public bool IsTired => currentTiredness > tirednessThreshold;
    public bool IsVeryHungry => currentHunger < veryHungryThreshold;

    public CatState GetCurrentState()
    {
        if (IsVeryHungry) return CatState.VeryHungry;
        if (IsTired) return CatState.Tired;
        if (currentLove < needsLoveThreshold) return CatState.NeedsLove;
        return CatState.Content;
    }

    public void IncreaseTiredness(float amount) => currentTiredness += amount;
    public void Rest() => currentTiredness = 0f;

    public void Feed(float amount)
    {
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger);
        UpdateBar();
    }

    public bool CanBeFed() => currentHunger < fullnessThreshold;

    public void ShowHungerBar()
    {
        if (hungerBarCanvas == null) return; 

        isEating = true;
        hungerBarCanvas.SetActive(true);
        UpdateBar();
        Debug.Log($"[CatNeeds] ShowHungerBar() → активирован. Текущее значение: {currentHunger}/{maxHunger}");
    }

    public void HideHungerBar()
    {
        if (hungerBarCanvas == null) return; 

        isEating = false;
        hungerBarCanvas.SetActive(false);
        Debug.Log("[CatNeeds] HideHungerBar() → скрыт");
    }

    public float GetCurrentHunger() => currentHunger;
    public float GetMaxHunger() => maxHunger;
    public float GetCurrentTiredness() => currentTiredness;

    void Awake()
    {
        currentHunger = maxHunger;
        currentLove = maxLove;
    }

    void Start()
    {
        if (hungerBarCanvas != null)
        {
            hungerBarInstance = hungerBarCanvas.GetComponentInChildren<HungerBar>();

            if (hungerBarInstance == null)
            {
                Debug.LogError("Ошибка! На дочерних объектах 'HungerBarCanvas' не найден скрипт 'HungerBar'. Убедитесь, что он там есть.");
            }

            hungerBarCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("Критическая ошибка! Объект 'HungerBarCanvas' не назначен в инспекторе на скрипте CatNeeds! Перетащите его туда.");
        }
    }

    void Update()
    {
        HandleHungerDrain();
        HandleLoveDecay();
    }

    public float GetLovePercentage() => maxLove > 0 ? currentLove / maxLove : 0;

    public void IncreaseLove(float amount)
    {
        currentLove = Mathf.Min(currentLove + amount, maxLove);
    }

    private void HandleHungerDrain()
    {
        if (currentHunger > 0)
            currentHunger -= hungerDrainPerSecond * Time.deltaTime;

        currentHunger = Mathf.Max(currentHunger, 0);

        if (hungerBarInstance != null && hungerBarCanvas.activeSelf) 
            UpdateBar();
    }

    private void HandleLoveDecay()
    {
        if (currentLove > 0)
            currentLove -= loveDecayPerSecond * Time.deltaTime;

        currentLove = Mathf.Max(currentLove, 0);
    }

    private void UpdateBar()
    {
        if (hungerBarInstance != null)
        {
            hungerBarInstance.UpdateBar(currentHunger, maxHunger);
        }
    }
}