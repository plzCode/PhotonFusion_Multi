using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Local;         // 로컬 싱글톤

    [SerializeField] float baseSpeed = 5f;
    float curSpeed;
    Coroutine slowCR;

    void Awake() { Local = this; curSpeed = baseSpeed; }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0, v).normalized;
        transform.Translate(dir * curSpeed * Time.deltaTime, Space.World);
    }

    public void ApplySlow(float factor, float time)
    {
        if (slowCR != null) StopCoroutine(slowCR);
        slowCR = StartCoroutine(SlowCR(factor, time));
    }

    IEnumerator SlowCR(float f, float t)
    {
        curSpeed = baseSpeed * f;
        yield return new WaitForSeconds(t);
        curSpeed = baseSpeed;
        slowCR = null;
    }
}