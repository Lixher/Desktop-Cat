using System;
using System.Collections; 
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(CatNeeds))]
[RequireComponent(typeof(CatMovement))]
[RequireComponent(typeof(CatAnimation))]
public class CatController : MonoBehaviour
{
    [Header("Настройки RGB")]
    [Tooltip("Скорость переливания цветов")]
    public float rgbSpeed = 0.5f;

    private const string RgbSaveKey = "RgbCatEnabled";

    private SpriteRenderer spriteRenderer;
    private Coroutine rgbCoroutine;

    [Header("Настройки Имени Кота")]
    [Tooltip("Перетащите сюда дочерний объект NameCanvas")]
    public CatNameTag nameTag; 

    private const string CatNameKey = "CatName"; 

    public static List<DraggableBed> AllBeds = new List<DraggableBed>();

    public enum CatState { Idle, Walking, Waiting, Attacking, WalkingToBed, Sleeping, Eating }
    [SerializeField] private CatState currentState;
    private bool isBrainPaused = false;
    private bool isSleepingEndlessly = false;

    [Header("Вероятности и циклы")]
    [Range(0f, 1f)][SerializeField] private float attackChance = 0.25f;
    [Header("Настройки цикла")]
    [SerializeField] private int minWalksInCycle = 2;
    [SerializeField] private int maxWalksInCycle = 5;
    [Header("Скорости")]
    [SerializeField] private float attackSpeedMultiplier = 1.5f;
    [SerializeField] private float feedAmountPerSecond = 25f;


    [Header("Длительность состояний (в секундах)")]
    [SerializeField] private float idleDuration = 8.0f;
    [SerializeField] private float minWaitTime = 1.0f;
    [SerializeField] private float maxWaitTime = 2.0f;
    [SerializeField] private float sleepDuration = 10.0f;

    [Tooltip("Минимальное время ожидания после закрытия меню")]
    [SerializeField] private float minWaitAfterMenu = 2.0f;
    [Tooltip("Максимальное время ожидания после закрытия меню")]
    [SerializeField] private float maxWaitAfterMenu = 4.0f;


    [Header("Навигация и дистанции")]
    [SerializeField] private float maxTravelTime = 15f;

    [Tooltip("Как близко кот должен подойти к цели, чтобы считать ее достигнутой")]
    [SerializeField] private float distanceToTargetThreshold = 0.1f;

    [Tooltip("На каком расстоянии от мыши кот ее 'ловит'")]
    [SerializeField] private float attackRange = 0.2f;



    [Header("Префабы")]
    public GameObject mousePrefab;

    private CatMovement movement;
    private CatAnimation catAnimation;
    private Collider2D catCollider;
    private CatNeeds catNeeds;
    private GameObject currentMouseTarget;
    private Transform dynamicTargetTransform;
    private DraggableBed currentBed = null;
    private Vector3 targetPosition;
    private float stateTimer;
    private float travelTimer;
    private int walksInCurrentCycle;
    private int walksCompleted = 0;
    private bool isPausedByMenu = false;

    void Awake()
    {
        movement = GetComponent<CatMovement>();
        catAnimation = GetComponent<CatAnimation>();
        catCollider = GetComponent<Collider2D>();
        catNeeds = GetComponent<CatNeeds>();

        if (catAnimation != null)
        {
            spriteRenderer = catAnimation.GetComponent<SpriteRenderer>();
        }
        else
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("Не удалось найти SpriteRenderer для RGB-эффекта на объекте кота!");
        }

