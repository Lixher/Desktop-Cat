using System.Diagnostics;
using Debug = UnityEngine.Debug;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class CatAnimation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    private static readonly int IsSleepingHash = Animator.StringToHash("IsSleeping");
    private static readonly int IsAttackingHash = Animator.StringToHash("IsAttacking");
    private static readonly int IsEatingHash = Animator.StringToHash("IsEating");

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (animator == null)
            Debug.LogError("�� ������� ����������� ��������� Animator!", this);

        if (spriteRenderer == null)
            Debug.LogError("�� ������� ����������� ��������� SpriteRenderer!", this);
    }

    public void Initialize()
    {
        // TODO: �������� ������������� ���� ���� ����))
    }

    private void SetBool(int parameterHash, bool value)
    {
        if (animator != null)
            animator.SetBool(parameterHash, value);
    }

    public void SetRunning(bool isRunning) => SetBool(IsRunningHash, isRunning);

    public void SetSleeping(bool isSleeping) => SetBool(IsSleepingHash, isSleeping);

    public void SetAttacking(bool isAttacking) => SetBool(IsAttackingHash, isAttacking);

    public void SetEating(bool isEating) => SetBool(IsEatingHash, isEating);

    public void SetTired(bool isTired)
    {
        // TODO: �������� �������� ��������� ���� ���� ����)))
    }

    /// <summary>
    /// ������������� ��������� �������� (����� ����� ���������� ����� ����������).
    /// </summary>
    public void ForceUpdate()
    {
        if (animator != null)
            animator.Update(0f);
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    public void SetAnimatorSpeed(float speed)
    {
        if (animator != null)
            animator.speed = speed;
    }
}
