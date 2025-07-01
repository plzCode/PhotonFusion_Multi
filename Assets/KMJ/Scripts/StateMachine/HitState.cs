using System.Linq;
using UnityEngine;
using Zombie.States;


public class HitState : ZombieState
{
    const string STATE = "Hit";
    const string CLIP = "zombie_hit_react_F_01";
    float timer;

    public HitState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.agent.isStopped = true;             // 경직
        ctrl.zCtrl.SfxHit();                // 맞았을 때 사운드
        ctrl.agent.velocity = Vector3.zero;     // 미끄러짐 방지
        ctrl.anim.CrossFade(STATE, 0.05f, 0);
        timer = ctrl.anim.runtimeAnimatorController.animationClips
                .First(x => x.name == CLIP).length;   // ≈ 1.1~1.3 s
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0) return;

        ctrl.agent.isStopped = false;
        ctrl.ChangeState(ctrl.InSightFov ? new ChaseState(ctrl)
                                         : new IdleWalkState(ctrl));
    }
}