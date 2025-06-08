using UnityEngine;
using Pathfinding;

public class ProfessorAnimatorController : MonoBehaviour
{
    [SerializeField]
    private AIPath aiPath;
    [SerializeField]
    private Animator animator;

    void Awake()
    {
        aiPath = GetComponentInParent<AIPath>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator != null && aiPath != null)
        {
            float speed = aiPath.desiredVelocity.magnitude;
            animator.SetFloat("Speed", speed);
        }
    }
}
