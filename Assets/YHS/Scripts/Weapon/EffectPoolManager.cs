using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPoolManager : MonoBehaviour
{
    [SerializeField] private GameObject hitEffectPrefab;
    private Queue<GameObject> pool = new Queue<GameObject>();

    public static EffectPoolManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject GetEffect(Vector3 position, Quaternion rotation)
    {
        GameObject effect;

        if (pool.Count > 0)
        {
            effect = pool.Dequeue();
        }
        else
        {
            effect = Instantiate(hitEffectPrefab);
        }

        effect.transform.SetPositionAndRotation(position, rotation);
        effect.SetActive(true);
        effect.GetComponent<ParticleSystem>().Play();

        // 자동 반환
        StartCoroutine(ReturnToPool(effect, 0.1f));

        return effect;
    }

    private IEnumerator ReturnToPool(GameObject effect, float delay)
    {
        yield return new WaitForSeconds(delay);
        effect.SetActive(false);
        pool.Enqueue(effect);
    }
}
