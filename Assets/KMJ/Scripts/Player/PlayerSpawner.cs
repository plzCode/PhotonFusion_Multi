using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System.Collections.Generic;         // Dictionary
using System;
using Fusion.Protocol;                             // ArraySegment

public class PlayerSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] NetworkObject playerPrefab;

    /* 실제로 쓸 콜백 */
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;
        runner.Spawn(playerPrefab, new Vector3(0, 1, -5), Quaternion.identity, player);
    }

    /* 빈 스텁 콜백 (시그니처 최신) */
    public void OnPlayerLeft(NetworkRunner r, PlayerRef p) { }
    public void OnInput(NetworkRunner r, NetworkInput i) { }
    public void OnInputMissing(NetworkRunner r, PlayerRef p, NetworkInput i) { }
    public void OnShutdown(NetworkRunner r, ShutdownReason s) { }
    public void OnConnectedToServer(NetworkRunner r) { }
    public void OnDisconnectedFromServer(NetworkRunner r, NetDisconnectReason dr) { }
    public void OnConnectRequest(NetworkRunner r, NetworkRunnerCallbackArgs.ConnectRequest req, byte[] token) { }
    public void OnConnectFailed(NetworkRunner r, NetAddress addr, NetConnectFailedReason reason) { }
    public void OnSessionListUpdated(NetworkRunner r, List<SessionInfo> l) { }
    public void OnReliableDataReceived(NetworkRunner r, PlayerRef p, ReliableKey k, ArraySegment<byte> d) { }
    public void OnReliableDataProgress(NetworkRunner r, PlayerRef p, ReliableKey k, float progress) { }
    public void OnUserSimulationMessage(NetworkRunner r, SimulationMessagePtr msg) { }
    public void OnCustomAuthenticationResponse(NetworkRunner r, Dictionary<string, object> data) { }
    public void OnSceneLoadStart(NetworkRunner r) { }
    public void OnSceneLoadDone(NetworkRunner r) { }
    public void OnObjectEnterAOI(NetworkRunner r, NetworkObject o, PlayerRef p) { }
    public void OnObjectExitAOI(NetworkRunner r, NetworkObject o, PlayerRef p) { }
    public void OnHostMigration(NetworkRunner r, HostMigrationToken token) { }
    public void OnUserDisconnected(NetworkRunner r, PlayerRef player, DisconnectReason reason) { }
}
