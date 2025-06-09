using UnityEngine;
using Pathfinding;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;

    void Start()
    {
        GetComponent<AIDestinationSetter>().target = target;
    }
}
