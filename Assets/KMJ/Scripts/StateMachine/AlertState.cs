using System.Linq;
using UnityEngine;
using Zombie.States;

public class AlertState : ZombieState
{
    const string STATE = "Alert";                      // Animator  State
    const string CLIP = "zombie_alert_to_aggro_01";   // AnimationClip
    float timer;

    public AlertState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.agent.isStopped = true;
        ctrl.agent.velocity = Vector3.zero;

        // 애니 재생 + 길이만큼 타이머
        ctrl.anim.CrossFade(STATE, 0.05f, 0);
        timer = ctrl.anim.runtimeAnimatorController.animationClips
                .First(x => x.name == CLIP).length;
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0) return;

        ctrl.agent.isStopped = false;
        ctrl.ChangeState(new ChaseState(ctrl));
    }
}