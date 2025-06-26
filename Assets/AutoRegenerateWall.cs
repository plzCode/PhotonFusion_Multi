using System.Collections;
using UnityEngine;

public class AutoRegenerateWall : MonoBehaviour
{
    private Collider wallCollider;
    private bool isRegenerating = false;

    void Start()
    {
        wallCollider = GetComponent<Collider>();
        Debug.Log("▶ Start 실행됨");
        Debug.Log("▶ Collider 있음? → " + (wallCollider != null));

    }

     void Update()
    {
        StartCoroutine(ReenableAfterDelay());
    }
    void OnTriggerEnter(Collider other)
        {
            Debug.Log("▶ Trigger 진입: " + other.name);

            if (!isRegenerating && other.CompareTag("Player"))
            {
                Debug.Log("▶ 코루틴 시작");
                StartCoroutine(ReenableAfterDelay());
            }
        }

        IEnumerator ReenableAfterDelay()
        {
            isRegenerating = true;
            if (wallCollider == null)
            {
                Debug.Log("❌ Collider가 null임!");
                yield break;
            }

            wallCollider.enabled = false;
            Debug.Log("▶ Collider 비활성화");

            yield return new WaitForSeconds(3f); // 👈 이게 문제 없이 지나가야 함

            wallCollider.enabled = true;
            Debug.Log("✅ Collider 다시 활성화됨");
            isRegenerating = false;
        }
    }
