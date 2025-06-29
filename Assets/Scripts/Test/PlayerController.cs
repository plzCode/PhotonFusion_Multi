using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
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
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Transform armTransform;
    [SerializeField] private Transform fpsMuzzleTransform;
    [SerializeField] private Vector3 defaultArmPosition; 
    [SerializeField] private Vector3 armTargetPositon = new Vector3(-0.12f,0f,0f);
    [SerializeField] private float zoomLerpSpeed = 10f;

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
    public SkinnedMeshRenderer[] bodyCasting;
    [SerializeField]
    public MeshRenderer[] rifleCasting;
    [SerializeField]
    public SkinnedMeshRenderer[] fpsBodyCasting;
    #endregion

    #region PlayerCamera
    [Header("카메라,민감도")]
    //카메라 
    [SerializeField] private float mouseSensitivity = 0.5f;
    [SerializeField] private Transform cameraHolder; // 상하 회전용
    [Networked] private Vector3 cameraVec { get; set; }
    [SerializeField] private Transform aimPos; // 조준점(빈 오브젝트)
    [SerializeField] public Camera playerCamera; // 플레이어 카메라
    [SerializeField] private float aimDistance = 100f; // 조준 최대 거리
    [SerializeField] private LayerMask aimLayerMask = ~0; // 조준에 사용할 레이어
    private float verticalLookRotation = 0f;


    [SerializeField] private float defaultFov = 60f;
    [SerializeField] private float zoomFOV = 50f;
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

    [SerializeField] public CharacterHUDUnit characterHUDUnit;
    [SerializeField] private Sprite playerImage;
    private float localPitch; // 실제 카메라에 적용할 값

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

        if (GameManager.Instance != null)
            GameManager.RegisterPlayer(this.Object);

        // 초기 팔 위치값 저장(일반상태 팔위치)
        defaultArmPosition = armTransform.localPosition;


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
            {
                cam.enabled = false;
            }

            // 다른 플레이어의 1인칭 바디 렌더링 끄기
            for (int i = 0; i < fpsBodyCasting.Length; i++)
            {
                fpsBodyCasting[i].enabled=false;
            }
            

        }

        Runner.SetIsSimulated(Object, true);
    }
    private void Update()
    {
        if (!isAlive)
        {
            
            if (HasInputAuthority && spectatePlayers.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    ChangeCamera();   
                }
            }
        }
        else
        {
            // 사격
            if (PlayerInputs.IsDown(PlayerInput.NetworkInputData.ButtonFire) || PlayerInputs.IsDownThisFrame(PlayerInput.NetworkInputData.ButtonFire))
            {
                if (weaponManager.ShouldFire())
                {
                    bool isOwner = false;
                    if (HasInputAuthority || GameManager.Instance.observerPlayer == this) // 플레이어 자기자신이거나 관전중인 대상이면
                    {
                        isOwner = true;
                        GameObject flash = Instantiate(weaponManager.muzzleFlash, fpsMuzzleTransform.position, fpsMuzzleTransform.rotation, fpsMuzzleTransform);
                    }
                    weaponManager.Fire(aimPos, isOwner, fpsMuzzleTransform);

                    // 상하 반동 적용
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

                    // 좌우 반동 랜덤 적용
                    float yawRecoil = Random.Range(-maxYawRecoil, maxYawRecoil);
                    Yaw += yawRecoil;

                    currentYawRecoverTimer = recoilRecoveryTime;
                    currentYawRecoilOffset = yawRecoil;
                    yawRecoveryPerSecond = yawRecoil / recoilRecoveryTime;


                    Debug.Log("쏘는중");
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


            // 줌 상태일 때 팔 위치 보간
            Vector3 targetPos = defaultArmPosition;
            if (PlayerInputs.IsZooming)
                targetPos += armTargetPositon;

            armTransform.localPosition = Vector3.Lerp(
                armTransform.localPosition,
                targetPos,
                zoomLerpSpeed * Time.deltaTime
            );


            // 카메라 줌 Fov 보간
            //float targetFOV = defaultFov;
            float targetFOV = PlayerInputs.IsZooming ? zoomFOV : defaultFov;


            playerCamera.fieldOfView = Mathf.Lerp(
                playerCamera.fieldOfView,
                targetFOV,
                zoomLerpSpeed * Time.deltaTime
            );


            UpdateAimPos();
        }


        localPitch = Mathf.Lerp(localPitch, Pitch, 150f * Time.deltaTime); // 10f는 원하는 반응속도
        cameraHolder.localEulerAngles = new Vector3(localPitch, 0, 0);



    }

    public void ChangeCamera()
    {
        // 다음 인덱스 순환
        int nextIndex = (currentSpectateIndex + 1) % spectatePlayers.Count;
        PlayerController nextPlayer = spectatePlayers[nextIndex];
        PlayerController currentPlayer = spectatePlayers[currentSpectateIndex];
        GameManager.Instance.observerPlayer = nextPlayer;
        // 현재 카메라 끈다
        currentPlayer.playerCamera.enabled = false;

        // 현재 캐릭터 1인칭 Off, 3인칭 On
        for (int i = 0; i < currentPlayer.fpsBodyCasting.Length; i++)
        {
            currentPlayer.fpsBodyCasting[i].enabled = false;
        }
        for (int i = 0; i < currentPlayer.bodyCasting.Length; i++)
        {
            currentPlayer.bodyCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        for (int i = 0; i < currentPlayer.rifleCasting.Length; i++)
        {
            currentPlayer.rifleCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }


        // 다음 카메라 먼저 켠다
        nextPlayer.playerCamera.enabled = true;

        // 다음 관전 플레이어 3인칭 끄고 1인칭 키기
        for (int i = 0; i < nextPlayer.fpsBodyCasting.Length; i++)
        {
            spectatePlayers[nextIndex].fpsBodyCasting[i].enabled = true;
        }
        for (int i = 0; i < nextPlayer.bodyCasting.Length; i++)
        {
            nextPlayer.bodyCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }

        for (int i = 0; i < nextPlayer.rifleCasting.Length; i++)
        {
            nextPlayer.rifleCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }

        currentSpectateIndex = nextIndex;
    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_ZombieHit(float yaw, float pitch)
    {
        TakeDamage(10);

        Vector3 rayOrigin = playerCamera.transform.position; // 또는 총구 위치
        Quaternion aimRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 rayDir = aimRotation * Vector3.forward;

        if (Physics.Raycast(rayOrigin, rayDir, out var hit, 100f, aimLayerMask))
        {
            if (hit.collider.CompareTag("Zombie"))
            {
                // 서버가 맞은 좀비 컴포넌트 얻기
                var zombie = hit.collider.GetComponent<ZombieController>();
                if (zombie != null)
                {
                    zombie.RPC_RequestDamage(weaponManager.damage); // 데미지 수치는 필요에 따라 조절
                }

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

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SetPlayerUI()
    {       
    
        if (Object.HasInputAuthority)
        {
            // 스탯 UI 연결 ( 자신 )
            characterHUDUnit = InterfaceManager.Instance.characterHUDcontainter.MyPlayerStatus_UI;
            characterHUDUnit.SetName($"{RoomUser.Username}");
            characterHUDUnit.SetPortraitImage(playerImage);
            RPC_ChangeHealth(Hp);
            characterHUDUnit.gameObject.SetActive(true);

        }
        else
        {
            // 스탯 UI 연결 ( 다른 플레이어 )
            for (int i = 0; i < 3; i++)
            {
                if (InterfaceManager.Instance.characterHUDUnits[i].player == null || InterfaceManager.Instance.characterHUDUnits[i].playerName == RoomUser.Username)
                {
                    InterfaceManager.Instance.characterHUDUnits[i].player = this;
                    characterHUDUnit = InterfaceManager.Instance.characterHUDUnits[i];
                    characterHUDUnit.SetName($"{RoomUser.Username}");
                    characterHUDUnit.SetPortraitImage(playerImage);
                    RPC_ChangeHealth(Hp);
                    characterHUDUnit.gameObject.SetActive(true);
                    break;
                }
            }

        
        }
    }


    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();

        

        if (GetInput(out PlayerInput.NetworkInputData input))
        {
            PlayerInputs = input;

            HandleInput(PlayerInputs);
        }

        cameraHolder.localEulerAngles = new Vector3(Pitch, 0, 0);

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

                if (!GameManager.Instance.PlayerAliveCheck())
                {
                    return;
                }

                RPC_SetAnim("isAlive", isAlive);
                RPC_SetArmAnim("isAliveBool", isAlive);
                rb.linearVelocity = Vector3.zero;
                RPC_cameraOnOff();
                
                /*if (GameManager.Instance.PlayerAliveCheck_Bool())
                {
                    SpawnScript.Current.ReSpawn();
                }*/

            }

            RPC_ChangeHealth(Hp);
            
        }
                
    }

    // 관전 대상 플레이어 리스트
    [SerializeField] private List<PlayerController> spectatePlayers = new List<PlayerController>();

    

    // 현재 바라보는 플레이어 인덱스
    private int currentSpectateIndex = 0;


    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_cameraOnOff()
    {
        // 관전 리스트 초기화
        spectatePlayers.Clear();

        foreach (var p in GameManager.Players)
        {
            var other = p.GetComponent<PlayerController>();
            if (other != null )// other.isAlive
            {
                spectatePlayers.Add(other);
            }
        }

        

        // 관전할 대상이 있으면 첫번째만 켜기
        if (spectatePlayers.Count > 0)
        {
            // 내 카메라 끄기
            playerCamera.enabled = false;

            // 자신의 3인칭 캐릭터 렌더링 키기
            for (int i = 0; i < bodyCasting.Length; i++)
            {
                bodyCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
            for (int i = 0; i < rifleCasting.Length; i++)
            {
                rifleCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            // 자신의 1인칭 바디 렌더링 끄기
            for (int i = 0; i < fpsBodyCasting.Length; i++)
            {
                fpsBodyCasting[i].enabled = false;
            }

            currentSpectateIndex = 0;
            spectatePlayers[currentSpectateIndex].playerCamera.enabled = true;
            PlayerController nextPlayer = spectatePlayers[currentSpectateIndex];
            GameManager.Instance.observerPlayer = nextPlayer;

            // 관전 플레이어 3인칭 끄고 1인칭 키기
            for (int i = 0; i < nextPlayer.fpsBodyCasting.Length; i++)
            {
                nextPlayer.fpsBodyCasting[i].enabled = true;
            }
            for (int i = 0; i < nextPlayer.bodyCasting.Length; i++)
            {
                nextPlayer.bodyCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                Debug.Log(nextPlayer+ "ㅇ" + nextPlayer.bodyCasting[i] + "현재 " + i + "번째");
            }

            for (int i = 0; i < nextPlayer.rifleCasting.Length; i++)
            {
                nextPlayer.rifleCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }

        
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.InputAuthority)]
    private void RPC_cameraON()
    {
        // 자신의 카메라 다시 켜기
        playerCamera.enabled = true;

        // 자신의 3인칭 캐릭터 렌더링 끄기
        for (int i = 0; i < bodyCasting.Length; i++)
        {
            bodyCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }
        for (int i = 0; i < rifleCasting.Length; i++)
        {
            rifleCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        }

        // 자신의 1인칭 바디 렌더링 다시 켜기
        for (int i = 0; i < fpsBodyCasting.Length; i++)
        {
            fpsBodyCasting[i].enabled = true;
        }

        // 이전에 관전하던 대상의 카메라/렌더링 끄기
        if (spectatePlayers.Count > 0 && currentSpectateIndex >= 0 && currentSpectateIndex < spectatePlayers.Count)
        {
            PlayerController spectatedPlayer = spectatePlayers[currentSpectateIndex];

            // 관전 대상의 카메라 끄기
            if (spectatedPlayer != null && spectatedPlayer.playerCamera != null)
            {
                spectatedPlayer.playerCamera.enabled = false;
            }

            // 관전 대상의 1인칭 바디 끄기
            for (int i = 0; i < spectatedPlayer.fpsBodyCasting.Length; i++)
            {
                spectatedPlayer.fpsBodyCasting[i].enabled = false;
            }

            // 관전 대상의 3인칭 바디 다시 보이도록 설정
            for (int i = 0; i < spectatedPlayer.bodyCasting.Length; i++)
            {
                spectatedPlayer.bodyCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }

            for (int i = 0; i < spectatedPlayer.rifleCasting.Length; i++)
            {
                spectatedPlayer.rifleCasting[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }

        // 관전자 관련 변수 초기화
        spectatePlayers.Clear();
        currentSpectateIndex = -1;
        GameManager.Instance.observerPlayer = null;
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ChangeHealth(float hp)
    {
        characterHUDUnit.OnChangeHealth(Hp);
    }


    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetArmAnim(string paramName,bool _bool)
    {
        armAnim.SetBool(paramName, _bool);
    }
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SetAnim(string paramName, bool _bool)
    {
        anim.SetBool(paramName, _bool);
    }


    [SerializeField] private float stickToGroundForce = 30f; // 접지 유지 힘
    private void ApplyGravity()
    {
        // 중력 처리 (StateAuthority만 담당)
        if (HasStateAuthority )
        {
            if (isGrounded)
            {
                

                // 살짝 바닥으로 누르는 힘
                rb.AddForce(Vector3.down * stickToGroundForce, ForceMode.Acceleration);
            }
            else
            {
                // 일반 중력
                Vector3 gravity = Vector3.up * customGravity;
                if (rb.linearVelocity.y < 0)
                    gravity *= fallMultiplier;

                rb.AddForce(gravity, ForceMode.Acceleration);
            }

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
        Yaw %= 360f;

        float mouseY = input.LookDirection.y * mouseSensitivity;
        Pitch -= mouseY;

        // 반동 복구 처리
        ApplyRecoilRecovery();

        // 클램핑은 마지막에 한 번만
        Pitch = Mathf.Clamp(Pitch, -80f, 80f);

        // 회전 적용
        Quaternion targetRot = Quaternion.Euler(0, Yaw, 0);
        rb.MoveRotation(targetRot);

        
        



        //if (Object.HasInputAuthority&&!Runner.IsResimulation)
        //{
        //    float mouseY = input.LookDirection.y * mouseSensitivity;
        //    verticalLookRotation -= mouseY;
        //    verticalLookRotation = Mathf.Clamp(verticalLookRotation, -80f, 80f);
        //    cameraHolder.localEulerAngles = new Vector3(verticalLookRotation, 0, 0);
        //}
    }


    //상하 반동
    [SerializeField] private float recoilAmount = 2f;
    [SerializeField] private float recoilRecoveryTime = 0.35f;

    private float currentRecoverTimer = 0f;
    private float currentRecoilOffset = 0f;
    private float recoilRecoveryPerSecond = 0f;

    //좌우 반동
    [SerializeField] private float maxYawRecoil = 1f; // 예: 좌우 최대 반동

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

        // 좌우(Yaw) 복구
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
    private Vector3 groundedHitPoint;

    private void CheckGrounded()
    {
        // 캡슐 모양의 접지 검사 (CapsuleCollider처럼)
        float capsuleRadius = 0.35f; // 발바닥 너비, CapsuleCollider 반지름과 맞추면 좋음
        float capsuleHeight = 1.8f; // 캐릭터 높이
        float checkDistance = 0.2f; // 바닥과의 거리 허용치

        // 캡슐의 상단, 하단 구 중심 위치
        Vector3 point1 = transform.position + Vector3.up * (capsuleHeight * 0.5f - capsuleRadius);
        Vector3 point2 = transform.position + Vector3.up * capsuleRadius;

        if (Physics.CapsuleCast(
                point1,
                point2,
                capsuleRadius,
                Vector3.down,
                out RaycastHit hit,
                checkDistance + 0.01f, // 약간의 여유
                ~0
            ))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        // 애니메이션에도 연동
        if (anim != null)
            anim.SetBool("IsGrounded", isGrounded);

        // 디버그용 Gizmo
        groundedHitPoint = hit.point; // for debug drawing
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;

        float capsuleRadius = 0.35f;
        float capsuleHeight = 1.8f;
        float checkDistance = 0.2f;

        Vector3 point1 = transform.position + Vector3.up * (capsuleHeight * 0.5f - capsuleRadius);
        Vector3 point2 = transform.position + Vector3.up * capsuleRadius;

        // 기본 캡슐
        Gizmos.DrawWireSphere(point1, capsuleRadius);
        Gizmos.DrawWireSphere(point2, capsuleRadius);

        // CapsuleCast 검사 영역을 추가로 시각화
        Vector3 castDirection = Vector3.down * checkDistance;

        // 끝지점 (checkDistance만큼 아래로 내려간 위치)
        Vector3 point1End = point1 + castDirection;
        Vector3 point2End = point2 + castDirection;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(point1End, capsuleRadius);
        Gizmos.DrawWireSphere(point2End, capsuleRadius);

        // 상단과 하단의 연결선
        Gizmos.DrawLine(point1, point1End);
        Gizmos.DrawLine(point2, point2End);

        // 실제 바닥 충돌 위치 디버그
        if (isGrounded)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(groundedHitPoint, 0.05f);
        }
    }

    private void OnDestroy()
    {
        if(GameManager.Instance != null)
            GameManager.UnregisterPlayer(this.Object);
    }

    public void Player_Reset()
    {
        Hp = 100f;
        isAlive = true;
        rb.linearVelocity = Vector3.zero;

        RPC_SetAnim("isAlive", isAlive);
        RPC_SetArmAnim("isAliveBool", isAlive);
        RPC_cameraON();
    }


}
