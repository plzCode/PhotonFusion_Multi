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
            if (Runner.IsResimulation) return; // ���� ���� �Ǵ� ���� ���� �� �ϳ��� ����ǰ� ����
            // �Է� ���� �ε巴�� ����
            currentInput = Vector2.Lerp(currentInput, input.MoveDirection, inputSmoothSpeed * Runner.DeltaTime);

            // ��ǥ �ӵ� ���� ����
            float targetSpeedMultiplier = input.IsRunning ? runSpeed : walkSpeed;
            currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, inputSmoothSpeed * Runner.DeltaTime);

            // �̵� ����� �ӵ� ���
            Vector3 moveDir = new Vector3(currentInput.x, 0, currentInput.y);
            if (moveDir.sqrMagnitude > 1f)
                moveDir.Normalize();

            Vector3 targetVelocity = moveDir * moveSpeed * currentSpeedMultiplier;
            targetVelocity.y = rb.linearVelocity.y;  // �߷� �� ���� �ӵ� ����


            if (input.JumpPressed && isGrounded)
            {
                Debug.Log("���� ����");
                var players = GameManager.Players;
                foreach (var playerObject in players)
                {
                    if (playerObject == null) continue;

                    // ��: �÷��̾��� Transform ����
                    Transform playerTransform = playerObject.transform;
                    NetworkObject netObject = playerObject.GetComponent<NetworkObject>();
                    // ��: �÷��̾� ��ũ��Ʈ ��������
                    
                    if (playerTransform != null)
                    {
                        Debug.Log(playerTransform.position);
                    }
                    if (netObject != null)
                    {
                        Debug.Log("Id : "+ netObject.Id + "name : " + netObject.name);
                    }

                    // ���ϴ� �۾� ���� (�Ÿ� ���, ���� üũ ��)
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
        Gizmos.DrawSphere(origin + direction, 0.05f); // ������ ���� ���� ǥ��
    }

}
