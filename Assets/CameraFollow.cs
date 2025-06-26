using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;         // ���� ��� (�÷��̾�)
    public Vector3 offset = new Vector3(0, 5, -7); // ī�޶���� �Ÿ�

    void LateUpdate()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target); // Ÿ���� �ٶ󺸰�
        }
    }
}
