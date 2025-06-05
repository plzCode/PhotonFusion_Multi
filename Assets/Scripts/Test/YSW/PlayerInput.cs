using System;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : NetworkBehaviour, INetworkRunnerCallbacks
{
    public struct NetworkInputData : INetworkInput
    {
       /* public const uint ButtonW = 1 << 0;
        public const uint ButtonS = 1 << 1;
        public const uint ButtonDrift = 1 << 2;
        public const uint ButtonLookbehind = 1 << 3;
        public const uint UseItem = 1 << 4;*/

        public uint Buttons;
        public uint OneShots;
        public bool IsUp(uint button) => IsDown(button) == false;
        public bool IsDown(uint button) => (Buttons & button) == button;
        public bool IsDownThisFrame(uint button) => (OneShots & button) == button;        

        public Vector2 MoveDirection;


    }
    public Gamepad gamepad;
    [SerializeField] private InputAction move;

    public override void Spawned()
    {
        base.Spawned();
        Runner.AddCallbacks(this);

        move = move.Clone();
        move.Enable();
    }
    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        base.Despawned(runner, hasState);

        DisposeInputs();
        Runner.RemoveCallbacks(this);
    }
    private void OnDestroy()
    {
        DisposeInputs();
    }
    public void OnInput(NetworkRunner runner, NetworkInput input) 
    {
        gamepad = Gamepad.current;
        var userInput = new NetworkInputData();

        Vector2 moveInput = move.ReadValue<Vector2>();
        userInput.MoveDirection = moveInput.normalized;
        input.Set(userInput);
    }
    private static bool ReadBool(InputAction action) => action.ReadValue<float>() != 0;
    private static float ReadFloat(InputAction action) => action.ReadValue<float>();

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    private void DisposeInputs()    {    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)    { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)    {}

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)    {}

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)    {}

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)    {}

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)    {}

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)    {}

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)    {}

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)    {}

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)    {}

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)    {}
   
    public void OnConnectedToServer(NetworkRunner runner)    {    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)    {}

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)    {}

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)    {}
    public void OnSceneLoadDone(NetworkRunner runner)    {}

    public void OnSceneLoadStart(NetworkRunner runner)    {}
}
