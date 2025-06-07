using Fusion;
using UnityEngine;
using static Fusion.NetworkBehaviour;
using static KartInput;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    private Rigidbody rb;

    [SerializeField]
    private CharacterController controller;

    [Networked] public RoomPlayer RoomUser { get; set; }


    //[Networked] public float AppliedSpeed { get; set; }
    //[Networked] public float MaxSpeed { get; set; }
    //[Networked] private KartInput.NetworkInputData Inputs { get; set; }
    [Networked] private PlayerInput.NetworkInputData PlayerInputs { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody is Not Found");
        }
    }
    public override void Spawned()
    {
        base.Spawned();
        //_changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        //MaxSpeed = maxSpeedNormal;
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (GetInput(out PlayerInput.NetworkInputData input))
        {
            Debug.Log("Input Is Entered");

            Vector3 move = new Vector3(input.MoveDirection.x, 0, input.MoveDirection.y);
            move = move.normalized * moveSpeed;

            // y ���� �ӵ��� ���� Rigidbody�� ���� ���� (�߷� ����)
            move.y = rb.linearVelocity.y;

            rb.linearVelocity = move;
        }
    }

    



}
