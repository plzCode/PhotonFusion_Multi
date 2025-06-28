using TMPro;
using UnityEngine;

public class FrameCounter : MonoBehaviour
{
    TextMeshProUGUI _text;

    void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float time = Time.time;
        int totalFrames = Mathf.FloorToInt(time * Application.targetFrameRate);
        int hours = totalFrames / (3600 * Application.targetFrameRate);
        int minutes = (totalFrames / (60 * Application.targetFrameRate)) % 60;
        int seconds = (totalFrames / Application.targetFrameRate) % 60;
        int frames = totalFrames % Application.targetFrameRate;
        _text.text = $"{hours:00}:{minutes:00}:{seconds:00}:{frames:00}";
    }
}
