using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class plauer : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float mouseSensitivity = 2f;

    public Transform camTransform;

    private Rigidbody rb;
    private bool isGrounded;
    private float yRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (camTransform == null)
            camTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 마우스 회전
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        yRotation += mouseX;

        transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        camTransform.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // 점프 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("스페이스바 눌림");
            if (isGrounded)
            {
                Debug.Log("점프!");
                Jump();
            }
            else
            {
                Debug.Log("점프 실패: 바닥 아님");
            }
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        Vector3 moveDir = (forward * v + right * h).normalized;

        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);

        // ✅ Layer 없이 바닥 체크: 아무 콜라이더나 감지
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1.2f);
        Debug.DrawRay(transform.position + Vector3.up * 0.1f, Vector3.down * 1.2f, isGrounded ? Color.green : Color.red);
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("충돌한 오브젝트: " + collision.gameObject.name);

        if (collision.gameObject.name.Contains("Collider") && collision.gameObject.GetComponent<MeshRenderer>() == null)
        {
            Debug.LogWarning("⚠️ 투명한 Collider 감지됨: " + collision.gameObject.name);
            Debug.Log("💥 충돌: " + collision.gameObject.name); ;
        }

    }
}