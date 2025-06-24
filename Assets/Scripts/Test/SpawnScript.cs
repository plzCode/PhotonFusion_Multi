using Fusion;
using UnityEngine;

public class SpawnScript : NetworkBehaviour 
{
    public static SpawnScript Current {get; private set;}

    public Transform[] spawnpoints;

    private void Awake()
    {
        Current = this;
        GameManager.SetSpawnScript(this);
    }

    public bool ControlCamera(Camera cam)
    {
        throw new System.NotImplementedException();
    }

    public void SpawnPlayer(NetworkRunner runner, RoomPlayer player)
    {
        var index = RoomPlayer.Players.IndexOf(player);
        //var point = spawnpoints[index];
        var point = spawnpoints[0];

        var prefabId = player.KartId;
        var prefab = ResourceManager.Instance.fps_Player[prefabId].prefab;

        // Spawn player
        var entity = runner.Spawn(
            prefab,
            point.position,
            point.rotation,
            player.Object.InputAuthority
        );

        entity.Controller.RoomUser = player;
        player.GameState = RoomPlayer.EGameState.GameCutscene;
        player.Player = entity.Controller;

        Debug.Log($"Spawning kart for {player.Username} as {entity.name}");
        entity.transform.name = $"Kart ({player.Username})";
    }
}
