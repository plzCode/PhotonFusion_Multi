using UnityEngine;
using Fusion;
using Pathfinding;
using PixelCrushers.DialogueSystem;

public class Professor : NetworkBehaviour
{
    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private Transform currentTarget;

    public override void Spawned()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
    }

    public void SetTarget(Transform target)
    {
        if (!Object.HasStateAuthority) return;

        currentTarget = target;
        destinationSetter.target = currentTarget;
        aiPath.SearchPath(); // ��� ��� ����
    }

    void Update()
    {
        //if (!Object.HasStateAuthority) return;
        //Debug.Log(currentTarget != null ? "���� Ÿ��: " + currentTarget.name : "���� Ÿ�� ����");
        /*if (DialogueLua.GetVariable("GoToTarget").asBool)
        {
            if (destinationSetter != null && currentTarget != null)
            {
                destinationSetter.target = currentTarget;
                aiPath.SearchPath();                
            }            
        }*/
        if (DialogueLua.GetVariable("GoToTarget").asBool)
        {
            if (destinationSetter != null && currentTarget != null)
            {
                if (destinationSetter.target != currentTarget)
                {
                    destinationSetter.target = currentTarget;
                    aiPath.SearchPath();
                }
            }
        }
    }

    void OnEnable()
    {
        DialogueManager.instance.conversationStarted += OnConversationStarted;
    }

    void OnDisable()
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationStarted -= OnConversationStarted;
    }

    private void OnConversationStarted(Transform actor)
    {
        currentTarget = actor;
        Debug.Log("��ȭ �õ��� Transform �����: " + currentTarget.name);
    }
}

