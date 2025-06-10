using UnityEngine;
using UnityEngine.AI;

public class TestAnimatorController : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent navMeshAgent;

    [SerializeField]
    private Animator animator;

    void Awake()
    {
        // �ʿ� �� GetComponentInParent�� ���� ����
        if (navMeshAgent == null)
            navMeshAgent = GetComponentInParent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator != null && navMeshAgent != null)
        {
            float speed = navMeshAgent.velocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }
}
