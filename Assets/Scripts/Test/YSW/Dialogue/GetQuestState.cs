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
    /// ���� Active ������ ��� ����Ʈ�� �̸��� ������ �ֿܼ� ����մϴ�.
    /// </summary>
    public static void PrintActiveQuestDescriptions()
    {
        // Active ������ ����Ʈ �̸��鸸 ��������
        var activeQuests = QuestLog.GetAllQuests(QuestState.Active);

        foreach (var questName in activeQuests)
        {
            // ����Ʈ ���� Ȯ�� (optional)
            var state = QuestLog.GetQuestState(questName);
            if (state != QuestState.Active) continue;

            // Description ��������
            string description = QuestLog.GetQuestDescription(questName);
            // �Ǵ�:
            // string description = DialogueLua.GetQuestField(questName, "Description").asString;

            Debug.Log($"[Active Quest] {questName} - {description}");
        }
    }
}
