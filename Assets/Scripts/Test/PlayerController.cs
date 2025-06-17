using Fusion;
using UnityEngine;
using UnityEngine.Windows;
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
    [Networked] private Vector2 currentInput { get; set; } = Vector2.zero;
    [Networked] private float Yaw { get; set; }
    public float inputSmoothSpeed = 5f; // or whatever feels right

    [Networked] private float currentSpeedMultiplier { get; set; } = 1.0f;
    private const float walkSpeed = 1.0f;
    private const float runSpeed = 1.5f;

    #endregion

    #region Weapon
    [Header("���� ����")]
    [SerializeField] WeaponManager weaponManager;

    #endregion

    #region Components

    private Rigidbody rb;
    [Header("������Ʈ")]
    [SerializeField]
    private CharacterController controller;
    [SerializeField]
    private Animator anim;
    [SerializeField]
    private Animator armAnim;
    [SerializeField]
    private SkinnedMeshRenderer[] bodyCasting;
    [SerializeField]
    private MeshRenderer[] rifleCasting;
    [SerializeField]
    private SkinnedMeshRenderer[] fpsBodyCasting;
    #endregion
    
    #region PlayerCamera
    [Header("ī�޶�,�ΰ���")]
    //ī�޶� 
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private Transform cameraHolder; // ���� ȸ����
    [SerializeField] private Transform aimPos; // ������(�� ������Ʈ)
    [SerializeField] private Camera playerCamera; // �÷��̾� ī�޶�
    [SerializeField] private float aimDistance = 100f; // ���� �ִ� �Ÿ�
    [SerializeField] private LayerMask aimLayerMask = ~0; // ���ؿ� ����� ���̾�
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


            // 3��Ī ĳ���� ������ �׸��ڸ� �ϱ�
            for (int i = 0; i < bodyCasting.Length; i++)
            {
                bodyCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
            for (int i = 0; i < rifleCasting.Length; i++)
            {
                rifleCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            

        }
        else
        {
            // �ٸ� �÷��̾�� ī�޶� ��
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = false;
            // �ٸ� �÷��̾��� 1��Ī �ٵ� ������ ����
            for (int i = 0; i < fpsBodyCasting.Length; i++)
            {
                fpsBodyCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

        }

        Runner.SetIsSimulated(Object, true);
    }
    private void Update()
    {
        if (PlayerInputs.IsDown(PlayerInput.NetworkInputData.ButtonFire))
        {
            if (weaponManager.ShouldFire())
            {
                weaponManager.Fire(aimPos);
                Debug.Log("�����");
            }
        }

        UpdateAimPos();
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

    void UpdateAimPos()
    {
        

        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, aimDistance, aimLayerMask))
        {
            aimPos.position = hit.point; // �浹�� ��
        }
        else
        {
            aimPos.position = rayOrigin + rayDirection * aimDistance; // �ִ� �Ÿ�
        }
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
            anim.SetFloat("MoveZ", input.MoveDirection.y * currentSpeedMultiplier/moveSpeed, 0.15f, Runner.DeltaTime);
            

            if (input.JumpPressed && isGrounded)
                anim.SetTrigger("Jump");
        }

        if (armAnim != null && !Runner.IsResimulation)
        {
            // 1) Rigidbody ��� ���� �ӵ��� ���
            Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float speed = horizontalVel.magnitude; // ����: m/s

            // (����) �ִ� �޸��� �ӵ��� ����ȭ
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

            moveDir = transform.TransformDirection(moveDir); // ĳ���� ���⿡ ���߱�

            Vector3 targetVelocity = moveDir  * currentSpeedMultiplier;
            targetVelocity.y = rb.linearVelocity.y;

            if (input.JumpPressed && isGrounded)
                targetVelocity.y = jumpForce;

            rb.linearVelocity = targetVelocity;
        
    }

    private void HandleMouseLook(PlayerInput.NetworkInputData input)
    {
        // ���콺 ȸ�� ó�� (StateAuthority �� ���� �����ϰ� Networked ������ ó��)
        float mouseX = input.LookDirection.x * mouseSensitivity;
        Yaw += mouseX;
        Yaw %= 360f; // ȸ���� ����

        // ȸ�� ����
        Quaternion targetRot = Quaternion.Euler(0, Yaw, 0);
        rb.MoveRotation(targetRot); // 20f�� ȸ�� �ε巯�� ����


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
        Gizmos.DrawSphere(origin + direction, 0.05f); // ������ ���� ���� ǥ��
    }

}
