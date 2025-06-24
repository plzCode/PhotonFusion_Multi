using Unity.VisualScripting;
using UnityEngine;
using Zombie.States;


public class DieState : ZombieState
{
    public DieState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.anim.SetBool("IsDead", true);
        if (ctrl.agent.isOnNavMesh)
            ctrl.agent.isStopped = true;
    }

    public override void Update() { }      // 아무것도 안 함
}