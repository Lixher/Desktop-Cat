using UnityEngine;
using Debug = UnityEngine.Debug;

public class Bowl : MonoBehaviour
{
    [Header("Параметры еды")]
    [SerializeField] private float feedAmount = 50f;
    [SerializeField] private float eatingDuration = 3f;

    [Header("Точки для кота")]
    [SerializeField] private Transform leftEatingPos;
    [SerializeField] private Transform rightEatingPos;

    private Transform cachedSitPoint;

    public bool IsInUse { get; private set; } = false;

    public void StartUsing()
    {
        IsInUse = true;
        Debug.Log($"Миска '{gameObject.name}' теперь используется.");
    }

    public void StopUsing()
    {
        IsInUse = false;
        Debug.Log($"Миска '{gameObject.name}' теперь свободна.");
    }


    public Transform GetEatingPos(CatController catController)
    {
        Transform chosenPoint;
        if (catController.transform.position.x < transform.position.x)
        {
            chosenPoint = leftEatingPos;
        }
        else
        {
            chosenPoint = rightEatingPos;
        }

        if (chosenPoint == null)
        {
            Debug.LogWarning("Точка для еды (left/right) не назначена на миске! Кот пойдет к центру миски.", this);
            return this.transform;
        }
        return chosenPoint;
    }

    public float GetFeedAmount() => feedAmount;
    public float GetEatingDuration() => eatingDuration;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (leftEatingPos != null) Gizmos.DrawSphere(leftEatingPos.position, 0.1f);
        if (rightEatingPos != null) Gizmos.DrawSphere(rightEatingPos.position, 0.1f);
    }
#endif
}