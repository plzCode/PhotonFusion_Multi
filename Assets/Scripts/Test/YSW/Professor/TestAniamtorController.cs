using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class TestAnimatorController : NetworkBehaviour
{
    [SerializeField]
    private NavMeshAgent navMeshAgent;

    [SerializeField]
    private Animator animator;

    [Networked] private float speed { get; set; } = 0f;

    void Awake()
    {
        // 필요 시 GetComponentInParent로 변경 가능
        if (navMeshAgent == null)
            navMeshAgent = GetComponentInParent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;
        if (animator != null && navMeshAgent != null)
        {
            speed = navMeshAgent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }
}
