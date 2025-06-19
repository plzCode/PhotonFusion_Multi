using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class ProjectileTrailOnly : MonoBehaviour
{
    [Header("Trail Settings")]
    public bool useAutoScaling = true;
    public float scaleMultiplier = 45f;

    private TrailRenderer trail;
    private Camera mainCam;

    private void Awake()
    {
        trail = GetComponent<TrailRenderer>();
        mainCam = Camera.main;

        if (!useAutoScaling)
        {
            transform.localScale = Vector3.one * scaleMultiplier;
            trail.widthMultiplier = scaleMultiplier;
        }
    }

    private void Update()
    {
        if (!useAutoScaling) return;

        if (mainCam == null)
        {
            mainCam = Camera.main;
            if (mainCam == null) return;
        }

        float distanceFromCam = Vector3.Distance(transform.position, mainCam.transform.position);
        float scale = (distanceFromCam * scaleMultiplier) * (mainCam.fieldOfView / 360f);

        transform.localScale = Vector3.one * scale;
        trail.widthMultiplier = scale;
    }
}