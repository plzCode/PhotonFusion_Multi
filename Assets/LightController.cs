using UnityEngine;

public class LightController : MonoBehaviour
{
    public GameObject lightGroup; 
    public bool isActivated = false;

    void Start()
    {
        if (lightGroup != null)
        {
            DisableAllLights();  // 처음에 꺼짐
            Debug.Log("LightGroup 꺼짐");
        }
        else
        {
            Debug.LogError("lightGroup 연결 안됨"); 
        }
    }

   /* void Update()
    {

        if (Input.anyKeyDown) Debug.Log("키 입력 감지됨");
        {
            if (!isActivated && Input.GetKeyDown(KeyCode.E))
            {

                Debug.Log("E키 눌림");
                EnableAllLights();
                isActivated = true;
            }
        }
    }*/


    public void DisableAllLights()
    {
        var lights = lightGroup.GetComponentsInChildren<Light>();
        Debug.Log($"DisableAllLights 찾은 라이트 개수: {lights.Length}");

        foreach (var light in lights)
        {
            Debug.Log($"끄는 라이트: {light.name}");
            light.enabled = false;
        }
    }

    public void EnableAllLights()
    {
        var lights = lightGroup.GetComponentsInChildren<Light>();
        Debug.Log($"EnableAllLights 찾은 라이트 개수: {lights.Length}");

        foreach (var light in lights)
        {
            Debug.Log($"켜는 라이트: {light.name}");
            light.enabled = true;
        }
    }
}