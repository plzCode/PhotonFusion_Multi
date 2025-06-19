using UnityEngine;

public class Rotor : MonoBehaviour
{
    public bool allowX = false;
    public bool allowY = true;
    public bool allowZ = false;

    public float rotationSpeed = 0f;

    private void Start()
    {

    }

    private void Update()
    {

        Vector3 deltaRotation = Vector3.zero;

        if (allowX)
            deltaRotation.x = rotationSpeed * Time.deltaTime;

        if (allowY)
            deltaRotation.y = rotationSpeed * Time.deltaTime;

        if (allowZ)
            deltaRotation.z = rotationSpeed * Time.deltaTime;

        transform.Rotate(deltaRotation, Space.Self);
    }

}
