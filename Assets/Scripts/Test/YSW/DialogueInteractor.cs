using UnityEngine;
using Fusion;
using Pathfinding; // AIDestinationSetter, AIPath
using PixelCrushers.DialogueSystem; // DialogueManager, DialogueLua
using System.Collections.Generic;

// 플레이어 상호작용 스크립트 예시
public class DialogueInteractor : NetworkBehaviour
{
    public Professor seekerController;

    public void Interact()
    {
        if (!Runner.IsServer) return;

        DialogueManager.StartConversation("NPC", transform, seekerController.transform);
        seekerController.SetTarget(transform);
    }
}

