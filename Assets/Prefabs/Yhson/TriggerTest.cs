using UnityEngine;

public class TriggerTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("�÷��̾� ����");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("�÷��̾� ����");
    }
}
