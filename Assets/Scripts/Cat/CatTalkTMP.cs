using System.Collections;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class CatTalkBubble : MonoBehaviour
{
    [Header("UI")]
    public GameObject bubbleUI;
    public TextMeshProUGUI catText;

    [Header("Фразы кота по состояниям")]
    [Tooltip("Милые фразы, когда кот всем доволен")]
    [TextArea(2, 5)]
    public string[] idlePhrases;

    [Tooltip("Фразы, когда кот очень голоден")]
    [TextArea(2, 5)]
    public string[] hungryPhrases;

    [Tooltip("Фразы, когда кот хочет ласки")]
    [TextArea(2, 5)]
    public string[] affectionPhrases;

    [Tooltip("Фразы, когда кот устал и хочет спать")]
    [TextArea(2, 5)]
    public string[] tiredPhrases;

    [Header("Настройки")]
    public float minDelay = 5f;
    public float maxDelay = 15f;
    public float phraseDuration = 3f;

    private CatNeeds catNeeds;

    void Start()
    {
        catNeeds = GetComponent<CatNeeds>();
        if (catNeeds == null)
        {
            Debug.LogError("На объекте кота отсутствует скрипт CatNeeds!", this);
            return; 
        }

        if (bubbleUI != null)
        {
            bubbleUI.SetActive(false);
        }

        StartCoroutine(TalkRoutine());
    }

    IEnumerator TalkRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            CatNeeds.CatState currentState = catNeeds.GetCurrentState();

            string[] phrasesToShow;
            switch (currentState)
            {
                case CatNeeds.CatState.VeryHungry:
                    phrasesToShow = hungryPhrases;
                    break;
                case CatNeeds.CatState.NeedsLove:
                    phrasesToShow = affectionPhrases;
                    break;
                case CatNeeds.CatState.Tired:
                    phrasesToShow = tiredPhrases;
                    break;
                case CatNeeds.CatState.Content:
                default:
                    phrasesToShow = idlePhrases;
                    break;
            }

            if (phrasesToShow != null && phrasesToShow.Length > 0)
            {
                string phrase = phrasesToShow[Random.Range(0, phrasesToShow.Length)];
                yield return StartCoroutine(ShowBubble(phrase));
            }
        }
    }

    IEnumerator ShowBubble(string phrase)
    {
        bubbleUI.SetActive(true);
        catText.text = phrase;

        yield return new WaitForSeconds(phraseDuration);

        bubbleUI.SetActive(false);
        catText.text = "";
    }
}