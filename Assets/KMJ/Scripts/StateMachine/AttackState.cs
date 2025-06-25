using UnityEngine;
using Zombie.States;

public class AttackState : ZombieState
{
    const float CLIP_LEN = 0.9f;   // Attack 애니 길이
    float t;

    public AttackState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        if (!ctrl.agent.enabled || !ctrl.agent.isOnNavMesh) return;
        ctrl.IsAttacking = true;
        ctrl.agent.isStopped = true;
        ctrl.agent.speed = 0f;
        ctrl.anim.SetFloat("Speed", 0f);
        ctrl.anim.CrossFade("Attack", 0.05f);

        t = 0f;
    }

    public override void Update()
    {
        t += Time.deltaTime;
        if (t >= CLIP_LEN)
        {
            ctrl.ChangeState(new ChaseState(ctrl));
        }
    }

    public override void Exit()
    {
        ctrl.IsAttacking = false;
        if (ctrl.agent.enabled) ctrl.agent.isStopped = false;
        ctrl.anim.SetFloat("Speed", 1f);
    }
}
