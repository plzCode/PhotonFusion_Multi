using UnityEngine;
using Fusion;
using Pathfinding; // AIDestinationSetter, AIPath
using PixelCrushers.DialogueSystem; // DialogueManager, DialogueLua
using System.Collections.Generic;

// �÷��̾� ��ȣ�ۿ� ��ũ��Ʈ ����
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

