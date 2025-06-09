using UnityEngine;
using Fusion;
using Pathfinding;
using PixelCrushers.DialogueSystem;

public class Professor : NetworkBehaviour
{
    private AIDestinationSetter destinationSetter;
    private AIPath aiPath;
    private Transform currentTarget;

    public float defaultDistance = 4f;      // ������ ���� �Ÿ�
    public float stairDistance = 0.8f;      // ��ܿ� ���� �Ÿ�
    public float stairCheckRadius = 0.6f;   // ���� �ݰ�
    public LayerMask stairLayer;            // ��� ���̾� (��: "Stair")
    private Transform tr;


    public override void Spawned()
    {
        destinationSetter = GetComponent<AIDestinationSetter>();
        aiPath = GetComponent<AIPath>();
        tr = transform;
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

        // ��� ����
        bool nearStairs = Physics.CheckSphere(tr.position, stairCheckRadius, stairLayer);

        aiPath.endReachedDistance = nearStairs ? stairDistance : defaultDistance;

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
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stairCheckRadius);
    }
}

