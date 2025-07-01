using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Helicoptor_Controller: MonoBehaviour
{
    public Vector3 startPosition;
    public Transform flyTargetPoint;   // 헬기가 날아갈 지점 (착륙 전 공중)
    public Transform landingPoint;     // 착륙 위치
    public float flyDuration = 5f;
    public float descendDuration = 3f;
    public float tiltAngle = 15f;      // 비행 시 기울기 (앞으로)

    public GameObject heli_model;

    public AudioSource heliSound;

    // 모델이 X+ 방향일 경우 보정용 회전
    private readonly Quaternion modelRotationOffset = Quaternion.Euler(0f, -90f, 0f);

    public void Start()
    {
        startPosition = heli_model.transform.position;
    }

    public void StartEvent()
    {
        InterfaceManager.Instance.countdownTimer.StartCountdown(60f, StartFlightSequence);
    }

    public void StartFlightSequence()
    {
        if (!heli_model.gameObject.activeSelf)
        {
            heli_model.SetActive(true);
        }
        if(heliSound != null && !heliSound.isPlaying)
        {
            heliSound.Play();
        }

        Vector3 toTarget = (flyTargetPoint.position - heli_model.transform.position).normalized;
        // 1️ 비행 방향 회전 계산 (보정 포함)
        Quaternion lookRotZ = Quaternion.LookRotation(toTarget);
        Quaternion lookRot = lookRotZ * Quaternion.Euler(0f, -90f, 0f);

        // 2️ 기울이기 회전 (헬기 기준 Z축 → 앞으로 기울임)
        Vector3 heliRight = lookRot * Vector3.forward;
        Quaternion tiltRot = Quaternion.AngleAxis(tiltAngle, heliRight) * lookRot;

        // 3️ Z축만 0으로 정리한 회전값
        Vector3 lookEuler = lookRot.eulerAngles;
        lookEuler.z = 0f;
        Quaternion correctedLookRot = Quaternion.Euler(lookEuler);

        // 4 DOTween 시퀀스
        Sequence seq = DOTween.Sequence();

        // 기울여서 비행
        seq.Append(heli_model.transform.DORotateQuaternion(tiltRot, 1f));
        seq.Join(heli_model.transform.DOMove(flyTargetPoint.position, flyDuration).SetEase(Ease.InOutSine));

        // ✅ Z축 회전 제거한 상태로 정리
        seq.Append(heli_model.transform.DORotateQuaternion(correctedLookRot, 0.4f));

        // 착륙
        seq.Append(heli_model.transform.DOMove(landingPoint.position, descendDuration).SetEase(Ease.OutSine));

        // 착륙 후 보정
        seq.OnComplete(() => {
            // 혹시 미세하게 틀어졌다면 마지막으로 정리
            heli_model.transform.rotation = correctedLookRot;
        });

    }
    // 어두워지기 (투명 → 불투명)
    public void FadeIn(float duration = 3f)
    {
        InterfaceManager.Instance.fadeImage.gameObject.SetActive(true);
        InterfaceManager.Instance.fadeImage.DOFade(1f, duration).SetEase(Ease.Linear);
    }
    public void ReturnFlightSequence()
    {
        if (!heli_model.gameObject.activeSelf)
            heli_model.SetActive(true);

        if (heliSound != null && !heliSound.isPlaying)
            heliSound.Play();

        FadeIn();

        Vector3 currentPos = heli_model.transform.position;

        // ✅ 1. 수직으로 상승 (XZ 고정, Y만 flyTargetPoint 높이로)
        Vector3 verticalTarget = new Vector3(currentPos.x, flyTargetPoint.position.y, currentPos.z);

        // DOTween 시퀀스 생성
        Sequence seq = DOTween.Sequence();

        // 1️⃣ 수직 상승
        seq.Append(heli_model.transform.DOMove(verticalTarget, descendDuration).SetEase(Ease.OutSine));

        // ✅ 2. 비행 방향 계산 (올라간 지점 → startPosition)
        Vector3 toStart = (startPosition - verticalTarget).normalized;
        Quaternion lookRot = Quaternion.LookRotation(toStart) * modelRotationOffset;

        // ✅ 3. 기울임 회전
        Vector3 heliRight = lookRot * Vector3.forward;
        Quaternion tiltRot = Quaternion.AngleAxis(tiltAngle, heliRight) * lookRot;

        // ✅ 4. Z축 제거한 회전값
        Vector3 lookEuler = lookRot.eulerAngles;
        lookEuler.z = 0f;
        Quaternion correctedLookRot = Quaternion.Euler(lookEuler);

        // 2️⃣ 비행 방향으로 회전하고 이동
        seq.Append(heli_model.transform.DORotateQuaternion(tiltRot, 1f));
        seq.Join(heli_model.transform.DOMove(startPosition, flyDuration).SetEase(Ease.InOutSine));

        // 3️⃣ Z축 회전 정리
        seq.Append(heli_model.transform.DORotateQuaternion(correctedLookRot, 0.4f));

        // 4️⃣ 종료 정리
        seq.OnComplete(() =>
        {
            heli_model.transform.rotation = correctedLookRot;
        });
    }

}
