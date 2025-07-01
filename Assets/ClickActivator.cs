using UnityEngine;

public class ClickActivator : MonoBehaviour
{
    public LightController lightController;
    public bool isPlayerInRange = false;
    private bool hasActivated = false;

    void Start()
    {
        if (lightController == null)
        {
            Debug.LogError("❌ lightController가 Inspector에서 연결되지 않았습니다.");
        }
        else
        {
            Debug.Log("✅ lightController 연결됨: " + lightController.name);
        }
    }

    void Update()
    {
        //Debug.Log("Update 확인");
        /*
                if (Input.anyKeyDown)
                    *//*Debug.Log("Key 눌림");*//*

                if (Input.GetKeyDown(KeyCode.E))
                   *//* Debug.Log("E 키 눌림");*/

        // Debug.Log(" !!! >>> " + gameObject.activeInHierarchy);
        if (isPlayerInRange && !lightController.isActivated && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E키 눌림");
            lightController.EnableAllLights();
            lightController.isActivated = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            Debug.Log("🚪 플레이어 장치 범위 안에 들어옴");

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            Debug.Log("🚪 플레이어 장치 범위 밖으로 나감");
        }
    }
}
