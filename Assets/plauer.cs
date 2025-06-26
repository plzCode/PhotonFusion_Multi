using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class plauer : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 1f, transform.forward, out RaycastHit hit, 1f))
        {
            Debug.Log("전방 감지됨: " + hit.collider.gameObject.name);
            Debug.DrawRay(transform.position + Vector3.up, transform.forward * hit.distance, Color.red);
        }
    }

    void FixedUpdate()
    {
        float moveX = Input.GetAxis("Horizontal"); // A, D
        float moveZ = Input.GetAxis("Vertical");   // W, S

        Vector3 move = new Vector3(moveX, 0f, moveZ).normalized;

        rb.MovePosition(rb.position + move * moveSpeed * Time.fixedDeltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"충돌한 오브젝트: {collision.gameObject.name}");

        if (collision.gameObject.name.Contains("Collider"))
        {
            Debug.LogWarning("⚠️ 플레이어를 막는 Collider 감지됨!");
        }
    }
}