using UnityEngine;
using UnityEngine.InputSystem.XR;
using Zombie.States;

public class AlertState : ZombieState
{
    float timer;

    public AlertState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        timer = 0f;
        ctrl.SetMoveSpeed(0f);              // 제자리
        ctrl.anim.SetBool("IsAlert", true); // 애니메이터 bool ON
    }

    public override void Update()
    {
        /* ① 플레이어를 계속 보고 있나? */
        if (ctrl.CanSeePlayer())
        {
            timer += Time.deltaTime;        // 시야 유지 시간 누적

            /* ② 2초 이상 감지 유지 → Chase 전환 */
            if (timer >= 2f)
            {
                ctrl.ChangeState(new ChaseState(ctrl));
            }
        }
        else
        {
            /* ③ 시야를 잃었다 → IdleWalk(배회)로 복귀 */
            ctrl.anim.SetBool("IsAlert", false);
            ctrl.ChangeState(new IdleWalkState(ctrl));
        }
    }

    public override void Exit()
    {
        ctrl.anim.SetBool("IsAlert", false);   // 상태 나갈 때 항상 OFF
    }
}
