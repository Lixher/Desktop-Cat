using UnityEngine;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(CatController))]
[RequireComponent(typeof(PettingManager))]
[RequireComponent(typeof(SpriteRenderer))]
public class CatInteractionHandler : MonoBehaviour
{
    // ... (все ваши переменные остаются без изменений) ...
    private CatController catController;
    private PettingManager pettingManager;
    private SpriteRenderer spriteRenderer;
    public PettingModeController pettingModeController;
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector3 dragOffset;
    private Vector3 lastMousePosition;

    void Awake()
    {
        catController = GetComponent<CatController>();
        pettingManager = GetComponent<PettingManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (pettingManager.isPettingModeActive)
            {
                return;
            }

            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.transform == this.transform)
            {
                CatController.CatState currentState = catController.GetCurrentState();
                bool isEating = currentState == CatController.CatState.Eating;
                bool isSleeping = currentState == CatController.CatState.Sleeping;
                bool isGoingToEat = catController.IsOnTheWayToEat();

                if (isEating || isSleeping || isGoingToEat)
                {
                    Debug.Log($"Перетаскивание ЗАБЛОКИРОВАНО. Состояние: {currentState}, Идет к миске: {isGoingToEat}");
                    return;
                }

                HandleDragStart();
            }
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            HandleDragging();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                HandleDragEnd();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.transform == this.transform)
            {
                if (pettingManager.isPettingModeActive)
                {
                    if (pettingModeController != null)
                    {
                        pettingModeController.EndPettingMode();
                    }
                    else
                    {
                        Debug.LogError("Ошибка: Ссылка на PettingModeController не назначена в инспекторе на объекте кота!");
                    }
                }
                else
                {
                    catController.OnRightClick();
                }
            }
        }
    }

    private void HandleDragStart()
    {
        isDragging = true;
        //catController.PauseBrain(); // Мозг лучше ставить на паузу в HandleDragging, чтобы кот не "дергался" в первом кадре
        catController.InterruptActionForDrag();
        dragOffset = transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition);
        lastMousePosition = Input.mousePosition;
    }

    private void HandleDragging()
    {
        CatController.CatState currentState = catController.GetCurrentState();
        if (currentState == CatController.CatState.Eating || currentState == CatController.CatState.Sleeping || catController.IsOnTheWayToEat())
        {
            Debug.Log("Движение прервано, так как кот начал важное дело!");
            HandleDragEnd(); 
            return;
        }

        catController.PauseBrain(); // Ставим мозг на паузу здесь

        Vector3 currentMousePosition = Input.mousePosition;
        if (currentMousePosition.x > lastMousePosition.x)
        {
            spriteRenderer.flipX = false;
        }
        else if (currentMousePosition.x < lastMousePosition.x)
        {
            spriteRenderer.flipX = true;
        }
        lastMousePosition = currentMousePosition;

        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePos.x + dragOffset.x, mousePos.y + dragOffset.y, transform.position.z);
    }

    private void HandleDragEnd()
    {
        isDragging = false;
        catController.ResumeBrain();
        catController.HandleDrop(GetComponent<Collider2D>());
    }
}