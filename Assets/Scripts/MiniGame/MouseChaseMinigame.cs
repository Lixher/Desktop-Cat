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

    [Header("����� ��������� ����-����")]
    [SerializeField] private ChaseMode chaseMode = ChaseMode.SmartHunter;
    [SerializeField] private float catchDistance = 0.5f;
    [SerializeField] private float chaseSpeedMultiplier = 1.8f;
    [Tooltip("��� ������ � ������� ��� �������� ����������� �������� ���� (� ��������).")]
    [SerializeField] private float predictionTime = 0.2f;
    [Tooltip("��������� ������ ��� ������ ���� ����. ������ �������� = ����� �������, '�������' ��������. ������ �������� = ����� ������ � ����������. ������� ��������: 5-15.")]
    [SerializeField] private float targetSmoothing = 8f;
    [SerializeField] private float celebrationDuration = 2.0f;
    [SerializeField] private float cooldownDuration = 10.0f;

    [Header("��������� ������ � ����������")]
    [Tooltip("��������� ������������� ��������� �������� ������ ������� � ������ 'Accelerating'.")]
    [SerializeField] private float accelerationRate = 0.1f;
    [Tooltip("������������ ��������� ��������, �� �������� ��� ����� ����������� � ������ 'Accelerating'.")]
    [SerializeField] private float maxChaseSpeedMultiplier = 3.0f;

    [Header("��������� ������ '����� �������'")]
    [Tooltip("������������ ����� ������������ ����.")]
    [SerializeField] private float maxStamina = 100f;
    [Tooltip("������� ������������ �������� � ������� ��� ������� ������.")]
    [SerializeField] private float staminaDrainRate = 25f;
    [Tooltip("������� ������������ ����������������� � �������, ����� ��� �� ������� �������.")]
    [SerializeField] private float staminaRegenRate = 15f;
    [Tooltip("����� ������������, ���� �������� ��� ��������� �������� � �����������.")]
    [SerializeField] private float tiredThreshold = 20f;
    [Tooltip("��������� ��������, ����� ��� ����� (��������, 0.8, ����� �� ���� ���������).")]
    [SerializeField] private float tiredSpeedMultiplier = 0.8f;
    [Tooltip("����� ������� ������ ����������� ������ ��� ������� � ��������� ����.")]
    [SerializeField] private float frustrationTime = 8.0f;

    [Header("������ �� UI")]
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
            Debug.Log("<color=yellow>��� ����� �������� � ������� �������.</color>");
            EndMinigame(false);
            return;
        }

        catMovement.MoveTowards(predictedPosition, currentSpeedMultiplier);
    }

    public void StartMinigame()
    {
        if (isGameActive || isOnCooldown)
        {
            Debug.Log("������ ��������� ����-����: ��� ��� ������� ��� �� �����������.");
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

        Debug.Log("<color=orange>--- ������ ����-����: ������ �� ������ ---</color>");
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

        Debug.Log("<color=orange>--- ����� ����-���� ---</color>");

        catAnimation.SetRunning(false);
        catAnimation.SetTired(false);

        if (wasCaught)
        {
            Debug.Log("<color=green>����� �������! ��� �������.</color>");
        }

        StartCoroutine(CelebrationAndCooldownCoroutine(wasCaught));
    }

    private IEnumerator GracePeriodCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        canBeCaught = true;
        Debug.Log("���� �����������, ������ ����� ����� �������!");
    }

    private IEnumerator CelebrationAndCooldownCoroutine(bool shouldCelebrate)
    {
        if (shouldCelebrate)
        {
            yield return new WaitForSeconds(celebrationDuration);
        }

        if (catController.IsBrainPaused())
        {
            Debug.Log("����-���� ��������� ����� ����.");
            catController.ResumeBrain();
            catController.ConcludeInterruption();
        }
        else
        {
            Debug.Log("��� ��� ��� �������� �������. ���������� ���� ���.");
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
        Debug.Log("����-���� ����� ��������.");
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
            Debug.LogWarning("����-���� ��������.");
            EndMinigame(false);
        }
    }
}