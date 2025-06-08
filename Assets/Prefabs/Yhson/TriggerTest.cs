using UnityEngine;

public class TriggerTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("플레이어 들어옴");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("플레이어 나감");
    }
}
