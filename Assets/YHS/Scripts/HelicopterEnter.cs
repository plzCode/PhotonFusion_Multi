using System.Collections;
using UnityEngine;

public class HelicopterEnter : MonoBehaviour
{
    int alivePlayerCount;
    int helicopterEnterCount;
    public Helicoptor_Controller heli_Control;
    [SerializeField]public Camera helicopterCamera;

    private void Start()
    {
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(10f); // 10초 대기
        StartCoroutine(CheckAlivePlayersRoutine());
    }

    private IEnumerator CheckAlivePlayersRoutine()
    {
        while (true)
        {
            alivePlayerCount = 0;
            for (int i = 0; i < GameManager.Players.Count; i++)
            {
                if (GameManager.Players[i].GetComponent<PlayerController>().isAlive)
                {
                    alivePlayerCount++;
                }
            }

            if (alivePlayerCount <= helicopterEnterCount)
            {
                heli_Control.ReturnFlightSequence();
                Debug.Log("생존인원 : " + alivePlayerCount + "탑승 인원 : " + helicopterEnterCount);
                Debug.Log("게임클리어");

                // 타임라인 재생
            }

            yield return new WaitForSeconds(1f); // 1초마다 반복
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            player.isClear= true; 
            player.gameObject.SetActive(false);
            player.playerCamera = helicopterCamera;
            helicopterEnterCount++; 
            
        }
    }
}
