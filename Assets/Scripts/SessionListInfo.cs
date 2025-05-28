using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionListInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button button;  // 버튼 컴포넌트 연결

    public void Setup(SessionInfo session)
    {
        roomNameText.text = session.Name;
        playerCountText.text = $"{session.PlayerCount} / {session.MaxPlayers}";

        // 버튼 컴포넌트 가져오기
        Button btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => OnClickJoinSessionWithSession(session));
    }

    public void OnClickJoinSessionWithSession(SessionInfo session)
    {
        ClientInfo.LobbyName = session.Name;
        Debug.Log($"방 '{session.Name}'에 참가 요청!");
        Debug.Log(ClientInfo.LobbyName);
    }
}
