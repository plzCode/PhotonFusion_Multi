using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionListInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button button;  // ��ư ������Ʈ ����

    public void Setup(SessionInfo session)
    {
        roomNameText.text = session.Name;
        playerCountText.text = $"{session.PlayerCount} / {session.MaxPlayers}";

        // ��ư ������Ʈ ��������
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnClickJoinSessionWithSession(session));
    }

    public void OnClickJoinSessionWithSession(SessionInfo session)
    {
        ClientInfo.LobbyName = session.Name;
        Debug.Log($"�� '{session.Name}'�� ���� ��û!");
        Debug.Log(ClientInfo.LobbyName);
    }
}
