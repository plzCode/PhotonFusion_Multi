using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlidingDialoguePanel : MonoBehaviour
{
    [Header("Panel")]
    public float PanelWidth = 650f;

    [Space(2f)]
    [Header("Text")]
    public float TextContainerWidth = 650f;
    public float TextContainerHeight = 35f;

    public float TextFadeDuration = 2f;
    public float TextDisplayDuration = 2f;
    public float TextResizeDuration = 0.5f;

    #region Component
    RectTransform _rectTransform;
    VerticalLayoutGroup _verticalLayoutGroup;
    TextMeshProUGUI _textPrefab;
    #endregion

    #region Debug
    //[Header("Debug")]
    //[SerializeField] string DebugText = "Dialogue UI Initialized";
    //[SerializeField] Color DebugTextColor = Color.white;
    //int _debugTextCount = 0;
    #endregion

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        _textPrefab = Resources.Load<TextMeshProUGUI>("UI/SlidingDialogueText");
        _rectTransform.sizeDelta = new Vector2(PanelWidth, 0f);
    }

    public void PushText(string text)
    {
        PushText(text, Color.white);
    }
    public void PushText(string text, Color textColor)
    {
        TextMeshProUGUI textObj = Instantiate(_textPrefab, _verticalLayoutGroup.transform);
        textObj.SetText(text);
        textObj.color = textColor;
        textObj.alpha = 0f;

        SlidingDialogueText dialogueTextUI = textObj.GetComponent<SlidingDialogueText>();
        if (dialogueTextUI != null)
        {
            dialogueTextUI.DialoguePanel = this;
            dialogueTextUI.DisplayDuration = TextDisplayDuration;
            dialogueTextUI.FadeDuration = TextFadeDuration;
            dialogueTextUI.ResizeDuration = TextResizeDuration;
            dialogueTextUI.TargetHeight = GetTextContainerHeight(textObj);
            StartCoroutine(dialogueTextUI.PlayTextAnimation());
        }

        RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
        textRectTransform.sizeDelta = new Vector2(TextContainerWidth, 0f);
    }

    float GetTextContainerHeight(TextMeshProUGUI text)
    {
        Vector2 preferredSize = text.GetPreferredValues(TextContainerWidth, 0);
        int lineCount = Mathf.CeilToInt(preferredSize.y / TextContainerHeight);
        float height = lineCount * TextContainerHeight;

        return height;
    }

    public void Clear()
    {
        _rectTransform.sizeDelta = new Vector2(PanelWidth, 0f);
        SlidingDialogueText[] texts = GetComponentsInChildren<SlidingDialogueText>(true);
        
        for (int i = 0; i < texts.Length; i++)
            Destroy(texts[i].gameObject);
    }
}
