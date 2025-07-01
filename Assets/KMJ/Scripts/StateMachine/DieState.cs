using Unity.VisualScripting;
using UnityEngine;
using Zombie.States;


public class DieState : ZombieState
{
    public DieState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.anim.CrossFade("Die", 0.05f);
        ctrl.zCtrl.SfxDie();
        if (ctrl.agent.enabled && ctrl.agent.isOnNavMesh)
        {
            ctrl.agent.enabled = false;
        }
        ctrl.GetComponent<Collider>().enabled = false;
    }
    public override void Update() { }
    public override void Exit() { }
}