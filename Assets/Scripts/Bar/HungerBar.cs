using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HungerBar : MonoBehaviour
{
    private Slider slider;
    private Transform target; 
    private Vector3 offset = new Vector3(0, 0.7f, 0); 

    private Transform canvasTransform;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void Initialize(Transform targetToFollow, Transform parentCanvasTransform)
    {
        this.target = targetToFollow;
        this.canvasTransform = parentCanvasTransform;
    }

    void LateUpdate()
    {
        if (target == null || canvasTransform == null)
        {
            if (canvasTransform != null) Destroy(canvasTransform.gameObject);
            return;
        }

        canvasTransform.position = target.position + offset;
        canvasTransform.rotation = Camera.main.transform.rotation;
    }

    public void UpdateBar(float currentValue, float maxValue)
    {
        if (slider != null && maxValue > 0)
        {
            slider.value = currentValue / maxValue;
        }
    }
}