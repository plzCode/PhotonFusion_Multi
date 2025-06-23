using UnityEngine;

public class LightController : MonoBehaviour
{
    public GameObject lightGroup;  // �ݵ�� Inspector�� ����
    private bool isActivated = false;

    void Start()
    {
        if (lightGroup != null)
        {
            lightGroup.SetActive(false);  // ó���� ����
            Debug.Log("LightGroup ����");
        }
        else
        {
            Debug.LogError("lightGroup ���� �ȵ�"); // ���⿡ �ɸ��� ������ �� �� ��
        }
    }

    void Update()
    {
        if (Input.anyKeyDown) Debug.Log("Ű �Է� ������");

        if (!isActivated && Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("FŰ ����");
            TurnOnLights();
        }
    }

    public void TurnOnLights()
    {
        if (lightGroup != null)
        {
            lightGroup.SetActive(true);
            isActivated = true;
            Debug.Log("���� ����");
        }
        else
        {
            Debug.LogError("���� �׷��� ������� ����");
        }
    }
}
