using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [SerializeField]
    private RectTransform dragRectTransform;

    private Vector2 offset; 

    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = (Vector2)dragRectTransform.position - eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        dragRectTransform.position = eventData.position + offset;
    }
}