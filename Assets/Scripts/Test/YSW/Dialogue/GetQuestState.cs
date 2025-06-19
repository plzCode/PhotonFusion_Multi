using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Collections.Generic;

public class GetQuestState : MonoBehaviour
{
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            PrintActiveQuestDescriptions();
        }

    }
    /// <summary>
    /// 현재 Active 상태인 모든 퀘스트의 이름과 설명을 콘솔에 출력합니다.
    /// </summary>
    public static void PrintActiveQuestDescriptions()
    {
        // Active 상태인 퀘스트 이름들만 가져오기
        var activeQuests = QuestLog.GetAllQuests(QuestState.Active);

        foreach (var questName in activeQuests)
        {
            // 퀘스트 상태 확인 (optional)
            var state = QuestLog.GetQuestState(questName);
            if (state != QuestState.Active) continue;

            // Description 가져오기
            string description = QuestLog.GetQuestDescription(questName);
            // 또는:
            // string description = DialogueLua.GetQuestField(questName, "Description").asString;

            Debug.Log($"[Active Quest] {questName} - {description}");
        }
    }
}
