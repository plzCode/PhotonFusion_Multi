using UnityEngine;

public class IdleWalkState : ZombieState
{
    public IdleWalkState(ZombieAIController controller) : base(controller) { }

    public override void Enter()
    {
        //controller.SetMoveSpeed(0.7f);
        //controller.PlayBlend("IdleWalk", 0.7f); // Animator에 Speed 반영
    }

    public override void Update()
    {
        //if (controller.CanSeePlayer())
        //{
        //    controller.ChangeState(new AlertState(controller));
        //}
    }
}