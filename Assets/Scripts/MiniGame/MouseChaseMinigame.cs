using System.Collections;
using System.Collections.Generic; 
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(CatController))]
[RequireComponent(typeof(CatMovement))]
[RequireComponent(typeof(CatAnimation))]
public class MouseChaseMinigame : MonoBehaviour
{
    public enum ChaseMode
    {
        FixedSpeed,
        Accelerating,
        SmartHunter
    }

    [Header("Общие настройки мини-игры")]
    [SerializeField] private ChaseMode chaseMode = ChaseMode.SmartHunter;
    [SerializeField] private float catchDistance = 0.5f;
    [SerializeField] private float chaseSpeedMultiplier = 1.8f;
    [Tooltip("Как далеко в будущее кот пытается предсказать движение мыши (в секундах).")]
    [SerializeField] private float predictionTime = 0.2f;
    [Tooltip("Насколько плавно кот меняет свою цель. Меньше значение = более плавные, 'ленивые' повороты. Больше значение = более резкие и отзывчивые. Хорошее значение: 5-15.")]
    [SerializeField] private float targetSmoothing = 8f;
    [SerializeField] private float celebrationDuration = 2.0f;
    [SerializeField] private float cooldownDuration = 10.0f;

    [Header("Настройки режима с ускорением")]
    [Tooltip("Насколько увеличивается множитель скорости каждую секунду в режиме 'Accelerating'.")]
    [SerializeField] private float accelerationRate = 0.1f;
    [Tooltip("Максимальный множитель скорости, до которого кот может разогнаться в режиме 'Accelerating'.")]
    [SerializeField] private float maxChaseSpeedMultiplier = 3.0f;

    [Header("Настройки режима 'Умный Охотник'")]
    [Tooltip("Максимальный запас выносливости кота.")]
    [SerializeField] private float maxStamina = 100f;
    [Tooltip("Сколько выносливости тратится в секунду при быстрой погоне.")]
    [SerializeField] private float staminaDrainRate = 25f;
    [Tooltip("Сколько выносливости восстанавливается в секунду, когда кот не гонится активно.")]
    [SerializeField] private float staminaRegenRate = 15f;
    [Tooltip("Порог выносливости, ниже которого кот считается уставшим и замедляется.")]
    [SerializeField] private float tiredThreshold = 20f;
    [Tooltip("Множитель скорости, когда кот устал (например, 0.8, чтобы он стал медленнее).")]
    [SerializeField] private float tiredSpeedMultiplier = 0.8f;
    [Tooltip("Через сколько секунд безуспешной погони кот сдастся и прекратит игру.")]
    [SerializeField] private float frustrationTime = 8.0f;

    [Header("Ссылки на UI")]
    [SerializeField] private Button chaseButton;
    [SerializeField] private GameObject catMenuObject;

    private CatController catController;
    private CatMovement catMovement;
    private CatAnimation catAnimation;
    private Camera mainCamera;

    private bool isGameActive = false;
    private bool isOnCooldown = false;
    private bool canBeCaught = false;

    private float chaseTimer;
    private float currentStamina;
    private float frustrationTimer;
    private Vector3 lastMousePosition;
    private Vector3 mouseVelocity;
    private Vector3 smoothedTargetPosition; 

    void Awake()
    {
        catController = GetComponent<CatController>();
        catMovement = GetComponent<CatMovement>();
        catAnimation = GetComponent<CatAnimation>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (!isGameActive)
        {
            return;
        }

        chaseTimer += Time.deltaTime;
        Vector3 mousePosition = GetMouseWorldPosition();

        if (Time.deltaTime > 0)
        {
            mouseVelocity = (mousePosition - lastMousePosition) / Time.deltaTime;
        }
        lastMousePosition = mousePosition;

        Vector3 predictedPosition = mousePosition + (mouseVelocity * predictionTime);

        smoothedTargetPosition = Vector3.Lerp(smoothedTargetPosition, predictedPosition, targetSmoothing * Time.deltaTime);

        switch (chaseMode)
        {
            case ChaseMode.FixedSpeed:
                HandleFixedSpeedChase(smoothedTargetPosition);
                break;
            case ChaseMode.Accelerating:
                HandleAcceleratingChase(smoothedTargetPosition);
                break;
            case ChaseMode.SmartHunter:
                HandleSmartHunterChase(smoothedTargetPosition, mousePosition);
                break;
        }

        catAnimation.SetRunning(true);

        catMovement.FlipTowards(mousePosition);

        if (canBeCaught && Vector3.Distance(transform.position, mousePosition) < catchDistance)
        {
            EndMinigame(true);
        }

    catMovement.FlipTowards(mousePosition);

        if (canBeCaught && Vector3.Distance(transform.position, mousePosition) < catchDistance)
        {
            EndMinigame(true);
        }
    }

