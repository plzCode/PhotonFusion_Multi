using UnityEngine;

public class FpsAnimEvent : MonoBehaviour
{

    [SerializeField] private AudioClip[] FootSteps;
    private AudioSource audioSource;

    [SerializeField] private float footstepCooldown = 0.2f; // �ּ� �߼Ҹ� ����
    private bool canPlayFootstep = true;

    [SerializeField] private PlayerController player;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        player = GetComponentInParent<PlayerController>();
    }

    private void PlayFootstep(int index)
    {
        if (!canPlayFootstep) return;

        if (FootSteps != null && index >= 0 && index < FootSteps.Length && FootSteps[index] != null)
        {
            audioSource.PlayOneShot(FootSteps[index]);
            canPlayFootstep = false;
            Invoke(nameof(ResetFootstep), footstepCooldown);
        }
    }

    private void ResetFootstep()
    {
        canPlayFootstep = true;
    }

    // �ִϸ��̼� �̺�Ʈ���� ȣ��
    public void FootStepSound_1()
    {
        PlayFootstep(0);
    }

    public void FootStepSound_2()
    {
        PlayFootstep(1);
    }

    private void ReloadingEnd()
    {
        player.weaponManager.Reloading();
        player.isReloading = false;
        player.armAnim.SetBool("isReload", false);
        player.anim.SetBool("isReload", false);
        
    }
}
