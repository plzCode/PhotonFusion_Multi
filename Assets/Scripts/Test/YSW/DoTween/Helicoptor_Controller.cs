using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Helicoptor_Controller: MonoBehaviour
{
    public Transform flyTargetPoint;   // 헬기가 날아갈 지점 (착륙 전 공중)
    public Transform landingPoint;     // 착륙 위치
    public float flyDuration = 5f;
    public float descendDuration = 3f;
    public float tiltAngle = 15f;      // 비행 시 기울기 (앞으로)

    public AudioSource heliSound;

    // 모델이 X+ 방향일 경우 보정용 회전
    private readonly Quaternion modelRotationOffset = Quaternion.Euler(0f, -90f, 0f);

    public void Start()
    {
        StartFlightSequence();
    }

    public void StartFlightSequence()
    {
        Vector3 toTarget = (flyTargetPoint.position - transform.position).normalized;
        // 1️⃣ 비행 방향 회전 계산 (보정 포함)
        Quaternion lookRotZ = Quaternion.LookRotation(toTarget);
        Quaternion lookRot = lookRotZ * Quaternion.Euler(0f, -90f, 0f);

        // 2️⃣ 기울이기 회전 (헬기 기준 Z축 → 앞으로 기울임)
        Vector3 heliRight = lookRot * Vector3.forward;
        Quaternion tiltRot = Quaternion.AngleAxis(tiltAngle, heliRight) * lookRot;

        // ✅ 3️⃣ Z축만 0으로 정리한 회전값
        Vector3 lookEuler = lookRot.eulerAngles;
        lookEuler.z = 0f;
        Quaternion correctedLookRot = Quaternion.Euler(lookEuler);

        // 4️⃣ DOTween 시퀀스
        Sequence seq = DOTween.Sequence();

        // 기울여서 비행
        seq.Append(transform.DORotateQuaternion(tiltRot, 1f));
        seq.Join(transform.DOMove(flyTargetPoint.position, flyDuration).SetEase(Ease.InOutSine));

        // ✅ Z축 회전 제거한 상태로 정리
        seq.Append(transform.DORotateQuaternion(correctedLookRot, 0.4f));

        // 착륙
        seq.Append(transform.DOMove(landingPoint.position, descendDuration).SetEase(Ease.OutSine));

        // 착륙 후 보정
        seq.OnComplete(() => {
            // 혹시 미세하게 틀어졌다면 마지막으로 정리
            transform.rotation = correctedLookRot;
        });

    }
}
