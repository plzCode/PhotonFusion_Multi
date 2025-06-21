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
            if (ctrl.Target != null)
            {
                bool canSee = ctrl.CanSeePlayer();
                Debug.DrawLine(ctrl.transform.position + Vector3.up * 1.2f,
                               ctrl.Target.position + Vector3.up * 1.2f,
                               canSee ? Color.green : Color.red);
                Debug.Log($"[AI] Target={ctrl.TargetNetObj}");
            }
            ctrl.anim.SetBool("IsAlert", true);   // ← Bool ON
            ctrl.ChangeState(new AlertState(ctrl));
        }

    }
}