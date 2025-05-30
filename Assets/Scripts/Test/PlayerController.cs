using Fusion;
using UnityEngine;
using static Fusion.NetworkBehaviour;
using static KartInput;

public class PlayerController : NetworkBehaviour
{   
    public float moveSpeed = 5f;
    public float gravity = -9.81f;
    public float jumpForce = 5f;

    private float verticalVelocity = 0f;

    private CharacterController controller;


    [Networked] public RoomPlayer RoomUser { get; set; }


    //[Networked] public float AppliedSpeed { get; set; }
    //[Networked] public float MaxSpeed { get; set; }
    [Networked] private KartInput.NetworkInputData Inputs { get; set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
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

        if (GetInput(out NetworkInputData input))
        {
            Debug.Log("�Է��� �ֽ��ϴ�.");
            Vector3 move = new Vector3(input.MoveDirection.x, 0, input.MoveDirection.y);

            // �߷� ����
            if (controller.isGrounded)
            {
                verticalVelocity = 0f;

                // ���� Ű �Է� (���ϸ� ���⿡ �߰� ����)
                // if (Input.GetKey(KeyCode.Space)) verticalVelocity = jumpForce;
            }
            else
            {
                verticalVelocity += gravity * Runner.DeltaTime;
            }

            move.y = verticalVelocity;

            controller.Move(move * moveSpeed * Runner.DeltaTime);
        }




    }

    



}
