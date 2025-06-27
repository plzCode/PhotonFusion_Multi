using UnityEngine;
using Zombie.States;
using System.Linq;

public class AlertState : ZombieState
{
    const string CLIP = "zombie_alert_to_aggro_01";
    bool clipDone;

    public AlertState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.agent.isStopped = true;
        ctrl.agent.velocity = Vector3.zero;

        ctrl.anim.speed = 1f;
        ctrl.anim.CrossFade(CLIP, 0.05f);
        clipDone = false;
    }

    public override void Update()
    {
        if (clipDone) return;

        var info = ctrl.anim.GetCurrentAnimatorStateInfo(0);
        if (info.IsName(CLIP) && info.normalizedTime >= 1f)
        {
            clipDone = true;
            ctrl.agent.isStopped = false;
            ctrl.ChangeState(new ChaseState(ctrl));
        }
    }

    public override void Exit()
    {
        ctrl.agent.isStopped = false;
    }
}