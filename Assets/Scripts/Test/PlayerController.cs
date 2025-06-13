using Fusion;
using UnityEngine;
using static Fusion.NetworkBehaviour;
using static KartInput;

public class PlayerController : NetworkBehaviour
{

    #region PlayerStats
    [Header("플레이어 스탯")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private bool isGrounded = true;
    [SerializeField] private float groundCheckDistance;
    private Vector2 currentInput = Vector2.zero;
    public float inputSmoothSpeed = 5f; // or whatever feels right

    private float currentSpeedMultiplier = 1.0f;
    private const float walkSpeed = 1.0f;
    private const float runSpeed = 1.5f;

    #endregion

    #region Components
    
    private Rigidbody rb;
    [Header("컴포넌트")]
    [SerializeField]
    private CharacterController controller;
    [SerializeField]
    private Animator anim;
    #endregion
    
    #region PlayerCamera
    [Header("카메라,민감도")]
    //카메라 
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private Transform cameraHolder; // 상하 회전용
    private float verticalLookRotation = 0f;
    #endregion

    #region Gravity
    [Header("중력,중력가속도")]
    public float customGravity = -9.81f;
    public float fallMultiplier = 2.0f;
    #endregion

    #region NetworkComponents

    [Header("")]
    [Networked] public RoomPlayer RoomUser { get; set; }


    //[Networked] public float AppliedSpeed { get; set; }
    //[Networked] public float MaxSpeed { get; set; }
    //[Networked] private KartInput.NetworkInputData Inputs { get; set; }
    [Networked] private PlayerInput.NetworkInputData PlayerInputs { get; set; }

    #endregion

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
        if (Object.HasInputAuthority)
        {
            //마우스 락
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // MainCamera가 이 플레이어 오브젝트 안에 있을 경우
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = true;
        }
        else
        {
            // 다른 플레이어는 카메라 끔
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = false;
        }

        Runner.SetIsSimulated(Object, true);
    }
    private void Update()
    {
        
    }
    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        if (GetInput(out PlayerInput.NetworkInputData input))
        {
            HandleInput(input);
        }

        ApplyGravity();

        CheckGrounded();
    }

    private void HandleInput(PlayerInput.NetworkInputData input)
    {
        HandleAnimation(input);

        HandleMovement(input);

        HandleMouseLook(input);
    }

    private void ApplyGravity()
    {
        // 중력 처리 (StateAuthority만 담당)
        if (HasStateAuthority && !isGrounded)
        {
            Vector3 gravity = Vector3.up * customGravity;
            if (rb.linearVelocity.y < 0)
                gravity *= fallMultiplier;

            rb.AddForce(gravity, ForceMode.Acceleration);
        }
    }

    private void HandleAnimation(PlayerInput.NetworkInputData input)
    {
        //  클라이언트 (InputAuthority)용 애니메이션 처리
        if (anim != null && !Runner.IsResimulation)
        {
            anim.SetFloat("MoveX", input.MoveDirection.x, 0.15f, Runner.DeltaTime);
            anim.SetFloat("MoveZ", input.MoveDirection.y * currentSpeedMultiplier, 0.15f, Runner.DeltaTime);

            if (input.JumpPressed && isGrounded)
                anim.SetTrigger("Jump");
        }
    }

    private void HandleMovement(PlayerInput.NetworkInputData input)
    {
        //  서버/호스트 (StateAuthority)만 실제 이동 처리
        if (Object.HasStateAuthority && !Runner.IsResimulation)
        {
            currentInput = Vector2.Lerp(currentInput, input.MoveDirection, inputSmoothSpeed * Runner.DeltaTime);
            float targetSpeedMultiplier = input.IsRunning ? runSpeed : walkSpeed;
            currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, inputSmoothSpeed * Runner.DeltaTime);

            Vector3 moveDir = new Vector3(currentInput.x, 0, currentInput.y);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            moveDir = transform.TransformDirection(moveDir); // 캐릭터 방향에 맞추기

            Vector3 targetVelocity = moveDir * moveSpeed * currentSpeedMultiplier;
            targetVelocity.y = rb.linearVelocity.y;

            if (input.JumpPressed && isGrounded)
                targetVelocity.y = jumpForce;

            rb.linearVelocity = targetVelocity;
        }
    }

    private void HandleMouseLook(PlayerInput.NetworkInputData input)
    {
        if (Object.HasStateAuthority)
        {
            float mouseX = input.LookDirection.x * mouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);
        }


        if (Object.HasInputAuthority)
        {
            float mouseY = input.LookDirection.y * mouseSensitivity;
            verticalLookRotation -= mouseY;
            verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);
            cameraHolder.localEulerAngles = new Vector3(verticalLookRotation, 0, 0);
        }
    }

    

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, ~0);
        if (anim != null)
            anim.SetBool("IsGrounded", isGrounded);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;

        Vector3 origin = transform.position;
        Vector3 direction = Vector3.down * groundCheckDistance;

        Gizmos.DrawLine(origin, origin + direction);
        Gizmos.DrawSphere(origin + direction, 0.05f); // 끝점에 작은 점도 표시
    }

}