    private void HandleFixedSpeedChase(Vector3 targetPosition)
    {
        catMovement.MoveTowards(targetPosition, chaseSpeedMultiplier);
    }

    private void HandleAcceleratingChase(Vector3 targetPosition)
    {
        float acceleratedSpeed = chaseSpeedMultiplier + (chaseTimer * accelerationRate);
        float currentSpeedMultiplier = Mathf.Min(acceleratedSpeed, maxChaseSpeedMultiplier);
        catMovement.MoveTowards(targetPosition, currentSpeedMultiplier);
    }

    private void HandleSmartHunterChase(Vector3 predictedPosition, Vector3 currentMousePosition)
    {
        if (mouseVelocity.magnitude > 1.0f)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        float currentSpeedMultiplier;
        if (currentStamina < tiredThreshold)
        {
            currentSpeedMultiplier = tiredSpeedMultiplier;
            catAnimation.SetTired(true);
        }
        else
        {
            currentSpeedMultiplier = chaseSpeedMultiplier;
            catAnimation.SetTired(false);
        }

        if (Vector3.Distance(transform.position, currentMousePosition) > catchDistance * 3)
        {
            frustrationTimer += Time.deltaTime;
        }
        else
        {
            frustrationTimer = 0;
        }

        if (frustrationTimer > frustrationTime)
        {
            Debug.Log("<color=yellow>Кот устал гоняться и потерял интерес.</color>");
            EndMinigame(false);
            return;
        }

        catMovement.MoveTowards(predictedPosition, currentSpeedMultiplier);
    }

    public void StartMinigame()
    {
        if (isGameActive || isOnCooldown)
        {
            Debug.Log("Нельзя запустить мини-игру: она уже активна или на перезарядке.");
            return;
        }

        chaseTimer = 0f;
        frustrationTimer = 0f;
        currentStamina = maxStamina;
        lastMousePosition = GetMouseWorldPosition();
        mouseVelocity = Vector3.zero;

        smoothedTargetPosition = transform.position;

        if (catMenuObject != null)
        {
            catMenuObject.SetActive(false);
        }

        Debug.Log("<color=orange>--- НАЧАЛО МИНИ-ИГРЫ: ПОГОНЯ ЗА МЫШКОЙ ---</color>");
        isGameActive = true;
        canBeCaught = false;

        catController.PauseBrain();
        catController.InterruptActionForDrag();

        catAnimation.SetRunning(true);
        catAnimation.SetTired(false);

        StartCoroutine(GracePeriodCoroutine());
    }

    private void EndMinigame(bool wasCaught)
    {
        if (!isGameActive) return;

        isGameActive = false;

        if (catMenuObject != null)
        {
            catMenuObject.SetActive(true);
        }

        Debug.Log("<color=orange>--- КОНЕЦ МИНИ-ИГРЫ ---</color>");

        catAnimation.SetRunning(false);
        catAnimation.SetTired(false);

        if (wasCaught)
        {
            Debug.Log("<color=green>Мышка поймана! Кот доволен.</color>");
        }

        StartCoroutine(CelebrationAndCooldownCoroutine(wasCaught));
    }

    private IEnumerator GracePeriodCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        canBeCaught = true;
        Debug.Log("Фора закончилась, теперь мышку можно поймать!");
    }

    private IEnumerator CelebrationAndCooldownCoroutine(bool shouldCelebrate)
    {
        if (shouldCelebrate)
        {
            yield return new WaitForSeconds(celebrationDuration);
        }

        if (catController.IsBrainPaused())
        {
            Debug.Log("Мини-игра завершает паузу кота.");
            catController.ResumeBrain();
            catController.ConcludeInterruption();
        }
        else
        {
            Debug.Log("Кот уже был разбужен игроком. Пропускаем этот шаг.");
        }

        isOnCooldown = true;
        if (chaseButton != null)
        {
            chaseButton.interactable = false;
        }

        yield return new WaitForSeconds(cooldownDuration);

        isOnCooldown = false;
        if (chaseButton != null)
        {
            chaseButton.interactable = true;
        }
        Debug.Log("Мини-игра снова доступна.");
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mainCamera.WorldToScreenPoint(transform.position).z;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    public void InterruptMinigame()
    {
        if (isGameActive)
        {
            Debug.LogWarning("Мини-игра прервана.");
            EndMinigame(false);
        }
    }
}