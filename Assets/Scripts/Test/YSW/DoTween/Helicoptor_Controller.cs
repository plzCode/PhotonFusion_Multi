using UnityEngine;
using DG.Tweening;

public class Helicopter_Controller : MonoBehaviour
{
    public Transform flyTargetPoint;   // 헬기가 날아갈 지점 (착륙 전 공중)
    public Transform landingPoint;     // 착륙 위치
    public float flyDuration = 5f;
    public float descendDuration = 3f;
    public float tiltAngle = 15f;      // 비행 시 기울기 (앞으로)

    public AudioSource heliSound;

    // 모델이 X+ 방향일 경우 보정용 회전
    private readonly Quaternion modelRotationOffset = Quaternion.Euler(0f, -90f, 0f);    

    public void StartFlightSequence()
    {
        if (!transform.gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        Vector3 toTarget = (flyTargetPoint.position - transform.position).normalized;

        // ✅ 기본 회전 (Z+ 기준 → X+ 보정)
        Quaternion lookRotZ = Quaternion.LookRotation(toTarget);
        Quaternion lookRot = lookRotZ * Quaternion.Euler(0f, -90f, 0f);

        // ✅ 기울이기 축: 헬기 기준 로컬 Z축
        Vector3 heliRight = lookRot * Vector3.forward;
        Quaternion tiltRot = Quaternion.AngleAxis(tiltAngle, heliRight) * lookRot;

        Sequence seq = DOTween.Sequence();

        // 1️⃣ 기울이며 비행
        seq.Append(transform.DORotateQuaternion(tiltRot, 1f));
        seq.Join(transform.DOMove(flyTargetPoint.position, flyDuration).SetEase(Ease.InOutSine));

        // 2️⃣ 수평 자세로 복원
        seq.Append(transform.DORotateQuaternion(lookRot, 0.5f));

        // 3️⃣ 착륙
        seq.Append(transform.DOMove(landingPoint.position, descendDuration).SetEase(Ease.OutSine));



    }
}
