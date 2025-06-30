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
        yield return new WaitForSeconds(10f); // 10�� ���
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
                Debug.Log("�����ο� : " + alivePlayerCount + "ž�� �ο� : " + helicopterEnterCount);
                Debug.Log("����Ŭ����");

                // Ÿ�Ӷ��� ���
            }

            yield return new WaitForSeconds(1f); // 1�ʸ��� �ݺ�
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
