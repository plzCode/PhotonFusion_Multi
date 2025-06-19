using Fusion;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(ZombieDetection), typeof(NavMeshAgent))]
public class ZombieAIController : NetworkBehaviour
{
    public enum State { Idle, Alert, Chase }
    [Networked] State current { get; set; }

    public State CurrentState => current;
    public Transform Target { get; private set; }
    NavMeshAgent agent;
    ZombieDetection detect;
    Vector3 alertTarget;

    public override void Spawned()
    {
        agent = GetComponent<NavMeshAgent>();
        detect = GetComponent<ZombieDetection>();
        Target = GameObject.FindGameObjectWithTag("Player")?.transform;
        current = State.Idle;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        switch (current)
        {
            case State.Idle:
                if (detect.CanSeePlayer())
                {
                    current = State.Chase;
                    Debug.Log($"{name} ▶ Chase");
                }
                break;

            case State.Alert:
                agent.SetDestination(alertTarget);
                if (agent.remainingDistance < 0.3f)
                {
                    current = State.Idle;
                    Debug.Log($"{name} ▶ Idle");
                }
                if (detect.CanSeePlayer())
                {
                    current = State.Chase;
                    Debug.Log($"{name} ▶ Chase");
                }
                break;

            case State.Chase:
                if (detect.CanSeePlayer())
                {
                    agent.SetDestination(Target.position);
                }
                else
                {
                    // 놓쳤으면 마지막 위치로 Alert
                    alertTarget = Target.position;
                    current = State.Alert;
                    Debug.Log($"{name} ▶ Alert");
                }
                break;
        }
    }

    /* 사운드 알람용 공개 메서드 */
    public void OnHearSound(Vector3 pos)
    {
        if (current == State.Idle && detect.CanHearSound(pos))
        {
            alertTarget = pos;
            current = State.Alert;
            Debug.Log($"{name} ▶ Alert (sound)");
        }
    }
}
