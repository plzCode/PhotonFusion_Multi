using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class EventSpawnPoint : MonoBehaviour
{
    [Tooltip("이 스폰 지점이 속한 Wave 이름 (발전기A, 사이렌B 등)")]
    public string waveId = "Generator";
    //앰뷸런스, 헬리콥터 구조 요청, 기타 등등

#if UNITY_EDITOR
    /* Scene 뷰에 아이콘 그리기 */
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.6f);
        Handles.Label(transform.position + Vector3.up * 0.7f, waveId,
                      new GUIStyle { normal = new GUIStyleState { textColor = Color.yellow } });
    }
#endif
}

