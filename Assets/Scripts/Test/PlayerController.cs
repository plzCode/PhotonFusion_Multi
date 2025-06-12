using Fusion;
using UnityEngine;
using static Fusion.NetworkBehaviour;
using static KartInput;

public class PlayerController : NetworkBehaviour
{

    #region PlayerStats
    [Header("�÷��̾� ����")]
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
    [Header("������Ʈ")]
    [SerializeField]
    private CharacterController controller;
    [SerializeField]
    private Animator anim;
    #endregion
    
    #region PlayerCamera
    [Header("ī�޶�,�ΰ���")]
    //ī�޶� 
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private Transform cameraHolder; // ���� ȸ����
    private float verticalLookRotation = 0f;
    #endregion

    #region Gravity
    [Header("�߷�,�߷°��ӵ�")]
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
            //���콺 ��
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // MainCamera�� �� �÷��̾� ������Ʈ �ȿ� ���� ���
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = true;
        }
        else
        {
            // �ٸ� �÷��̾�� ī�޶� ��
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
        // �߷� ó�� (StateAuthority�� ���)
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
        //  Ŭ���̾�Ʈ (InputAuthority)�� �ִϸ��̼� ó��
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
        //  ����/ȣ��Ʈ (StateAuthority)�� ���� �̵� ó��
        if (Object.HasStateAuthority && !Runner.IsResimulation)
        {
            currentInput = Vector2.Lerp(currentInput, input.MoveDirection, inputSmoothSpeed * Runner.DeltaTime);
            float targetSpeedMultiplier = input.IsRunning ? runSpeed : walkSpeed;
            currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, inputSmoothSpeed * Runner.DeltaTime);

            Vector3 moveDir = new Vector3(currentInput.x, 0, currentInput.y);
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

            moveDir = transform.TransformDirection(moveDir); // ĳ���� ���⿡ ���߱�

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
        Gizmos.DrawSphere(origin + direction, 0.05f); // ������ ���� ���� ǥ��
    }

}
