using Fusion;
using UnityEngine;
using static Fusion.NetworkBehaviour;
using static KartInput;

public class PlayerController : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private bool isGrounded = true;
    [SerializeField] private float groundCheckDistance;
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
            if (Runner.IsResimulation) return; // 예측 루프 또는 보정 루프 중 하나만 실행되게 막음
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


            if (input.JumpPressed && isGrounded)
            {
                Debug.Log("점프 눌림");
                var players = GameManager.Players;
                foreach (var playerObject in players)
                {
                    if (playerObject == null) continue;

                    // 예: 플레이어의 Transform 참조
                    Transform playerTransform = playerObject.transform;
                    NetworkObject netObject = playerObject.GetComponent<NetworkObject>();
                    // 예: 플레이어 스크립트 가져오기
                    
                    if (playerTransform != null)
                    {
                        Debug.Log(playerTransform.position);
                    }
                    if (netObject != null)
                    {
                        Debug.Log("Id : "+ netObject.Id + "name : " + netObject.name);
                    }

                    // 원하는 작업 수행 (거리 계산, 상태 체크 등)
                }
                anim.SetTrigger("Jump");
                targetVelocity.y =  jumpForce;
            }

            rb.linearVelocity = targetVelocity;

            
            if (anim != null)
            {
                anim.SetFloat("MoveX", currentInput.x, 0.1f, Runner.DeltaTime);
                anim.SetFloat("MoveZ", currentInput.y * currentSpeedMultiplier, 0.1f, Runner.DeltaTime);
            }

            Debug.Log(Object.Id + ", " + Object.Name + ", " + Object.HasStateAuthority + ", " + Object.HasInputAuthority);
        }

        
        if (!isGrounded)
        {
            Vector3 gravity = Vector3.up * customGravity;
            if (rb.linearVelocity.y < 0)
                gravity *= fallMultiplier;

            rb.AddForce(gravity, ForceMode.Acceleration);
        }

        CheckGrounded();
    }


    public float customGravity = -9.81f;
    public float fallMultiplier = 2.0f;

    private void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, ~0);
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
