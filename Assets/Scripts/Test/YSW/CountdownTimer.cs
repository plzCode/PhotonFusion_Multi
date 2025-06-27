using TMPro;
using UnityEngine;
using System;

public class CountdownTimer : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text countdownText;

    [Header("Timer Settings")]
    public float CountdownEndTime = 0f;
    public float RemainingTime => Mathf.Max(CountdownEndTime - Time.time, 0f);

    private bool isCounting = false;
    private Action onComplete; // ✅ 외부에서 넘겨주는 완료 후 실행 함수

    /// <summary>
    /// 지정된 시간으로 타이머 시작하고, 완료 후 실행할 콜백 설정
    /// </summary>
    public void StartCountdown(float durationSeconds, Action onCompleteCallback = null)
    {
        transform.gameObject.SetActive(true);
        countdownText.gameObject.SetActive(true);
        CountdownEndTime = Time.time + durationSeconds;
        isCounting = true;
        onComplete = onCompleteCallback;
    }

    /// <summary>
    /// 남은 시간 직접 설정 (옵션)
    /// </summary>
    public void SetRemainingTime(float seconds, Action onCompleteCallback = null)
    {
        CountdownEndTime = Time.time + seconds;
        isCounting = true;
        countdownText.gameObject.SetActive(true);
        onComplete = onCompleteCallback;
    }

    private void Update()
    {
        if (!isCounting) return;

        float remaining = RemainingTime;

        if (remaining > 0f)
        {
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            countdownText.text = $"{minutes:D2}:{seconds:D2}";
        }
        else
        {
            countdownText.text = "00:00";
            countdownText.gameObject.SetActive(false);
            isCounting = false;

            // ✅ 콜백 실행
            onComplete?.Invoke();
        }
    }
}
