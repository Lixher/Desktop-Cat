using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class CatMeow : MonoBehaviour
{
    [Header("�����")]
    [Tooltip("������ ����������� � ���������.")]
    public AudioClip[] meowSounds;

    [Header("��������� ��������")]
    [Tooltip("�������� ����� ��������� (���).")]
    public float meowCooldown = 1.5f;

    [Tooltip("����������� ������ ����.")]
    [Range(0.5f, 1.0f)]
    public float minPitch = 0.9f;

    [Tooltip("������������ ������ ����.")]
    [Range(1.0f, 1.5f)]
    public float maxPitch = 1.1f;

    [Header("��������� �������� �����")]
    [Tooltip("����. ����� ����� ����� ������� ��� ������������ �������� �����.")]
    public float doubleClickThreshold = 0.3f;

    private AudioSource audioSource;
    private float lastMeowTime = -999f;
    private float lastClickTime = -1f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lastMeowTime = -meowCooldown;
    }

    private void OnMouseDown()
    {
        float currentTime = Time.time;

        if (currentTime - lastClickTime <= doubleClickThreshold)
        {

            TryMeow();
            lastClickTime = -1f; 
        }
        else
        {
            lastClickTime = currentTime;
        }
    }

    private void TryMeow()
    {
        if (Time.time < lastMeowTime + meowCooldown)
        {
            return;
        }

        if (meowSounds == null || meowSounds.Length == 0)
        {
            Debug.LogWarning("������ ������ 'meowSounds' ����!");
            return;
        }

        lastMeowTime = Time.time;

        int randomIndex = Random.Range(0, meowSounds.Length);
        AudioClip selectedClip = meowSounds[randomIndex];

        audioSource.pitch = Random.Range(minPitch, maxPitch);

        float randomDelay = Random.Range(0.05f, 0.2f);
        StartCoroutine(PlayMeowWithDelay(selectedClip, randomDelay));
    }

    private IEnumerator PlayMeowWithDelay(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        audioSource.PlayOneShot(clip);
    }
}
