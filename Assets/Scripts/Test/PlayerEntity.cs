using Fusion;
using UnityEngine;

public class PlayerEntity : NetworkBehaviour
{
    public PlayerController Controller { get; private set; }

    private void Awake()
    {
        Controller = GetComponent<PlayerController>();
    }
}
