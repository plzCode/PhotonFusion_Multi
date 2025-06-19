using Unity.VisualScripting;
using UnityEngine;
using Zombie.States;


public class DieState : ZombieState
{
    public DieState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        ctrl.SetMoveSpeed(0);
        ctrl.anim.SetBool("IsDead", true);      // Die 애니 Bool
        ctrl.agent.enabled = false;             // 네비 꺼두기
    }

    public override void Update() { }      // 아무것도 안 함
}