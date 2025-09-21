using System;
using System.Diagnostics;
using UnityEngine;
using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

public class CatMovement : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private float speed = 3.0f;

    [Header("Физика движения (инерция)")]
    [Tooltip("Сколько секунд требуется, чтобы разогнаться до полной скорости")]
    [SerializeField] private float accelerationTime = 0.2f;

    [Tooltip("Кривая, по которой кот будет замедляться до полной остановки. Ось X - время (0-1), ось Y - множитель скорости (от 1 до 0).")]
    [SerializeField] private AnimationCurve decelerationCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Tooltip("За сколько секунд кот должен полностью остановиться (длительность кривой замедления)")]
    [SerializeField] private float decelerationDuration = 0.4f;

    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 velocitySmoothRef = Vector3.zero;

    private bool isStopping = false;
    private float stoppingTimer = 0f;
    private Vector3 velocityOnStop;

    [Header("Система 'Памяти' и навигация")]
    [SerializeField] private Vector2Int gridSize = new Vector2Int(5, 5);
    [Range(0f, 0.5f)][SerializeField] private float viewportPadding = 0.1f;
    [SerializeField] private float memoryBias = 2.0f;

    [Header("Зависимости")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private SpriteRenderer catSpriteRenderer;

    [Header("Отладка")]
    [SerializeField] private bool showDebugGizmos = true;

    private const float Z_PLANE_OFFSET = 10f;
    private float[,] gridVisitTimestamps;
    private Vector2Int currentGridCell = new Vector2Int(-1, -1);
    private float[,] gridDesirability;

    void Awake()
    {
        if (mainCamera == null)
        {
            Debug.LogError("Камера не назначена в инспекторе!", this);
            enabled = false;
            return;
        }
        if (catSpriteRenderer == null)
        {
            catSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (catSpriteRenderer == null)
            {
                Debug.LogError("Sprite Renderer не найден ни на объекте, ни в дочерних объектах!", this);
                enabled = false;
                return;
            }
        }

        InitializeGrid();
    }

    void Update()
    {
        UpdateCurrentCell();
    }

    public void MoveTowards(Vector3 targetPosition, float speedMultiplier = 1.0f)
    {
        float targetSpeed = speed * speedMultiplier;
        Vector3 toTarget = targetPosition - transform.position;

        bool shouldBeStopping = toTarget.magnitude < 0.1f;

        if (shouldBeStopping)
        {
            if (!isStopping && currentVelocity.magnitude > 0.1f)
            {
                isStopping = true;
                stoppingTimer = 0f;
                velocityOnStop = currentVelocity; 
            }
        }
        else
        {
            isStopping = false;
        }


        if (isStopping)
        {
            stoppingTimer += Time.deltaTime;
            float timeRatio = Mathf.Clamp01(stoppingTimer / decelerationDuration);

            float speedFactor = decelerationCurve.Evaluate(timeRatio);

            currentVelocity = velocityOnStop * speedFactor;

            if (timeRatio >= 1.0f)
            {
                currentVelocity = Vector3.zero;
                isStopping = false;
            }
        }
        else
        {
            Vector3 desiredDirection = toTarget.normalized;

            float wiggleAmount = 0.5f;
            float wiggleSpeed = 3f;
            Vector3 perpendicular = Vector3.Cross(desiredDirection, Vector3.forward).normalized;
            desiredDirection += perpendicular * Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmount;
            desiredDirection.Normalize();

            Vector3 desiredVelocity = desiredDirection * targetSpeed;

            currentVelocity = Vector3.SmoothDamp(
                currentVelocity,
                desiredVelocity,
                ref velocitySmoothRef,
                accelerationTime
            );
        }

        transform.position += currentVelocity * Time.deltaTime;
    }

    public Vector3 PickNewTarget()
    {
        Vector2Int targetCell = ChooseTargetCell();

        float randomX = Random.Range(0f, 1f);
        float randomY = Random.Range(0f, 1f);

        float viewportX = (targetCell.x + randomX) / gridSize.x;
        float viewportY = (targetCell.y + randomY) / gridSize.y;

        viewportX = Mathf.Lerp(viewportPadding, 1 - viewportPadding, viewportX);
        viewportY = Mathf.Lerp(viewportPadding, 1 - viewportPadding, viewportY);

        Vector3 newTarget = mainCamera.ViewportToWorldPoint(new Vector3(viewportX, viewportY, Z_PLANE_OFFSET));
        newTarget.z = transform.position.z;

        return newTarget;
    }

    public void FlipTowards(Vector3 targetPosition)
    {
        if (targetPosition.x > transform.position.x)
            catSpriteRenderer.flipX = false;
        else if (targetPosition.x < transform.position.x)
            catSpriteRenderer.flipX = true;
    }

    private void InitializeGrid()
    {
        gridVisitTimestamps = new float[gridSize.x, gridSize.y];
        gridDesirability = new float[gridSize.x, gridSize.y];
    }

    private void UpdateCurrentCell()
    {
        Vector2Int newCell = GetCellFromWorldPosition(transform.position);
        if (newCell != currentGridCell && IsCellValid(newCell))
        {
            currentGridCell = newCell;
            gridVisitTimestamps[currentGridCell.x, currentGridCell.y] = Time.time;
        }
    }

    private Vector2Int ChooseTargetCell()
    {
        float totalWeight = 0;
        float[] weights = new float[gridSize.x * gridSize.y];
        int i = 0;

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                float staleness = Time.time - gridVisitTimestamps[x, y];
                float weight = Mathf.Pow(staleness, memoryBias);

                if (x == currentGridCell.x && y == currentGridCell.y)
                    weight = 0;

                weights[i] = weight;
                totalWeight += weight;
                i++;
            }
        }

        if (totalWeight <= 0)
        {
            Vector2Int randomCell;
            do
            {
                randomCell = new Vector2Int(Random.Range(0, gridSize.x), Random.Range(0, gridSize.y));
            } while (randomCell == currentGridCell && (gridSize.x * gridSize.y > 1));
            return randomCell;
        }

        float randomValue = Random.Range(0, totalWeight);
        i = 0;
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (randomValue < weights[i])
                    return new Vector2Int(x, y);
                randomValue -= weights[i];
                i++;
            }
        }

        return new Vector2Int(Random.Range(0, gridSize.x), Random.Range(0, gridSize.y));
    }

    private Vector2Int GetCellFromWorldPosition(Vector3 worldPos)
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(worldPos);

        float normalizedX = (viewportPos.x - viewportPadding) / (1 - viewportPadding * 2);
        float normalizedY = (viewportPos.y - viewportPadding) / (1 - viewportPadding * 2);

        int x = Mathf.FloorToInt(normalizedX * gridSize.x);
        int y = Mathf.FloorToInt(normalizedY * gridSize.y);

        return new Vector2Int(x, y);
    }

    private bool IsCellValid(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < gridSize.x && cell.y >= 0 && cell.y < gridSize.y;
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos || !UnityEngine.Application.isPlaying || mainCamera == null) return;

        for (int i = 0; i <= gridSize.x; i++)
        {
            float x = Mathf.Lerp(viewportPadding, 1 - viewportPadding, (float)i / gridSize.x);
            Vector3 start = mainCamera.ViewportToWorldPoint(new Vector3(x, viewportPadding, Z_PLANE_OFFSET));
            Vector3 end = mainCamera.ViewportToWorldPoint(new Vector3(x, 1 - viewportPadding, Z_PLANE_OFFSET));
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(start, end);
        }
        for (int i = 0; i <= gridSize.y; i++)
        {
            float y = Mathf.Lerp(viewportPadding, 1 - viewportPadding, (float)i / gridSize.y);
            Vector3 start = mainCamera.ViewportToWorldPoint(new Vector3(viewportPadding, y, Z_PLANE_OFFSET));
            Vector3 end = mainCamera.ViewportToWorldPoint(new Vector3(1 - viewportPadding, y, Z_PLANE_OFFSET));
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(start, end);
        }

        float maxStaleness = 0f;
        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                float staleness = Time.time - gridVisitTimestamps[x, y];
                if (staleness > maxStaleness)
                {
                    maxStaleness = staleness;
                }
            }
        }

        float cellWidthNorm = (1f / gridSize.x) * (1 - viewportPadding * 2);
        float cellHeightNorm = (1f / gridSize.y) * (1 - viewportPadding * 2);

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                float staleness = Time.time - gridVisitTimestamps[x, y];
                float normalizedStaleness = (maxStaleness > 0) ? (staleness / maxStaleness) : 0;

                Color cellColor = Color.Lerp(Color.red, Color.green, normalizedStaleness);
                cellColor.a = 0.3f;
                Gizmos.color = cellColor;

                float xNorm = viewportPadding + x * cellWidthNorm;
                float yNorm = viewportPadding + y * cellHeightNorm;

                Vector3 bl = mainCamera.ViewportToWorldPoint(new Vector3(xNorm, yNorm, Z_PLANE_OFFSET));
                Vector3 tr = mainCamera.ViewportToWorldPoint(new Vector3(xNorm + cellWidthNorm, yNorm + cellHeightNorm, Z_PLANE_OFFSET));

                Vector3 center = (bl + tr) / 2;
                Vector3 size = new Vector3(tr.x - bl.x, tr.y - bl.y, 0.01f);

                Gizmos.DrawCube(center, size);
            }
        }

        if (IsCellValid(currentGridCell))
        {
            float xNorm = viewportPadding + currentGridCell.x * cellWidthNorm;
            float yNorm = viewportPadding + currentGridCell.y * cellHeightNorm;

            Vector3 bl = mainCamera.ViewportToWorldPoint(new Vector3(xNorm, yNorm, Z_PLANE_OFFSET));
            Vector3 br = mainCamera.ViewportToWorldPoint(new Vector3(xNorm + cellWidthNorm, yNorm, Z_PLANE_OFFSET));
            Vector3 tl = mainCamera.ViewportToWorldPoint(new Vector3(xNorm, yNorm + cellHeightNorm, Z_PLANE_OFFSET));
            Vector3 tr = mainCamera.ViewportToWorldPoint(new Vector3(xNorm + cellWidthNorm, yNorm + cellHeightNorm, Z_PLANE_OFFSET));

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(bl, br);
            Gizmos.DrawLine(br, tr);
            Gizmos.DrawLine(tr, tl);
            Gizmos.DrawLine(tl, bl);
        }
    }
}