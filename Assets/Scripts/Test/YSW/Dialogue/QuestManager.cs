using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

[System.Serializable]
public class QuestActivationConfig
{
    public string questName;                     // 감시할 퀘스트 이름
    public GameObject[] targetObjects;           // bool 값을 true로 설정할 오브젝트들
    public string targetBoolName = "isActivated"; // 해당 오브젝트에 있는 bool 변수 이름
}

public class QuestManager : MonoBehaviour
{    
    public List<QuestActivationConfig> quests = new List<QuestActivationConfig>();

    private Dictionary<string, bool> lastQuestStates = new Dictionary<string, bool>();

    void Start()
    {
        foreach (var quest in quests)
        {
            lastQuestStates[quest.questName] = false;
        }
    }

    void Update()
    {
        foreach (var config in quests)
        {
            var questState = QuestLog.GetQuestState(config.questName);
            bool isActiveNow = questState == QuestState.Active;
            bool wasActiveBefore = lastQuestStates[config.questName];

            if (isActiveNow != wasActiveBefore)
            {
                SetBools(config, isActiveNow);
                lastQuestStates[config.questName] = isActiveNow;
            }
        }
    }

    void SetBools(QuestActivationConfig config, bool value)
    {
        foreach (var obj in config.targetObjects)
        {
            var components = obj.GetComponents<MonoBehaviour>();
            foreach (var comp in components)
            {
                var type = comp.GetType();
                var field = type.GetField(config.targetBoolName);
                if (field != null && field.FieldType == typeof(bool))
                {
                    field.SetValue(comp, value);
                    Debug.Log($"[{config.questName}] {obj.name}의 '{config.targetBoolName}'를 {value}로 설정함");
                }
            }
        }
    }
}
