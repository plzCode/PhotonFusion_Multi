using UnityEngine;

public class Bullet : MonoBehaviour
{

    [SerializeField] float timeToDestroy;

    void Start()
    {
        Destroy(this.gameObject, timeToDestroy);
    }


    void Update()
    {

    }
}
