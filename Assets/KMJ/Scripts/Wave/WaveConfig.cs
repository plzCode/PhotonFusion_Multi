using UnityEngine;

[System.Serializable]

[CreateAssetMenu(fileName = "NewWaveConfig", menuName = "Configs/WaveConfig")]
public class WaveConfig : ScriptableObject  
{
    public string waveId = "default";  // Wave ID for identification
    public int minPerPlayer = 2;
    public int maxPerPlayer = 4;
    public float innerRadius = 3f;
    public float outerRadius = 8f;

    public int minEvnet = 10;
    public int maxEvnet = 20;

    public bool isContinuous = false;  // true → SOS 형태
    public float duration = 210f;   // 총 지속 시간
    public float interval = 30f;    // 스폰 간격
}
