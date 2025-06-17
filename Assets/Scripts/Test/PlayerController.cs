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
    [Networked] private Vector2 currentInput { get; set; } = Vector2.zero;
    [Networked] private float Yaw { get; set; }
    public float inputSmoothSpeed = 5f; // or whatever feels right

    [Networked] private float currentSpeedMultiplier { get; set; } = 1.0f;
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
    [SerializeField]
    private Animator armAnim;
    [SerializeField]
    private SkinnedMeshRenderer headRenderer;
    [SerializeField]
    private SkinnedMeshRenderer bodyRenderer;
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


            // 3인칭 캐릭터 렌더링 그림자만 하기
            headRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            bodyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

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
            PlayerInputs = input;
            HandleInput(PlayerInputs);
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
            anim.SetFloat("MoveZ", input.MoveDirection.y * currentSpeedMultiplier/moveSpeed, 0.15f, Runner.DeltaTime);
            

            if (input.JumpPressed && isGrounded)
                anim.SetTrigger("Jump");
        }

        if (armAnim != null && !Runner.IsResimulation)
        {
            // 1) Rigidbody 기반 실제 속도를 사용
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float speed = horizontalVel.magnitude; // 단위: m/s

            // (선택) 최대 달리기 속도로 정규화
            float normalizedSpeed = speed / (moveSpeed * runSpeed);

            armAnim.SetFloat("Speed", normalizedSpeed, 0.1f, Runner.DeltaTime);
        }
    }

    private void HandleMovement(PlayerInput.NetworkInputData input)
    {
        
        
            currentInput = Vector2.Lerp(currentInput, input.MoveDirection, inputSmoothSpeed * Runner.DeltaTime);
            float targetSpeedMultiplier = moveSpeed *(input.IsRunning ? runSpeed : walkSpeed);
            currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, inputSmoothSpeed * Runner.DeltaTime);

            Vector3 moveDir = new Vector3(currentInput.x, 0, currentInput.y);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            moveDir = transform.TransformDirection(moveDir); // 캐릭터 방향에 맞추기

            Vector3 targetVelocity = moveDir  * currentSpeedMultiplier;
            targetVelocity.y = rb.linearVelocity.y;

            if (input.JumpPressed && isGrounded)
                targetVelocity.y = jumpForce;

            rb.linearVelocity = targetVelocity;
        
    }

    private void HandleMouseLook(PlayerInput.NetworkInputData input)
    {
        // 마우스 회전 처리 (StateAuthority → 예측 가능하게 Networked 값으로 처리)
        float mouseX = input.LookDirection.x * mouseSensitivity;
        Yaw += mouseX;
        Yaw %= 360f; // 회전값 유지

        // 회전 적용
        Quaternion targetRot = Quaternion.Euler(0, Yaw, 0);
        rb.MoveRotation(targetRot); // 20f는 회전 부드러움 조절


        if (Object.HasInputAuthority&&!Runner.IsResimulation)
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
