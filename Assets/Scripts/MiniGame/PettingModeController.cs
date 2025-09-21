using UnityEngine;
using UnityEngine.UI; 
using Image = UnityEngine.UI.Image;

public class PettingModeController : MonoBehaviour
{
    [Header("������� ��� ����������")]
    [Tooltip("���������� ���� ������ � ����, ������� ����� ������")]
    public GameObject menuObject;

    [Tooltip("���������� ���� ������������ ������ ������ ���� ����� (Background2)")]
    public GameObject loveBarObject;

    [Tooltip("���������� ���� �������� ������ 'Bar' (���� ������� � ����� Image 'Filled')")]
    public Image loveBarFill;

    [Tooltip("���������� ���� ������ ����, �� ������� ����� ������ PettingManager")]
    public PettingManager pettingManager;

    [Tooltip("���������� ���� ������ � ������ ������� (��� ������� � ������)")]
    public GameObject catNameObject;

    [Tooltip("���������� ���� ������ �������� (��� �������� ����� ����� � ������, �����������)")]
    public GameObject heartSpriteObject;

    [Header("��������� �������")]
    [Tooltip("������ ���� ��� �������")]
    public Texture2D handCursor;

    [Header("��������� ������� ����")]
    [Tooltip("������ ������� ���� ��� ����� (�� Y - �����, ���������������� ��� ������ ����)")]
    public Vector3 barOffset = new Vector3(0, 1f, 0);

    [Tooltip("��������� offset �������� ������������ ���� (X ������������� ��� ����� � ��������)")]
    public Vector3 heartOffset = new Vector3(-50f, 0, 0);

    [Tooltip("������ (gap) ����� ��������� � ����� (� �������� UI)")]
    public float heartGap = 10f;

    [Tooltip("��������, ����� Canvas ������� �� ������ (billboard). ���������, ���� �������������� ������� ��������")]
    public bool useBillboard = false;

    [Tooltip("���������� ������� �������� � ������ (1f = ��������, 1.5f = ������)")]
    public float heartScaleMultiplier = 1.5f; 

    private Transform originalParent;
    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Image.FillMethod originalFillMethod;
    private int originalFillOrigin;

    private Transform originalHeartParent;
    private Vector3 originalHeartLocalPosition;
    private Quaternion originalHeartLocalRotation;
    private bool originalHeartActive;
    private Vector3 originalHeartScale;

    private GameObject tempCanvasObject;
    private Canvas tempCanvas;

    public void StartPettingMode()
    {
        if (menuObject != null) { menuObject.SetActive(false); }

        if (catNameObject != null)
        {
            catNameObject.SetActive(false);
        }

        if (loveBarObject != null)
        {
            originalParent = loveBarObject.transform.parent;
            originalLocalPosition = loveBarObject.transform.localPosition;
            originalLocalRotation = loveBarObject.transform.localRotation;

            if (loveBarFill != null)
            {
                originalFillMethod = loveBarFill.fillMethod;
                originalFillOrigin = loveBarFill.fillOrigin;
            }

            tempCanvasObject = new GameObject("TempPettingCanvas");
            tempCanvas = tempCanvasObject.AddComponent<Canvas>();
            tempCanvas.renderMode = RenderMode.WorldSpace;
            tempCanvas.worldCamera = Camera.main;
            tempCanvasObject.AddComponent<CanvasScaler>();
            tempCanvasObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            loveBarObject.transform.SetParent(tempCanvasObject.transform);
            loveBarObject.transform.localPosition = Vector3.zero;
            loveBarObject.transform.localRotation = Quaternion.identity;

            if (loveBarFill != null)
            {
                loveBarFill.fillMethod = Image.FillMethod.Horizontal;
                loveBarFill.fillOrigin = (int)Image.OriginHorizontal.Right;
            }

            loveBarObject.SetActive(true);
        }

        if (heartSpriteObject != null)
        {
            originalHeartParent = heartSpriteObject.transform.parent;
            originalHeartLocalPosition = heartSpriteObject.transform.localPosition;
            originalHeartLocalRotation = heartSpriteObject.transform.localRotation;
            originalHeartActive = heartSpriteObject.activeSelf;
            originalHeartScale = heartSpriteObject.transform.localScale; 

            heartSpriteObject.transform.SetParent(tempCanvasObject.transform);
            heartSpriteObject.transform.localRotation = Quaternion.identity;

            heartSpriteObject.transform.localScale = originalHeartScale * heartScaleMultiplier;

            heartSpriteObject.SetActive(true);
        }

        Cursor.SetCursor(handCursor, Vector2.zero, CursorMode.Auto);
        if (pettingManager != null) { pettingManager.isPettingModeActive = true; }

        UpdateBarPosition();
    }

