using System.Linq;
using UnityEngine;
using Zombie.States;

public class AttackState : ZombieState
{

    public AttackState(ZombieAIController c) : base(c)
    {
        //var clips = ctrl.anim.runtimeAnimatorController.animationClips;
        //attackLength = clips.First(c => c.name == "zombie_swipe_attack").length;
    }
    public override void Enter()
    {
        ctrl.IsAttacking = true;

        // 움직임 멈추기
        if (ctrl.agent.isOnNavMesh)
            ctrl.agent.isStopped = true;

        // 한 번만 재생
        ctrl.anim.Play("Attack", 0, 0f);
        ctrl.zCtrl.SfxAttack();
    }

    public override void Update()
    {
        AnimatorStateInfo state = ctrl.anim.GetCurrentAnimatorStateInfo(0);

        // 1) “Attack” 애니메이션이 끝날 때까지 기다림
        if (state.IsName("Attack") && state.normalizedTime < 1f)
        {
            return;  // 아직 애니가 끝나지 않았다
        }

        // 2) 애니 끝난 시점: 사거리 밖이면 추적, 안이면 재공격
        if (!ctrl.InAttackRange)
        {
            ctrl.ChangeState(new ChaseState(ctrl));
        }
        else
        {
            ctrl.ChangeState(new AttackState(ctrl));
        }
    }

    public override void Exit()
    {
        // 다음 상태(Chase) 진입 전에 멈춤 해제
        if (ctrl.agent.isOnNavMesh)
            ctrl.agent.isStopped = false;

        ctrl.IsAttacking = false;
    }
}