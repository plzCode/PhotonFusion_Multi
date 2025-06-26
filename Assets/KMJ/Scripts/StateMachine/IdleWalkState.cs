using UnityEngine;
using Zombie.States;

public class IdleWalkState : ZombieState
{
    enum Phase { Wait, Roam }
    Phase phase;

    const float WAIT_MIN = 1f, WAIT_MAX = 3f;
    const float MOVE_TIME = 4f;
    const float PATROL_RADIUS = 10f;

    float timer;
    Vector3 homePos;
    Vector3 roamPoint;

    readonly int zombieMask = 1 << 11;          // “Zombie” 레이어

    public IdleWalkState(ZombieAIController c) : base(c) { }

    public override void Enter()
    {
        if (!ctrl.agent.enabled || !ctrl.agent.isOnNavMesh)   // ← 가드
            return;

        homePos = ctrl.transform.position;
        ctrl.agent.isStopped = true;
        ctrl.agent.speed = ctrl.walkSpeed;
        ctrl.anim.SetFloat("Speed", 0.15f);     // 느린 걷기 블렌드
        ctrl.anim.speed = 0.75f;                // 3 s 루프

        phase = Phase.Wait;
        timer = Random.Range(WAIT_MIN, WAIT_MAX);
    }

    public override void Update()
    {
        if (!ctrl.agent.enabled || !ctrl.agent.isOnNavMesh)   // ← 가드
            return;

        switch (phase)
        {
            case Phase.Wait:
                timer -= Time.deltaTime;
                if (timer <= 0f && PickRoamPoint(out roamPoint))
                {
                    ctrl.agent.SetDestination(roamPoint);
                    ctrl.agent.isStopped = false;
                    timer = MOVE_TIME;
                    phase = Phase.Roam;
                }
                break;

            case Phase.Roam:
                timer -= Time.deltaTime;

                if (!ctrl.agent.pathPending &&
                    (ctrl.agent.remainingDistance < 0.2f || timer <= 0f) ||
                    ctrl.agent.pathStatus == UnityEngine.AI.NavMeshPathStatus.PathPartial)
                {
                    ctrl.agent.isStopped = true;
                    phase = Phase.Wait;
                    timer = Random.Range(WAIT_MIN, WAIT_MAX);
                }
                break;
        }

        if (ctrl.InAlertRadius || (ctrl.InAlertRadius && ctrl.InSightFov))
        {
            ctrl.anim.SetBool("IsAlert", true);
            ctrl.ChangeState(new AlertState(ctrl));
        }
    }

    public override void Exit() => ctrl.anim.speed = 1f;

    bool PickRoamPoint(out Vector3 point)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector3 rnd = homePos + Random.insideUnitSphere * PATROL_RADIUS;
            rnd.y = homePos.y;
            if (!UnityEngine.AI.NavMesh.SamplePosition(rnd, out var hit, 1f, UnityEngine.AI.NavMesh.AllAreas))
                continue;
            if (Physics.CheckSphere(hit.position, ctrl.agent.radius * 2, zombieMask))
                continue;

            point = hit.position; return true;
        }
        point = homePos; return false;
    }
}