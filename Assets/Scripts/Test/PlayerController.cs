using Fusion;
using UnityEngine;
using UnityEngine.Windows;
using static Fusion.NetworkBehaviour;
using static KartInput;

public class PlayerController : NetworkBehaviour
{

    #region PlayerStats
    [Header("플레이어 스탯")]
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
    [Header("무기 정보")]
    [SerializeField] WeaponManager weaponManager;

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
    private SkinnedMeshRenderer[] bodyCasting;
    [SerializeField]
    private MeshRenderer[] rifleCasting;
    [SerializeField]
    private SkinnedMeshRenderer[] fpsBodyCasting;
    #endregion

    #region PlayerCamera
    [Header("카메라,민감도")]
    //카메라 
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private Transform cameraHolder; // 상하 회전용
    [SerializeField] private Transform aimPos; // 조준점(빈 오브젝트)
    [SerializeField] private Camera playerCamera; // 플레이어 카메라
    [SerializeField] private float aimDistance = 100f; // 조준 최대 거리
    [SerializeField] private LayerMask aimLayerMask = ~0; // 조준에 사용할 레이어
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
            // 다른 플레이어는 카메라 끔
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = false;
            // 다른 플레이어의 1인칭 바디 렌더링 끄기
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

        if (PlayerInputs.IsDown(PlayerInput.NetworkInputData.ButtonFire)||PlayerInputs.IsDownThisFrame(PlayerInput.NetworkInputData.ButtonFire))
        {
            if (weaponManager.ShouldFire())
            {
                weaponManager.Fire(aimPos);
                Debug.Log("쏘는중");
                armAnim.SetTrigger("Fire");
                SoundManager.Instance.Play("RifleFire");
                //armAnim.Play("Fire", 0, 0);
                if (Object.HasInputAuthority)
                {
                    RPC_ZombieHit(Yaw, Pitch);
                }
                    
                

            }
        }

        UpdateAimPos();
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ZombieHit(float yaw, float pitch)
    {
        TakeDamage(2);

        Vector3 rayOrigin = playerCamera.transform.position; // 또는 총구 위치
        Quaternion aimRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 rayDir = aimRotation * Vector3.forward;

        if (Physics.Raycast(rayOrigin, rayDir, out var hit, 100f, aimLayerMask))
        {
            if (hit.collider.CompareTag("Zombie"))
            {
                //// 서버가 맞은 좀비 컴포넌트 얻기
                //var zombie = hit.collider.GetComponent<ZombieHealth>();
                //if (zombie != null)
                //{
                //    zombie.TakeDamage(10); // 데미지 수치는 필요에 따라 조절
                //}

                // 모든 클라이언트에 피격 이펙트 실행 요청 (RPC 호출)
                RPC_HitEffect(hit.point);
            }
            

        }
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_HitEffect(Vector3 point)
    {
        Quaternion rotation = Quaternion.identity; // 필요시 hit.normal 사용 가능
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

        // Yaw, Pitch 값을 이용해 회전값을 만들고
        Quaternion aimRotation = Quaternion.Euler(Pitch, Yaw, 0f);

        // forward 벡터를 직접 구함
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
                Debug.Log("죽음");
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
            anim.SetFloat("MoveZ", input.MoveDirection.y * currentSpeedMultiplier / moveSpeed, 0.15f, Runner.DeltaTime);


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
        float targetSpeedMultiplier = moveSpeed * (input.IsRunning ? runSpeed : walkSpeed);
        currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, inputSmoothSpeed * Runner.DeltaTime);

        Vector3 moveDir = new Vector3(currentInput.x, 0, currentInput.y);
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        moveDir = transform.TransformDirection(moveDir); // 캐릭터 방향에 맞추기

        Vector3 targetVelocity = moveDir * currentSpeedMultiplier;
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


        float mouseY = input.LookDirection.y * mouseSensitivity;
        Pitch -= mouseY;
        Pitch = Mathf.Clamp(Pitch, -80f, 80f); // Networked 값으로도 클램핑 필요            

        cameraHolder.localEulerAngles = new Vector3(Pitch, 0, 0);



        //if (Object.HasInputAuthority&&!Runner.IsResimulation)
        //{
        //    float mouseY = input.LookDirection.y * mouseSensitivity;
        //    verticalLookRotation -= mouseY;
        //    verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);
        //    cameraHolder.localEulerAngles = new Vector3(verticalLookRotation, 0, 0);
        //}
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
