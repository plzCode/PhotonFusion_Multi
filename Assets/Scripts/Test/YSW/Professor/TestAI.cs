using UnityEngine;
using Fusion;
using UnityEngine.AI;
using PixelCrushers.DialogueSystem;

public class TestAI : NetworkBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] private Transform currentTarget;
    private Transform tr;

    [Header("Navigation Settings")]
    public float defaultDistance = 4f;
    public float stairDistance = 0.8f;
    public float stairCheckRadius = 0.6f;
    public LayerMask stairLayer;
    private bool nearStairs = false;

    public float rotationSpeed = 10f;

    [Header("Stats")]
    [Networked] public float health { get; set; } = 100f; // AI의 체력

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        tr = transform;
    }

    public override void Spawned()
    {
        InitAgent();
    }

    public void SetTarget(Transform target)
    {
        if (!Object.HasStateAuthority) return;

        currentTarget = target;

        if (agent != null && currentTarget != null)
        {
            agent.isStopped = false;
            agent.SetDestination(currentTarget.position);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;

        // 계단 감지
        nearStairs = Physics.CheckSphere(tr.position, stairCheckRadius, stairLayer);

        // 도달 거리 설정
        agent.stoppingDistance = nearStairs ? stairDistance : defaultDistance;

        if (DialogueLua.GetVariable("GoToTarget").asBool && currentTarget != null)
        {
            float distance = Vector3.Distance(tr.position, currentTarget.position);

            if (distance > agent.stoppingDistance + 0.1f)
            {
                agent.isStopped = false;
                agent.SetDestination(currentTarget.position);
            }
            else
            {
                /*agent.isStopped = true;
                agent.velocity = Vector3.zero;*/
            }

            // 직접 회전 제어
            Vector3 dir = agent.desiredVelocity.normalized;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                tr.rotation = Quaternion.Slerp(tr.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
        }
        else
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }

    private void OnEnable()
    {
        DialogueManager.instance.conversationStarted += OnConversationStarted;
    }

    private void OnDisable()
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationStarted -= OnConversationStarted;
    }

    private void OnConversationStarted(Transform actor)
    {
        currentTarget = actor;
        Debug.Log("대화 시도자 Transform 저장됨: " + currentTarget.name);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stairCheckRadius);
    }

    #region navmesh agent Settings
    private void InitAgent()
    {
        if (agent != null)
        {
            if (!Object.HasStateAuthority)
            {
                agent.enabled = false; // 클라이언트에서 비활성화
            }
            else
            {
                agent.updateRotation = false; // 직접 회전 제어
            }
        }
    }
    #endregion

    public void TakeDamage(float damage)
    {
        //if (!HasStateAuthority) return;
        health -= damage;
        if (health <= 0f)
        {
            // Handle death logic here
            Debug.Log("AI has died.");
            // Optionally, you can disable the agent or trigger a death animation
            agent.isStopped = true;
            agent.enabled = false;
        }
    }

    public void Heal(float amount)
    {
        if (!Object.HasStateAuthority) return;
        health += amount;
        if (health > 100f) health = 100f; // Max health limit
    }

    public void ResetAI()
    {
        if (!Object.HasStateAuthority) return;

        health = 100f;

        // 에이전트 재활성화
        if (!agent.enabled)
            agent.enabled = true;

        agent.isStopped = false;
    }

}
