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
        Runner.SetIsSimulated(Object, true);
    }
    private void Update()
    {
        //if (anim != null)
        //{
        //    anim.SetFloat("MoveX", currentInput.x, 0.1f, Time.deltaTime);
        //    anim.SetFloat("MoveZ", currentInput.y * currentSpeedMultiplier, 0.1f, Time.deltaTime);
        //}
    }
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (GetInput(out PlayerInput.NetworkInputData input))
        {
            // 입력 방향 부드럽게 보간
            currentInput = Vector2.Lerp(currentInput, input.MoveDirection, inputSmoothSpeed * Runner.DeltaTime);

            // 목표 속도 배율 보간
            float targetSpeedMultiplier = input.IsRunning ? runSpeed : walkSpeed;
            currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, inputSmoothSpeed * Runner.DeltaTime);

            // 이동 방향과 속도 계산
            Vector3 moveDir = new Vector3(currentInput.x, 0, currentInput.y);
            if (moveDir.sqrMagnitude > 1f)
                moveDir.Normalize();

            Vector3 targetVelocity = moveDir * moveSpeed * currentSpeedMultiplier;
            targetVelocity.y = rb.linearVelocity.y;  // 중력 등 수직 속도 유지

            rb.linearVelocity = targetVelocity;

            
            if (anim != null)
            {
                anim.SetFloat("MoveX", currentInput.x, 0.1f, Runner.DeltaTime);
                anim.SetFloat("MoveZ", currentInput.y * currentSpeedMultiplier, 0.1f, Runner.DeltaTime);
            }
        }
    }

    



}
