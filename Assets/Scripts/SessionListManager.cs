using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class SessionListManager : MonoBehaviour
{

    [SerializeField] private Transform sessionListContainer;  // UI 아이템이 들어갈 부모
    [SerializeField] private GameObject sessionListItemPrefab; // UI 아이템 프리팹

    public void UpdateSessionListUI(List<SessionInfo> sessions)
    {
        // 기존 UI 초기화
        foreach (Transform child in sessionListContainer)
        {
            Destroy(child.gameObject);
        }

        // 새로운 세션 아이템 UI 생성
        foreach (var session in sessions)
        {
            GameObject item = Instantiate(sessionListItemPrefab, sessionListContainer);
            item.GetComponent<SessionListInfo>().Setup(session);
            // item.GetComponent<SessionListItem>().Setup(session); // 세션 정보를 UI에 세팅하는 함수 호출
        }
    }
}
