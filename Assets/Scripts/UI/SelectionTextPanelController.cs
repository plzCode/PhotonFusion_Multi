using PixelCrushers.DialogueSystem;
using System.Collections;
using UnityEngine;

public class SelectionTextPanelController : MonoBehaviour
{
    public float yOffset = 50f;

    Coroutine _updateCoroutine;
    [SerializeField] RectTransform _rectTransform;

    void Start()
    {
        Selector selector = FindAnyObjectByType<Selector>();

        selector.onSelectedUsable.AddListener(OnSelectedUsableEvent);
        selector.onDeselectedUsable.AddListener(OnDeselectedUsableEvent);
    }

    void OnSelectedUsableEvent(Usable usable)
    {
        if (_updateCoroutine != null)
            StopCoroutine(_updateCoroutine);

        _updateCoroutine = StartCoroutine(UpdatePosition(usable.transform));
    }

    void OnDeselectedUsableEvent(Usable usable)
    {
        if (_updateCoroutine == null)
            return;

        StopCoroutine(_updateCoroutine);
    }

    IEnumerator UpdatePosition(Transform conversant)
    {
        RectTransform canvasTransform = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        
        while (true)
        {
            if (conversant == null)
            {
                Debug.LogError("conversant is null. Please check if the Dialogue Manager is set up correctly.");
                break;
            }

            Vector3 screenPosition = Camera.main.WorldToScreenPoint(conversant.position);

            // 위치 보정
            screenPosition.y += yOffset;

            bool isValid = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasTransform,
                screenPosition,
                null,
                out Vector2 localPoint);
            if (isValid)
                _rectTransform.anchoredPosition = localPoint;

            yield return null;
        }
    }
}
