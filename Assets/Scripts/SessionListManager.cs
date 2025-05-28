using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class SessionListManager : MonoBehaviour
{

    [SerializeField] private Transform sessionListContainer;  // UI �������� �� �θ�
    [SerializeField] private GameObject sessionListItemPrefab; // UI ������ ������

    public void UpdateSessionListUI(List<SessionInfo> sessions)
    {
        // ���� UI �ʱ�ȭ
        foreach (Transform child in sessionListContainer)
        {
            Destroy(child.gameObject);
        }

        // ���ο� ���� ������ UI ����
        foreach (var session in sessions)
        {
            GameObject item = Instantiate(sessionListItemPrefab, sessionListContainer);
            item.GetComponent<SessionListInfo>().Setup(session);
            // item.GetComponent<SessionListItem>().Setup(session); // ���� ������ UI�� �����ϴ� �Լ� ȣ��
        }
    }
}
