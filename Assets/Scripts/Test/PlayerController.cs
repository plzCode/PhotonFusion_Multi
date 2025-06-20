using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static Fusion.NetworkBehaviour;
using static KartInput;

public class PlayerController : NetworkBehaviour
{

    #region PlayerStats
    [Header("�÷��̾� ����")]
    [Networked] public float Hp { get; set; } = 100;
    [Networked] public bool isAlive { get; set; } = true;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private bool isGrounded = true;
    [SerializeField] private float groundCheckDistance;
    [Networked] private Vector2 currentInput { get; set; } = Vector2.zero;
    [Networked] private float Yaw { get; set; }
    [Networked] private float Pitch { get; set; }
    public float inputSmoothSpeed = 5f; // or whatever feels right

    [Networked] private float currentSpeedMultiplier { get; set; } = 1.0f;
    private const float walkSpeed = 1.0f;
    private const float runSpeed = 1.5f;

    #endregion

    #region Weapon
    [Header("���� ����")]
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Transform armTransform;
    [SerializeField] private Vector3 defaultArmPosition; 
    [SerializeField] private Vector3 armTargetPositon = new Vector3(-0.12f,0f,0f);
    [SerializeField] private float zoomLerpSpeed = 10f;

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


    [SerializeField] private float defaultFov = 60f;
    [SerializeField] private float zoomFOV = 50f;
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

        // �ʱ� �� ��ġ�� ����(�Ϲݻ��� ����ġ)
        defaultArmPosition = armTransform.localPosition;


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
        if (!isAlive)
            return;

        // ���
        if (PlayerInputs.IsDown(PlayerInput.NetworkInputData.ButtonFire)||PlayerInputs.IsDownThisFrame(PlayerInput.NetworkInputData.ButtonFire))
        {
            if (weaponManager.ShouldFire())
            {
                weaponManager.Fire(aimPos);

                // ���� �ݵ� ����
                if (PlayerInputs.IsZooming)
                {
                    float zoomRecoilAmount = recoilAmount * 0.66f;
                    Pitch -= zoomRecoilAmount;
                }
                else
                {
                    Pitch -= recoilAmount;
                }                    

                currentRecoverTimer = recoilRecoveryTime;
                currentRecoilOffset = recoilAmount;
                recoilRecoveryPerSecond = recoilAmount / recoilRecoveryTime;

                // �¿� �ݵ� ���� ����
                float yawRecoil = Random.Range(-maxYawRecoil, maxYawRecoil);
                Yaw += yawRecoil;

                currentYawRecoverTimer = recoilRecoveryTime;
                currentYawRecoilOffset = yawRecoil;
                yawRecoveryPerSecond = yawRecoil / recoilRecoveryTime;


                Debug.Log("�����");
                armAnim.SetTrigger("Fire");
                anim.SetTrigger("Fire");
                SoundManager.Instance.Play("RifleFire");
                //armAnim.Play("Fire", 0, 0);
                if (Object.HasInputAuthority)
                {
                    RPC_ZombieHit(Yaw, Pitch);
                }                 
            }
        }

        
        // �� ������ �� �� ��ġ ����
        Vector3 targetPos = defaultArmPosition;
        if (PlayerInputs.IsZooming)
            targetPos += armTargetPositon;

        armTransform.localPosition = Vector3.Lerp(
            armTransform.localPosition,
            targetPos,
            zoomLerpSpeed * Time.deltaTime
        );


