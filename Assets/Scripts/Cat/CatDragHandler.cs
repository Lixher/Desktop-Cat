using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CatController))]
public class CatDragHandler : MonoBehaviour
{
    public event Action OnDragStart;
    public event Action<Vector3> OnDragEnd;

    public bool IsDragging { get; private set; }

    private Camera mainCamera;
    private Vector3 offset;
    private Vector3 lastMousePosition;
    private SpriteRenderer spriteRenderer;
    private CatController catController;

    void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        catController = GetComponent<CatController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null && hit.transform == this.transform)
            {
                CatController.CatState currentState = catController.GetCurrentState();

                Debug.Log($"[Drag Attempt] Попытка перетащить кота. Его текущее состояние: {currentState}");

                if (currentState == CatController.CatState.Eating || currentState == CatController.CatState.Sleeping)
                {
                    Debug.Log("[Drag Result] Перетаскивание ЗАБЛОКИРОВАНО, потому что кот ест или спит.");
                    return;
                }

                Debug.Log("[Drag Result] Перетаскивание РАЗРЕШЕНО.");
                IsDragging = true;
                offset = transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition);
                lastMousePosition = Input.mousePosition;
                OnDragStart?.Invoke();
            }
        }

        if (IsDragging && Input.GetMouseButton(0))
        {
            CatController.CatState currentState = catController.GetCurrentState();
            if (currentState == CatController.CatState.Eating || currentState == CatController.CatState.Sleeping)
            {
                Debug.Log("Кот начал есть/спать во время перетаскивания! Отпускаем.");
                IsDragging = false;
                OnDragEnd?.Invoke(transform.position);
                return;
            }

            Vector3 currentMousePosition = Input.mousePosition;
            if (currentMousePosition.x > lastMousePosition.x) spriteRenderer.flipX = false;
            else if (currentMousePosition.x < lastMousePosition.x) spriteRenderer.flipX = true;
            lastMousePosition = currentMousePosition;

            Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, transform.position.z);
        }

        if (IsDragging && Input.GetMouseButtonUp(0))
        {
            IsDragging = false;
            OnDragEnd?.Invoke(transform.position);
        }
    }
}