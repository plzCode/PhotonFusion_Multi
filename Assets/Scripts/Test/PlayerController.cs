using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Networked] public RoomPlayer RoomUser { get; set; }
}
