using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueText_UI : MonoBehaviour
{
    [HideInInspector] public Dialogue_UI Dialogue_UI;

    [HideInInspector] public float Duration = 2f;
    [HideInInspector] public float TextFadeInSpeed = 2f;
    [HideInInspector] public float TextFadeOutSpeed = 2f;
    [HideInInspector] public float WaitTimeBeforeFade = 0.5f;
    [HideInInspector] public float MinimizeTime = 0.5f;

    TextMeshProUGUI _text;
    RectTransform _rectTransform;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public IEnumerator PlayTextAnimation()
    {
        yield return new WaitForSeconds(WaitTimeBeforeFade);
        yield return FadeInText();
        yield return new WaitForSeconds(Duration);
        yield return FadeOutText();
        //yield return Minimize();
        Destroy(gameObject);
    }

    IEnumerator FadeInText()
    {
        _text.alpha = 0f;
        while (_text.alpha < 1f)
        {
            _text.alpha += Time.deltaTime * TextFadeInSpeed;
            yield return null;
        }
    }

    IEnumerator FadeOutText()
    {
        while (_text.alpha > 0f)
        {
            _text.alpha -= Time.deltaTime * TextFadeOutSpeed;
            yield return null;
        }
    }

    // 이 함수의 역할은 의미는 없어보임
    IEnumerator Minimize()
    {
        Vector2 originalSize = _rectTransform.sizeDelta;
        Vector2 targetSize = new Vector2(originalSize.x, 0f);
        float elapsedTime = 0f;
        while (elapsedTime < MinimizeTime)
        {
            _rectTransform.sizeDelta = Vector2.Lerp(originalSize, targetSize, elapsedTime / MinimizeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _rectTransform.sizeDelta = targetSize; // Ensure it reaches the target size
    }

    void OnDestroy()
    {
        Dialogue_UI.OnDestroyText();
    }
}
