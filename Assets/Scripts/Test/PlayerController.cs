using Fusion;
using UnityEngine;
using static Fusion.NetworkBehaviour;
using static KartInput;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private Vector2 currentInput = Vector2.zero;
    public float inputSmoothSpeed = 5f; // or whatever feels right

    private float currentSpeedMultiplier = 1.0f;
    private const float walkSpeed = 1.0f;
    private const float runSpeed = 1.5f;

    private Rigidbody rb;

    [SerializeField]
    private CharacterController controller;
    [SerializeField]
    private Animator anim;

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
        anim = GetComponentInChildren<Animator>();
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

            // 방향 부드럽게 전환
            currentInput = Vector2.Lerp(currentInput, input.MoveDirection, inputSmoothSpeed * Runner.DeltaTime);

            // 목표 배속 설정
            float targetSpeedMultiplier = input.IsRunning ? runSpeed : walkSpeed;

            // 배속도 부드럽게 보간
            currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, inputSmoothSpeed * Runner.DeltaTime);

            // 이동 벡터 계산
            Vector3 move = new Vector3(currentInput.x, 0, currentInput.y) * moveSpeed * currentSpeedMultiplier;
            move.y = rb.linearVelocity.y;

            rb.linearVelocity = move;

            if (anim != null)
            {
                anim.SetFloat("MoveX", currentInput.x);
                anim.SetFloat("MoveZ", currentInput.y * currentSpeedMultiplier);
            }
        }
    }

    



}
