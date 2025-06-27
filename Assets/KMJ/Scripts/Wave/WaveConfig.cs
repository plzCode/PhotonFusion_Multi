using UnityEngine;

[System.Serializable]
public class WaveConfig
{
    public float innerRadius = 3f;
    public float outerRadius = 8f;
    public int minCount = 30;
    public int maxCount = 40;
    [Tooltip("웨이브 총 호출 가능 횟수 (-1 = 무제한)")]
    public int maxTriggerTimes = 1;

    [HideInInspector] public int triggered = 0;
}
