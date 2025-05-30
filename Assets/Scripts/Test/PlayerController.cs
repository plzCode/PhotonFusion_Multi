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
            Debug.Log("입력이 있습니다.");
            Vector3 move = new Vector3(input.MoveDirection.x, 0, input.MoveDirection.y);

            // 중력 적용
            if (controller.isGrounded)
            {
                verticalVelocity = 0f;

                // 점프 키 입력 (원하면 여기에 추가 가능)
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
