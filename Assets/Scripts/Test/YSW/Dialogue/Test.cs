using UnityEngine;

public class Test : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered the trigger zone.");
            // Add your logic here, e.g., start a dialogue or interaction
        }
    }
}