        // ī�޶� �� Fov ����
        //float targetFOV = defaultFov;
        float targetFOV = PlayerInputs.IsZooming ? zoomFOV : defaultFov;
        

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFOV,
            zoomLerpSpeed * Time.deltaTime
        );


        UpdateAimPos();
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ZombieHit(float yaw, float pitch)
    {
        TakeDamage(0);

        Vector3 rayOrigin = playerCamera.transform.position; // �Ǵ� �ѱ� ��ġ
        Quaternion aimRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 rayDir = aimRotation * Vector3.forward;

        if (Physics.Raycast(rayOrigin, rayDir, out var hit, 100f, aimLayerMask))
        {
            if (hit.collider.CompareTag("Zombie"))
            {
                // ������ ���� ���� ������Ʈ ���
                var zombie = hit.collider.GetComponent<ZombieController>();
                if (zombie != null)
                {
                    zombie.TakeDamage(weaponManager.damage); // ������ ��ġ�� �ʿ信 ���� ����
                }

                // ��� Ŭ���̾�Ʈ�� �ǰ� ����Ʈ ���� ��û (RPC ȣ��)
                RPC_HitEffect(hit.point);
            }
            

        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HitEffect(Vector3 point)
    {
        Quaternion rotation = Quaternion.identity; // �ʿ�� hit.normal ��� ����
        EffectPoolManager.Instance.GetEffect(point, rotation);
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
        if (!isAlive)
            return;

        HandleAnimation(input);

        HandleMovement(input);

        HandleMouseLook(input);


    }

    void UpdateAimPos()
    {


        Vector3 rayOrigin = playerCamera.transform.position;

        // Yaw, Pitch ���� �̿��� ȸ������ �����
        Quaternion aimRotation = Quaternion.Euler(Pitch, Yaw, 0f);

        // forward ���͸� ���� ����
        Vector3 rayDirection = aimRotation * Vector3.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, aimDistance, aimLayerMask))
        {
            aimPos.position = hit.point;
        }
        else
        {
            aimPos.position = rayOrigin + rayDirection * aimDistance;
        }
    }

    public void TakeDamage(float amount)
    {
        if (HasStateAuthority)
        {
            Hp -= amount;

            if (Hp <= 0)
            {
                Debug.Log("����");
                isAlive = false;

                anim.SetBool("isAlive", isAlive);
                RPC_SetArmAnim("isAliveBool", isAlive);
                rb.linearVelocity = Vector3.zero;

            }
        }
                
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_SetArmAnim(string paramName,bool _bool)
    {
        armAnim.SetBool(paramName, _bool);
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
            anim.SetFloat("MoveZ", input.MoveDirection.y * currentSpeedMultiplier / moveSpeed, 0.15f, Runner.DeltaTime);

            
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

        if (anim != null && !Runner.IsResimulation)
        {
            float normalizedLook = Mathf.InverseLerp(-80f, 80f, Pitch);
            float lookValue = (normalizedLook - 0.5f) * -2f;
            anim.SetFloat("Look", lookValue);
        }


    }



    private void HandleMovement(PlayerInput.NetworkInputData input)
    {


        currentInput = Vector2.Lerp(currentInput, input.MoveDirection, inputSmoothSpeed * Runner.DeltaTime);
        float targetSpeedMultiplier = moveSpeed * (input.IsRunning ? runSpeed : walkSpeed);
        currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, inputSmoothSpeed * Runner.DeltaTime);

        Vector3 moveDir = new Vector3(currentInput.x, 0, currentInput.y);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        moveDir = transform.TransformDirection(moveDir); // ĳ���� ���⿡ ���߱�

        Vector3 targetVelocity = moveDir * currentSpeedMultiplier;
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
        Yaw %= 360f;

        float mouseY = input.LookDirection.y * mouseSensitivity;
        Pitch -= mouseY;

        // �ݵ� ���� ó��
        ApplyRecoilRecovery();

        // Ŭ������ �������� �� ����
        Pitch = Mathf.Clamp(Pitch, -80f, 80f);

        // ȸ�� ����
        Quaternion targetRot = Quaternion.Euler(0, Yaw, 0);
        rb.MoveRotation(targetRot);

        cameraHolder.localEulerAngles = new Vector3(Pitch, 0, 0);



        //if (Object.HasInputAuthority&&!Runner.IsResimulation)
        //{
        //    float mouseY = input.LookDirection.y * mouseSensitivity;
        //    verticalLookRotation -= mouseY;
        //    verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);
        //    cameraHolder.localEulerAngles = new Vector3(verticalLookRotation, 0, 0);
        //}
    }
    //���� �ݵ�
    [SerializeField] private float recoilAmount = 2f;
    [SerializeField] private float recoilRecoveryTime = 0.35f;

    private float currentRecoverTimer = 0f;
    private float currentRecoilOffset = 0f;
    private float recoilRecoveryPerSecond = 0f;

    //�¿� �ݵ�
    [SerializeField] private float maxYawRecoil = 1f; // ��: �¿� �ִ� �ݵ�

    private float currentYawRecoilOffset = 0f;
    private float currentYawRecoverTimer = 0f;
    private float yawRecoveryPerSecond = 0f;

    private void ApplyRecoilRecovery()
    {
        if (currentRecoverTimer > 0f)
        {
            float recoverStep = recoilRecoveryPerSecond * Runner.DeltaTime;
            Pitch += recoverStep;
            currentRecoverTimer -= Runner.DeltaTime;

            if (currentRecoverTimer <= 0f)
            {
                currentRecoilOffset = 0f;
                recoilRecoveryPerSecond = 0f;
            }
        }

        // �¿�(Yaw) ����
        if (currentYawRecoverTimer > 0f)
        {
            float recoverStep = yawRecoveryPerSecond * Runner.DeltaTime;
            Yaw -= recoverStep;
            currentYawRecoverTimer -= Runner.DeltaTime;
            if (currentYawRecoverTimer <= 0f)
            {
                currentYawRecoilOffset = 0f;
                yawRecoveryPerSecond = 0f;
            }
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