        InitializeRgbState();
    }


    void Start()
    {
        LoadAndApplyName();

        Debug.Log("--- GAME START ---");
        float startChoice = Random.value;
        if (startChoice < 0.3f)
        {
            Debug.Log($"<color=cyan>Старт:</color> Шанс {startChoice:F2}. Начинаем в состоянии отдыха.");
            EnterIdleState(true);
        }
        else
        {
            Debug.Log($"<color=cyan>Старт:</color> Шанс {startChoice:F2}. Начинаем с нового цикла действий.");
            StartNewCycle();
        }
    }

    void Update()
    {
        if (isBrainPaused || isPausedByMenu) return;

        stateTimer -= Time.deltaTime;
        switch (currentState)
        {
            case CatState.Idle: if (stateTimer <= 0) StartNewCycle(); break;
            case CatState.Waiting: if (stateTimer <= 0) DecideNextActionInCycle(); break;
            case CatState.Sleeping:
                if (isSleepingEndlessly) break;
                if (stateTimer <= 0) WakeUp();
                break;
            case CatState.Eating: break;
            case CatState.Walking:
                travelTimer -= Time.deltaTime;

                if (dynamicTargetTransform != null)
                {
                    targetPosition = dynamicTargetTransform.position;
                }

                movement.FlipTowards(targetPosition);

                movement.MoveTowards(targetPosition);
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f || travelTimer <= 0)
                {
                    if (targetBowl != null)
                    {
                        HandleReachedBowl();
                    }
                    else
                    {
                        if (travelTimer <= 0) Debug.LogWarning("Кот не смог дойти до цели (Walking). Таймер истек.");
                        HandleActionCompletion();
                    }
                }
                break;
            case CatState.Attacking:
                travelTimer -= Time.deltaTime;
                if (currentMouseTarget == null)
                {
                    HandleActionCompletion(); return;
                }
                targetPosition = currentMouseTarget.transform.position;
                movement.MoveTowards(targetPosition, attackSpeedMultiplier);
                if (Vector3.Distance(transform.position, targetPosition) < 0.2f || travelTimer <= 0)
                {
                    if (travelTimer <= 0) Debug.LogWarning("Кот не смог поймать мышь. Таймер истек.");
                    if (currentMouseTarget != null) Destroy(currentMouseTarget);
                    HandleActionCompletion();
                }
                break;
            case CatState.WalkingToBed:
                travelTimer -= Time.deltaTime;

                if (dynamicTargetTransform != null)
                {
                    targetPosition = dynamicTargetTransform.position;
                }

                movement.FlipTowards(targetPosition);

                movement.MoveTowards(targetPosition);
                if (Vector3.Distance(transform.position, targetPosition) < 0.1f || travelTimer <= 0)
                {
                    if (travelTimer <= 0)
                    {
                        Debug.LogWarning("Кот не смог дойти до кровати. Засыпает на месте.");
                        SleepOnBed(null, false);
                    }
                    else
                    {
                        SleepOnBed(currentBed, false);
                    }
                }
                break;
        }
    }

    public CatState GetCurrentState()
    {
        return currentState;
    }

    public void ResumeFromMenu()
    {
        if (ContextMenuManager.instance != null)
        {
            ContextMenuManager.instance.HideAllMenus();
        }

        ExitMenuState();

        if (currentState != CatState.Eating && targetBowl == null)
        {
            EnterIdleState(false);
            Debug.Log("[CatController] ResumeFromMenu → кот idle после меню");
        }
        else
        {
            Debug.Log("[CatController] ResumeFromMenu → кот продолжает текущее действие");
        }
    }

    public void OnRightClick()
    {

        Debug.Log("4. OnRightClick() в CatController УСПЕШНО ВЫЗВАН!");

        if (isPausedByMenu)
        {
            if (ContextMenuManager.instance != null)
            {
                ContextMenuManager.instance.HideAllMenus();
            }
            ResumeFromMenu();
        }
        else
        {
            isPausedByMenu = true;

            EnterIdleState(false);

            if (ContextMenuManager.instance != null)
            {
                ContextMenuManager.instance.ShowCatMenu(this.transform);
            }
            //catNeeds.ShowHungerBar();
        }
    }

    void StartNewCycle()
    {
        Debug.Log("--- <color=cyan>Начинается новый цикл действий</color> ---");
        walksInCurrentCycle = Random.Range(minWalksInCycle, maxWalksInCycle + 1);
        walksCompleted = 0;
        Debug.Log($"<color=cyan>План:</color> {walksInCurrentCycle} прогулок в этом цикле.");
        DecideNextActionInCycle();
    }

    void DecideNextActionInCycle()
    {
        Debug.Log("--- Решение о действии (начало цикла) ---");
        Debug.Log($"Шанс атаки: {attackChance * 100:F1}%");
        if (Random.value < attackChance && mousePrefab != null)
        {
            Debug.Log("Решение: <color=red>Атаковать!</color>");
            EnterAttackingState();
        }
        else
        {
            Debug.Log("Решение: <color=green>Обычная прогулка.</color>");
            EnterWalkingState();
        }
    }

    void HandleActionCompletion()
    {
        walksCompleted++;
        Debug.Log($"<color=lime>Действие завершено:</color> {walksCompleted}/{walksInCurrentCycle}");
        if (walksCompleted < walksInCurrentCycle)
        {
            EnterWaitingState();
            return;
        }
        Debug.Log("<color=cyan>--- Цикл действий завершен ---</color>");
        if (catNeeds.IsTired)
        {
            Debug.Log($"Кот устал (усталость: {catNeeds.GetCurrentTiredness():F1}). Решение: <color=blue>Искать место для сна.</color>");
            GoToSleep();
        }
        else
        {
            Debug.Log($"Кот не устал (усталость: {catNeeds.GetCurrentTiredness():F1}). Решение: <color=yellow>Отдыхать.</color>");
            EnterIdleState(true);
        }
    }

    void EnterIdleState(bool useLongDuration)
    {
        currentState = CatState.Idle;
        SetAllAnimationsFalse();
        if (useLongDuration)
        {
            stateTimer = idleDuration;
        }
        else
        {
            stateTimer = Random.Range(minWaitAfterMenu, maxWaitAfterMenu);
        }
    }

    void EnterWaitingState()
    {
        currentState = CatState.Waiting;
        SetAllAnimationsFalse();
        stateTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    void EnterWalkingState()
    {
        currentState = CatState.Walking;
        travelTimer = maxTravelTime;
        SetAllAnimationsFalse();
        catAnimation.SetRunning(true);
        catNeeds.IncreaseTiredness(1.0f);

        dynamicTargetTransform = null;

        targetPosition = movement.PickNewTarget();
        movement.FlipTowards(targetPosition);
    }

    void EnterAttackingState()
    {
        currentState = CatState.Attacking;
        travelTimer = maxTravelTime;
        Vector3 spawnPosition = movement.PickNewTarget();
        currentMouseTarget = Instantiate(mousePrefab, spawnPosition, Quaternion.identity);
        catNeeds.IncreaseTiredness(1.5f);
        SetAllAnimationsFalse();
        catAnimation.SetAttacking(true);
        movement.FlipTowards(currentMouseTarget.transform.position);
    }

    private void SetAllAnimationsFalse()
    {
        catAnimation.SetRunning(false);
        catAnimation.SetSleeping(false);
        catAnimation.SetAttacking(false);
    }

    public void ForceCloseMenu()
    {
        ExitMenuState();
    }

    void GoToSleep()
    {
        currentState = CatState.WalkingToBed;
        travelTimer = maxTravelTime;
        DraggableBed nearestBed = FindNearestBed();
        if (nearestBed != null)
        {
            currentBed = nearestBed;

            dynamicTargetTransform = nearestBed.sleepPoint;
            targetPosition = nearestBed.sleepPoint.position;

            movement.FlipTowards(targetPosition);
            SetAllAnimationsFalse();
            catAnimation.SetRunning(true);
            Debug.Log("<color=blue>SLEEPING:</color> Кот нашел кровать, идет к ней.");
        }
        else
        {
            Debug.Log("<color=blue>SLEEPING:</color> Кровать не найдена, засыпает на месте.");
            SleepOnBed(null, false);
        }
    }

    void SleepOnBed(DraggableBed bed, bool indefinite)
    {
        dynamicTargetTransform = null;
        currentState = CatState.Sleeping;
        isSleepingEndlessly = indefinite;
        currentBed = bed;

        if (bed != null)
        {
            transform.position = bed.sleepPoint.position;
            transform.SetParent(bed.transform, true);
            var bedRenderer = bed.GetComponentInChildren<SpriteRenderer>();
            if (bedRenderer != null && catAnimation.GetSpriteRenderer() != null)
                catAnimation.GetSpriteRenderer().sortingOrder = bedRenderer.sortingOrder + 1;
        }

        SetAllAnimationsFalse();
        catAnimation.SetSleeping(true);

        if (!indefinite)
        {
            stateTimer = sleepDuration;
        }

        catNeeds.Rest();
        Debug.Log($"<color=blue>SLEEPING:</color> Засыпает. Бесконечный сон: {indefinite}");
    }

    void WakeUp()
    {
        if (currentBed != null)
        {
            transform.SetParent(null);
            if (catAnimation.GetSpriteRenderer() != null)
                catAnimation.GetSpriteRenderer().sortingOrder = 1;
            currentBed = null;
        }

        isSleepingEndlessly = false;

        Debug.Log("<color=green>WAKE UP:</color> Проснулся. Начинаем новый цикл.");
        EnterIdleState(true);
    }

    private DraggableBed FindNearestBed()
    {
        if (AllBeds.Count == 0) return null;
        DraggableBed nearestBed = null;
        float minDistance = float.MaxValue;
        foreach (var bed in AllBeds)
        {
            float distance = Vector3.Distance(transform.position, bed.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestBed = bed;
            }
        }
        return nearestBed;
    }

    public bool IsBrainPaused()
    {
        return isBrainPaused;
    }

    private void ExitMenuState()
    {
        if (catAnimation != null)
        {
            catAnimation.SetAnimatorSpeed(1f);
        }

        isPausedByMenu = false;
        ResumeBrain();

        if (currentState != CatState.Eating && targetBowl == null)
        {
            catNeeds.HideHungerBar();
            Debug.Log("[CatController] ExitMenuState → бар скрыт (кот не ест и не идёт к миске)");
        }
        else
        {
            Debug.Log("[CatController] ExitMenuState → бар оставлен (кот ест или идёт к миске)");
        }

        Debug.Log("[CatController] Выход из состояния меню.");
    }

    public bool CanBeFed()
    {
        return catNeeds.CanBeFed();
    }

    public void StartEating()
    {
        Debug.Log("[CatController] StartEating() → кот начал есть");

        if (catAnimation != null)
        {
            catAnimation.SetAnimatorSpeed(1f);
        }

        currentState = CatState.Eating;

        SetAllAnimationsFalse();
        catAnimation.SetEating(true);

        catNeeds.ShowHungerBar();

    }

    public void UpdateHunger(float newHungerValue)
    {
        float current = catNeeds.GetCurrentHunger();
        catNeeds.Feed(newHungerValue - current);
    }

    public float GetCurrentHunger() => catNeeds.GetCurrentHunger();

    public float GetMaxHunger() => catNeeds.GetMaxHunger();

    public float GetCurrentTiredness()
    {
        return catNeeds.GetCurrentTiredness();
    }

    public void FinishEating()
    {
        catAnimation.SetEating(false);

        EnterIdleState(false);

        if (!isPausedByMenu)
        {
            catNeeds.HideHungerBar();
        }
    }

    public void PauseBrain()
    {
        isBrainPaused = true;
    }

    public void ResumeBrain()
    {
        isBrainPaused = false;
    }

    public void InterruptActionForDrag()
    {
        if (currentState == CatState.Sleeping)
        {
            WakeUp();
        }
        if (currentMouseTarget != null)
        {
            Destroy(currentMouseTarget);
        }

        SetAllAnimationsFalse();
    }

    public void ConcludeInterruption()
    {
        Debug.Log("[CatController] Прерывание (мини-игра) завершено. Переход в короткий Idle.");
        EnterIdleState(false);
    }

    public void HandleDrop(Collider2D droppedCollider)
    {

        foreach (var bed in AllBeds)
        {
            if (bed != null && bed.GetComponent<Collider2D>().bounds.Intersects(droppedCollider.bounds))
            {
                Debug.Log("<color=purple>DRAG & DROP:</color> Кот брошен в кровать. Засыпает.");

                SleepOnBed(bed, true);
                return;
            }
        }

        Debug.Log("<color=purple>DRAG & DROP:</color> Кот брошен, но не на кровать.");
        EnterIdleState(true);
    }

    public bool IsMenuOpen()
    {
        return isPausedByMenu;
    }

    private void LoadAndApplyName()
    {
        string savedName = PlayerPrefs.GetString(CatNameKey, "Name");
        UpdateDisplayName(savedName);
    }

    public void UpdateDisplayName(string newName)
    {
        if (nameTag != null)
        {
            nameTag.UpdateName(newName);
        }
        else
        {
            Debug.LogError("Ошибка! На объекте 'CatController' не назначена ссылка 'Name Tag' в инспекторе!");
        }
    }

    public void ToggleRgbEffect(bool isEnabled)
    {
        if (spriteRenderer == null) return; 

        if (isEnabled)
        {
            if (rgbCoroutine == null)
            {
                rgbCoroutine = StartCoroutine(RgbEffect());
            }
        }
        else
        {
            if (rgbCoroutine != null)
            {
                StopCoroutine(rgbCoroutine);
                rgbCoroutine = null;
                spriteRenderer.color = Color.white;
            }
        }
    }

    public bool IsOnTheWayToEat()
    {
        return targetBowl != null;
    }

    private IEnumerator RgbEffect()
    {
        float hue = 0f;
        while (true)
        {
            spriteRenderer.color = Color.HSVToRGB(hue, 1f, 1f);
            hue += Time.deltaTime * rgbSpeed;
            if (hue >= 1f) hue -= 1f;
            yield return null; 
        }
    }

    private void InitializeRgbState()
    {
        int savedValue = PlayerPrefs.GetInt(RgbSaveKey, 0);
        bool isEnabled = (savedValue == 1);

        ToggleRgbEffect(isEnabled);
    }

    private Bowl targetBowl;

    public void GoEatFromBowl(Bowl bowl)
    {
        if (bowl == null || !CanBeFed())
        {
            Debug.Log("[CatController] GoEatFromBowl() → bowl null или кот не голоден");
            return;
        }

        targetBowl = bowl;
        Debug.Log("[CatController] GoEatFromBowl() → кот отправлен к миске");

        catNeeds.ShowHungerBar();

        Transform sitPoint = bowl.GetEatingPos(this);
        if (sitPoint == null)
        {
            Debug.LogError("[CatController] GoEatFromBowl() → не удалось найти точку для еды!");
            return;
        }

        currentState = CatState.Walking;
        travelTimer = maxTravelTime;
        targetPosition = sitPoint.position;

        dynamicTargetTransform = sitPoint;
        targetPosition = sitPoint.position;

        SetAllAnimationsFalse();
        catAnimation.SetRunning(true);
        movement.FlipTowards(bowl.transform.position);

        Debug.Log("[CatController] GoEatFromBowl() → состояние Walking, кот бежит");
    }


    private void HandleReachedBowl()
    {
        if (targetBowl != null)
        {

            dynamicTargetTransform = null;

            transform.position = targetPosition;

            movement.FlipTowards(targetBowl.transform.position);

            StartCoroutine(EatAtBowlCoroutine(targetBowl));
            targetBowl = null;
        }
    }

    private IEnumerator EatAtBowlCoroutine(Bowl bowl)
    {
        bowl.StartUsing();

        try
        {
            PauseBrain();
            StartEating();

            while (catNeeds.CanBeFed())
            {
                catNeeds.Feed(feedAmountPerSecond * Time.deltaTime);

                yield return null;
            }
        }
        finally
        {
            FinishEating();
            ResumeBrain();
            bowl.StopUsing();
        }
    }
}