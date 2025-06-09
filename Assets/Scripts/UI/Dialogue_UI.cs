using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue_UI : Base_UI
{
    [Header("Panel")]
    public float PanelWidth = 650f;
    public float PanelPaddingHeight = 10f;

    [Space(2f)]
    [Header("Text")]
    public float TextContainerWidth = 650f;
    public float TextContainerHeight = 35f;

    public float TextFadeInSpeed = 2f;
    public float TextFadeOutSpeed = 2f;
    public float TextDuration = 2f;
    public float TextMinimizeTime = 0.5f;

    #region Component
    RectTransform _rectTransform;
    VerticalLayoutGroup _verticalLayoutGroup;
    TextMeshProUGUI _textPrefab;
    #endregion

    #region Adjust Layout
    [SerializeField] float _layoutAdjustTime = 0.3f;
    bool _isAdjustingLayout = false;

    float _currentHeight;
    float _targetHeight;
    float _percent = 0f;
    float _elapsedTime = 0f;
    #endregion

    #region Debug
    //[SerializeField] string DebugText = "Dialogue UI Initialized";
    //[SerializeField] Color DebugTextColor = Color.white;
    //int _debugTextCount = 0;
    #endregion


    public override void Initialize()
    {
        base.Initialize();

        _rectTransform = GetComponent<RectTransform>();
        _verticalLayoutGroup = GetComponent<VerticalLayoutGroup>();
        _textPrefab = Resources.Load<TextMeshProUGUI>("UI/DialogueText");
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

        DialogueText_UI dialogueTextUI = textObj.gameObject.GetComponent<DialogueText_UI>();
        dialogueTextUI.Dialogue_UI = this;
        dialogueTextUI.Duration = TextDuration;
        dialogueTextUI.TextFadeInSpeed = TextFadeInSpeed;
        dialogueTextUI.TextFadeOutSpeed = TextFadeOutSpeed;
        dialogueTextUI.WaitTimeBeforeFade = _layoutAdjustTime + 0.2f;
        dialogueTextUI.MinimizeTime = TextMinimizeTime;

        RectTransform textRectTransform = textObj.GetComponent<RectTransform>();
        float textContainerHeight = GetTextContainerHeight(textObj);
        textRectTransform.sizeDelta = new Vector2(TextContainerWidth, textContainerHeight);

        StartCoroutine(AdjustLayoutSize());
        StartCoroutine(dialogueTextUI.PlayTextAnimation());
    }

    public void OnDestroyText()
    {
        StartCoroutine(AdjustLayoutSize());
    }

    IEnumerator AdjustLayoutSize()
    {
        _currentHeight = _rectTransform.rect.height;
        _targetHeight = GetPanelHeight();
        _percent = 0f;
        _elapsedTime = 0f;

        if (_isAdjustingLayout)
            yield break;

        _isAdjustingLayout = true;

        while (_percent < 1f)
        {
            _elapsedTime += Time.deltaTime;
            _percent = _elapsedTime / _layoutAdjustTime;

            float nextHeight = Mathf.Lerp(_currentHeight, _targetHeight, _percent);
            _rectTransform.sizeDelta = new Vector2(PanelWidth, nextHeight);

            yield return null;
        }

        _rectTransform.sizeDelta = new Vector2(PanelWidth, _targetHeight);

        _isAdjustingLayout = false;
    }

    float GetPanelHeight()
    {
        float totalHeight = 0f;
        TextMeshProUGUI[] texts = _verticalLayoutGroup.GetComponentsInChildren<TextMeshProUGUI>();

        if (texts.Length == 0)
            return 0f;

        for (int i = 0; i < texts.Length; ++i)
        {
            totalHeight += texts[i].rectTransform.sizeDelta.y;
        }

        totalHeight += Mathf.Max(0, (texts.Length - 1)) * _verticalLayoutGroup.spacing;
        totalHeight += PanelPaddingHeight;

        return totalHeight;
    }

    float GetTextContainerHeight(TextMeshProUGUI text)
    {
        Vector2 preferredSize = text.GetPreferredValues(TextContainerWidth, 0);
        int lineCount = Mathf.CeilToInt(preferredSize.y / TextContainerHeight);
        float height = lineCount * TextContainerHeight;

        return height;
    }
}
