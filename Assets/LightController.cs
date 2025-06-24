using UnityEngine;

public class LightController : MonoBehaviour
{
    public GameObject lightGroup;  // 반드시 Inspector에 연결
    private bool isActivated = false;

    void Start()
    {
        if (lightGroup != null)
        {
            lightGroup.SetActive(false);  // 처음에 꺼짐
            Debug.Log("LightGroup 꺼짐");
        }
        else
        {
            Debug.LogError("lightGroup 연결 안됨"); // 여기에 걸리면 연결이 안 된 것
        }
    }

    void Update()
    {
        if (Input.anyKeyDown) Debug.Log("키 입력 감지됨");

        if (!isActivated && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F키 눌림");
            TurnOnLights();
        }
    }

    public void TurnOnLights()
    {
        if (lightGroup != null)
        {
            lightGroup.SetActive(true);
            isActivated = true;
            Debug.Log("조명 켜짐");
        }
        else
        {
            Debug.LogError("조명 그룹이 연결되지 않음");
        }
    }
}
