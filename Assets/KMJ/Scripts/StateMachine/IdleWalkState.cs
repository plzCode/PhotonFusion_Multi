using UnityEngine;
using Zombie.States;

public class IdleWalkState : ZombieState
{
    public IdleWalkState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.SetMoveSpeed(0.7f);
        ctrl.PlayBlend(0.2f);               // Idle-Walk 블렌드
    }
    public override void Update()
    {
        if (ctrl.CanSeePlayer())
        {
            ctrl.anim.SetBool("IsAlert", true);   // ← Bool ON
            ctrl.ChangeState(new AlertState(ctrl));
        }
    }
}