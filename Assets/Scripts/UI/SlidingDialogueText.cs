using System.Collections;
using TMPro;
using UnityEngine;

public class SlidingDialogueText : MonoBehaviour
{
    [HideInInspector] public SlidingDialoguePanel DialoguePanel;

    [HideInInspector] public float DisplayDuration = 2f;
    [HideInInspector] public float FadeDuration = 2f;
    [HideInInspector] public float ResizeDuration = 0.5f;
    [HideInInspector] public float TargetHeight = 0f;
    public TextMeshProUGUI Text { get; private set; }
    public RectTransform RectTransform { get; private set; }

    void Awake()
    {
        Text = GetComponent<TextMeshProUGUI>();
        RectTransform = GetComponent<RectTransform>();
    }

    public IEnumerator PlayTextAnimation()
    {
        yield return Resize(0f, TargetHeight);
        yield return Fade(0f, 1f);
        yield return new WaitForSeconds(DisplayDuration);
        yield return Fade(1f, 0f);
        yield return Resize(TargetHeight, 0f);
        Destroy(gameObject);
    }

    IEnumerator Resize(float initHeight, float targetHeight)
    {
        float elapsedTime = 0f;
        float originalHeight = initHeight;
        Vector2 targetSize = new Vector2(RectTransform.sizeDelta.x, targetHeight);
        while (elapsedTime < ResizeDuration)
        {
            RectTransform.sizeDelta = Vector2.Lerp(RectTransform.sizeDelta, targetSize, elapsedTime / ResizeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        RectTransform.sizeDelta = targetSize;
    }

    IEnumerator Fade(float initAlpha, float targetAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < FadeDuration)
        {
            Text.alpha = Mathf.Lerp(initAlpha, targetAlpha, elapsedTime / FadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        Text.alpha = targetAlpha;
    }
}
