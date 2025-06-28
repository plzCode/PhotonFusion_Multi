using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
	public static event Action<GameManager> OnLobbyDetailsUpdated;

	[SerializeField, Layer] private int groundLayer;
	public static int GroundLayer => Instance.groundLayer;
	[SerializeField, Layer] private int kartLayer;
	public static int KartLayer => Instance.kartLayer;


	public new Camera camera;
	private ICameraController cameraController;

	public GameType GameType => ResourceManager.Instance.gameTypes[GameTypeId];

	public static Track CurrentTrack { get; private set; }
	public static SpawnScript CurrentMap { get; private set; } //For FPS
    public static bool IsPlaying => CurrentTrack != null;

	public static GameManager Instance { get; private set; }

	public string TrackName => ResourceManager.Instance.tracks[TrackId].trackName;
	public string ModeName => ResourceManager.Instance.gameTypes[GameTypeId].modeName;

	[Networked] public NetworkString<_32> LobbyName { get; set; }
	[Networked] public int TrackId { get; set; }
	[Networked] public int GameTypeId { get; set; }
	[Networked] public int MaxUsers { get; set; }

	// 게임 플레이어 목록
	[SerializeField]
    private List<NetworkObject> players = new List<NetworkObject>();

	// 현재 관전중인 플레이어
	public PlayerController observerPlayer;
	
	// 읽기 참조용
    public static IReadOnlyList<NetworkObject> Players => Instance.players;

    private static void OnLobbyDetailsChangedCallback(GameManager changed)
	{
		Debug.Log("변경사항으로 인한 업데이트실시");
		OnLobbyDetailsUpdated?.Invoke(changed);
	}
	
	private ChangeDetector _changeDetector;

	private void Awake()
	{
		if (Instance && Instance != this)
		{
			Debug.Log("삭제했다");
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public override void Spawned()
	{
		base.Spawned();
		
		_changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

		if (Object.HasStateAuthority)
		{
			LobbyName = ServerInfo.LobbyName;
			TrackId = ServerInfo.TrackId;
			GameTypeId = ServerInfo.GameMode;
			MaxUsers = ServerInfo.MaxUsers;
		}

        //OnLobbyDetailsUpdated?.Invoke(this); // 여기서 한번 호출
    }
	
	public override void Render()
	{
		foreach (var change in _changeDetector.DetectChanges(this))
		{
			switch (change)
			{
				case nameof(LobbyName):
				case nameof(TrackId):
				case nameof(GameTypeId):
				case nameof(MaxUsers):
					OnLobbyDetailsChangedCallback(this);
					break;
			}
		}
	}
	
	private void LateUpdate()
	{
		// this shouldn't really be an interface due to how Unity handle's interface lifecycles (null checks dont work).
		if (cameraController == null) return;
		if (cameraController.Equals(null))
		{
			Debug.LogWarning("Phantom object detected");
			cameraController = null;
			return;
		}

		/*if (cameraController.ControlCamera(camera) == false)
			cameraController = null;*/ //Intro Camera
	}
	
	public static void GetCameraControl(ICameraController controller)
	{
		Instance.cameraController = controller;
	}

	public static bool IsCameraControlled => Instance.cameraController != null;

	public static void SetTrack(Track track)
	{
		CurrentTrack = track;
	}
	public static void SetSpawnScript(SpawnScript spawnScript)
	{
		CurrentMap = spawnScript;
	}

    
	public static void RegisterPlayer(NetworkObject player)
	{
        if (!Instance.players.Contains(player))
            Instance.players.Add(player);
		
    }

    public static void UnregisterPlayer(NetworkObject player)
    {
        if (Instance.players.Contains(player))
            Instance.players.Remove(player);
    }


    
}