    void Update()
    {
        if (pettingManager != null && pettingManager.isPettingModeActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EndPettingMode();
            }

            UpdateBarPosition();
        }
    }

    private void UpdateBarPosition()
    {
        if (tempCanvasObject == null || pettingManager == null) return;

        if (tempCanvas == null || tempCanvas.renderMode != RenderMode.WorldSpace) return;

        Vector3 targetPos = pettingManager.transform.position + barOffset;

        tempCanvasObject.transform.position = targetPos;

        if (useBillboard && Camera.main != null)
        {
            tempCanvasObject.transform.LookAt(Camera.main.transform);
        }

        if (heartSpriteObject != null && loveBarObject != null)
        {
            RectTransform barRect = loveBarObject.GetComponent<RectTransform>();
            RectTransform heartRect = heartSpriteObject.GetComponent<RectTransform>();

            if (barRect != null && heartRect != null)
            {
                float barHalfWidth = barRect.sizeDelta.x / 2f;
                float heartHalfWidth = heartRect.sizeDelta.x / 2f;
                float xPos = -(barHalfWidth + heartGap + heartHalfWidth);

                heartSpriteObject.transform.localPosition = new Vector3(xPos + heartOffset.x, heartOffset.y, heartOffset.z);
            }
        }

        if (loveBarFill != null)
        {
            Vector3 forward = tempCanvasObject.transform.forward;
            Vector3 toCamera = Camera.main.transform.position - tempCanvasObject.transform.position;
            bool isFlipped = Vector3.Dot(forward, toCamera) < 0; 

            loveBarFill.fillOrigin = isFlipped ? (int)Image.OriginHorizontal.Left : (int)Image.OriginHorizontal.Right;
        }
    }

    public void UpdateLoveBar(float currentValue, float maxValue)
    {
        if (loveBarFill != null)
        {
            if (maxValue <= 0)
            {
                loveBarFill.fillAmount = 0;
                return;
            }
            loveBarFill.fillAmount = currentValue / maxValue;
        }
    }

    public void EndPettingMode()
    {
        if (menuObject != null) { menuObject.SetActive(true); }
        if (loveBarObject != null)
        {
            loveBarObject.transform.SetParent(originalParent);
            loveBarObject.transform.localPosition = originalLocalPosition;
            loveBarObject.transform.localRotation = originalLocalRotation;

            if (loveBarFill != null)
            {
                loveBarFill.fillMethod = originalFillMethod;
                loveBarFill.fillOrigin = originalFillOrigin;
            }

            loveBarObject.SetActive(true);
        }

        if (heartSpriteObject != null)
        {
            heartSpriteObject.transform.SetParent(originalHeartParent);
            heartSpriteObject.transform.localPosition = originalHeartLocalPosition;
            heartSpriteObject.transform.localRotation = originalHeartLocalRotation;
            heartSpriteObject.transform.localScale = originalHeartScale; 
            heartSpriteObject.SetActive(originalHeartActive);
        }

        if (catNameObject != null)
        {
            catNameObject.SetActive(true);
        }

        if (tempCanvasObject != null)
        {
            Destroy(tempCanvasObject);
        }

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        if (pettingManager != null) { pettingManager.isPettingModeActive = false; }
    }
}