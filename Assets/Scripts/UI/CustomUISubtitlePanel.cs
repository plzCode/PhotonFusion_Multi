using PixelCrushers.DialogueSystem;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomUISubtitlePanel : PixelCrushers.DialogueSystem.Wrappers.StandardUISubtitlePanel
{
    SlidingDialoguePanel _slidingDialoguePanel;

    protected override void Awake()
    {
        base.Awake();
        _slidingDialoguePanel = GetComponentInChildren<SlidingDialoguePanel>();
    }

    protected override IEnumerator SetAnimatorAtEndOfFrame(RuntimeAnimatorController animatorController)
    { yield break; }

    public override void SetPortraitImage(Sprite sprite) { }

    public override void OpenOnStartConversation(Sprite portraitSprite, string portraitName, DialogueActor dialogueActor)
    {
        //Open();
        portraitActorName = (dialogueActor != null) ? dialogueActor.GetActorName() : portraitName;
        if (this.portraitName != null) this.portraitName.text = portraitActorName;
        if (subtitleText.text != null) subtitleText.text = string.Empty;
    }

    protected override void SetUIElementsActive(bool value)
    {
        Tools.SetGameObjectActive(panel, value);
        portraitName.SetActive(false);
        subtitleText.SetActive(false);
        Tools.SetGameObjectActive(continueButton, false); // Let ConversationView determine if continueButton should be shown.
    }

    protected override void SetSubtitleTextContent(Subtitle subtitle)
    {
        TypewriterUtility.StopTyping(subtitleText);
        var previousText = accumulateText ? accumulatedText : string.Empty;
        if (accumulateText && !string.IsNullOrEmpty(subtitle.formattedText.text))
        {
            if (numAccumulatedLines < maxLines)
            {
                numAccumulatedLines += (1 + NumCharOccurrences('\n', subtitle.formattedText.text));
            }
            else
            {
                // If we're at the max number of lines, remove the first line from the accumulated text:
                previousText = previousText.Substring(previousText.IndexOf("\n") + 1);
            }
        }
        var previousChars = accumulateText ? UITools.StripRPGMakerCodes(Tools.StripTextMeshProTags(Tools.StripRichTextCodes(previousText))).Length : 0;
        SetFormattedText(subtitleText, previousText, subtitle.formattedText);
        if (accumulateText) accumulatedText = UITools.StripRPGMakerCodes(subtitleText.text) + "\n";
        if (scrollbarEnabler != null && !HasTypewriter())
        {
            scrollbarEnabler.CheckScrollbarWithResetValue(0);
        }
        else
        {
            TypewriterUtility.StartTyping(subtitleText, subtitleText.text, previousChars);  // 시스템에 텍스트가 종료되었다는 메시지를 보내려면 필요함
            _slidingDialoguePanel.PushText($"[{portraitActorName}] {subtitleText.text}");
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _slidingDialoguePanel.Clear();
    }
}
