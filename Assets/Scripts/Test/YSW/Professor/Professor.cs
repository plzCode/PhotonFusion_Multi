using UnityEngine;
using Pathfinding;
using Fusion;
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
        aiPath.SearchPath(); // 경로 즉시 갱신
    }

    void Update()
    {
        if (!Object.HasStateAuthority) return;

        if (DialogueLua.GetVariable("GoToTarget").asBool)
        {
            if (destinationSetter != null && currentTarget != null)
            {
                destinationSetter.target = currentTarget;
                aiPath.SearchPath();
                DialogueLua.SetVariable("GoToTarget", false);
            }
        }
    }
}
