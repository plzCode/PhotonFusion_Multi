using UnityEngine;

[System.Serializable]
public class WaveConfig
{
    public int minPerPlayer = 2;
    public int maxPerPlayer = 4;
    public float innerRadius = 3f;
    public float outerRadius = 8f;

    public bool isContinuous = false;  // true → SOS 형태
    public float duration = 210f;   // 총 지속 시간
    public float interval = 30f;    // 스폰 간격
}
