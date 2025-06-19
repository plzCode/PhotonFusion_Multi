using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FlashScreen : MonoBehaviour
{
    public static FlashScreen Instance;   // 싱글턴
    RawImage img;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);             // 중복 방지
            return;
        }
        img = GetComponent<RawImage>();
    }

    public void Blink(float t) => StartCoroutine(BlinkCR(t));

    IEnumerator BlinkCR(float t)
    {
        img.color = Color.white;
        float el = 0;
        while (el < t)
        {
            el += Time.deltaTime;
            img.color = new Color(1, 1, 1, 1 - el / t);  // 알파 1→0
            yield return null;
        }
        img.color = Color.clear;
    }
}
