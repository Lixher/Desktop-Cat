using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class WidgetInteraction : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки виджета")]
    [Tooltip("Изображение фона виджета, цвет которого мы будем менять")]
    public Image widgetBackground;

    [Header("Ссылки на UI")]
    [Tooltip("Перетащите сюда вашу панель ColorPickerPanel")]
    public ColorPickerController colorPicker;

    [Tooltip("Перетащите сюда вашу панель TooltipPanel")]
    public GameObject tooltipObject;

    [Header("Настройки подсказки")]
    [Tooltip("Через сколько секунд после наведения появится подсказка")]
    public float tooltipShowDelay = 0.5f;

    [Tooltip("Сколько секунд подсказка будет видна на экране")]
    public float tooltipDuration = 2.5f;

    private Coroutine tooltipCoroutine;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (colorPicker != null)
            {
                if (colorPicker.gameObject.activeInHierarchy)
                {
                    colorPicker.ClosePicker();
                }
                else
                {
                    colorPicker.OpenPicker(widgetBackground);
                    colorPicker.transform.position = eventData.position;
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
        }
        tooltipCoroutine = StartCoroutine(ShowTooltipRoutine(eventData.position));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipCoroutine != null)
        {
            StopCoroutine(tooltipCoroutine);
        }
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
    }

    private IEnumerator ShowTooltipRoutine(Vector2 position)
    {
        yield return new WaitForSeconds(tooltipShowDelay);

        if (tooltipObject != null)
        {
            tooltipObject.SetActive(true);
            tooltipObject.transform.position = position + new Vector2(10, -10);
        }

        yield return new WaitForSeconds(tooltipDuration);

        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
    }
}