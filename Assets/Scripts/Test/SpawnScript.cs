using Fusion;
using NUnit.Framework;
using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnScript : NetworkBehaviour
{
    public static SpawnScript Current { get; private set; }

    [System.Serializable]
    public class SpawnPointGroup
    {
        public Transform[] points;
    }

    public SpawnPointGroup[] spawnpoints;

    public int spawnPointIndex = 0;

    public GameObject professor;
    public Vector3 professorPos;
    public Quaternion professorRot;
    public GameObject key;

    List<string> questNames = new List<string>()
    {
        "열쇠 관리 명단을 찾아보자.",
        "열쇠를 찾자.",
        "2층을 탐사하자.",
        "전력을 공급하자.",
        "교수에게 돌아가자.",
        "구조를 요청하자."
    };
    List<string> variableNames = new List<string>()
    {
        "isDoorConact",
        "isListCheck",
        "isKeyGet",
        "isConactProfessor",
        "isPowerActive",
        "Dummy",
        "isRadioActive"
    };

    private void Awake()
    {
        Current = this;
        GameManager.SetSpawnScript(this);
        if (professor != null)
        {
            professorPos = professor.transform.position;
            professorRot = professor.transform.rotation;
        }
    }

    public bool ControlCamera(Camera cam)
    {
        throw new System.NotImplementedException();
    }

    /*public void SpawnPlayer(NetworkRunner runner, RoomPlayer player)
    {
        if (!HasStateAuthority) return;
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
    }*/
    public void SpawnPlayer(NetworkRunner runner, RoomPlayer player)
    {
        Debug.Log($"!!!!!!!!!!!!!!!!");
        if (!HasStateAuthority) return;

        var index = RoomPlayer.Players.IndexOf(player);

        if (spawnPointIndex >= spawnpoints.Length)
        {
            Debug.LogWarning("spawnPointIndex가 spawnpoints 범위를 초과합니다.");
            return;
        }

        Transform[] currentPoints = spawnpoints[spawnPointIndex].points;

        if (currentPoints == null || currentPoints.Length == 0)
        {
            Debug.LogWarning("해당 단계의 스폰 지점이 비어 있습니다.");
            return;
        }

        int spawnIndex = index % currentPoints.Length;
        var point = currentPoints[spawnIndex];

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

        Debug.Log($"Spawning player for {player.Username} at {point.name}");
        entity.transform.name = $"{player.Username}";
        
    }


    /*public void ReSpawn()
    {
        if (!HasStateAuthority) return;
        foreach (var player in GameManager.Players)
        {
            player.transform.position = spawnpoints[spawnPointIndex].position;
        }
        if (DialogueLua.GetVariable("GoToTarget").asBool && professor != null)
        {
            var agent = professor.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false;
            }

            //professor.transform.position =spawnpoints[spawnPointIndex].position;
            professor.GetComponent<NetworkTransform>().Teleport(spawnpoints[spawnPointIndex].position, Quaternion.identity);
            if (agent != null)
            {
                agent.enabled = true;
            }
            TestAI tmpAI = agent.GetComponent<TestAI>();
            if (tmpAI != null)
            {
                tmpAI.ResetAI();
            }
            //RPC_MoveProfessor(spawnpoints[spawnPointIndex].position);
        }
        InitQuestState();
    }*/

    public void ReSpawn()
    {
        if (!HasStateAuthority) return;

        // 플레이어 개수만큼 포인트가 있어야 함
        if (spawnpoints.Length <= spawnPointIndex)
        {
            Debug.LogWarning("지정된 spawnPointIndex에 해당하는 SpawnPointGroup이 없습니다.");
            return;
        }

        Transform[] currentSpawnPoints = spawnpoints[spawnPointIndex].points;
        int i = 0;
        for (i = 0; i < GameManager.Players.Count; i++)
        {
            var player = GameManager.Players[i];
            int spawnIndex = i % currentSpawnPoints.Length; // 오버플로우 방지
            player.transform.position = currentSpawnPoints[spawnIndex].position;
        }

        if (DialogueLua.GetVariable("GoToTarget").asBool && professor != null)
        {
            var agent = professor.GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) agent.enabled = false;

            professor.GetComponent<NetworkTransform>().Teleport(currentSpawnPoints[i].position, Quaternion.identity);

            if (agent != null)
            {
                agent.enabled = true;
                var tmpAI = agent.GetComponent<TestAI>();
                if (tmpAI != null) tmpAI.ResetAI();
            }
        }

        InitQuestState();
    }

    public void SetSpawnPoint(int num)
    {
        spawnPointIndex = num;
    }

    public void InitQuestState()
    {
        if (questNames.Count == 0 || variableNames.Count == 0)
        {
            Debug.LogWarning("퀘스트 이름 또는 변수 이름 리스트가 비어 있습니다.");
            return;
        }

        int activeIndex = -1;

        // 현재 Active 퀘스트 찾기
        for (int i = 0; i < questNames.Count; i++)
        {
            if (QuestLog.GetQuestState(questNames[i]) == QuestState.Active)
            {
                activeIndex = i;
                break;
            }
        }

        if (activeIndex == -1)
        {
            Debug.LogWarning("Active 상태인 퀘스트가 없습니다.");
            return;
        }

        int previousIndex = activeIndex - 1;
        int currentIndex = activeIndex;

        // 이전 퀘스트들: Success
        for (int i = 0; i < previousIndex && i < questNames.Count && i < variableNames.Count; i++)
        {
            QuestLog.SetQuestState(questNames[i], QuestState.Success);
            DialogueLua.SetVariable(variableNames[i], true);
        }

        // 이전 퀘스트: Active
        if (previousIndex >= 0 && previousIndex < questNames.Count && previousIndex < variableNames.Count)
        {
            QuestLog.SetQuestState(questNames[previousIndex], QuestState.Active);
            DialogueLua.SetVariable(variableNames[previousIndex], true);
            Debug.Log("이전 퀘스트 Active: " + questNames[previousIndex]);
        }

        // 현재 퀘스트: Unassigned
        if (currentIndex < questNames.Count && currentIndex < variableNames.Count)
        {
            QuestLog.SetQuestState(questNames[currentIndex], QuestState.Unassigned);
            DialogueLua.SetVariable(variableNames[currentIndex], false);
            Debug.Log("현재 퀘스트 Unassigned: " + questNames[currentIndex]);
        }
        if (questNames[currentIndex] == "2층을 탐사하자.") 
        {
            if(key != null)
            {
                key.SetActive(true);
            }
        }
        if(questNames[currentIndex] == "구조를 요청하자.")
        {
            if (professor != null)
            {
                DialogueLua.SetVariable("GoToTarget", false);
                /*professor.transform.position = professorPos;
                professor.transform.rotation = professorRot;*/
                professor.GetComponent<NetworkTransform>().Teleport(professorPos, professorRot);
            }
        }

    }
}
