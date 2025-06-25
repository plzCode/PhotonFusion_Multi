using Fusion;
using UnityEngine;


public class WeaponManager : NetworkBehaviour
{
    [Header("Fire Rate")]
    [SerializeField] float fireRate;
    [SerializeField] bool semiAuto;
    float fireRateTimer;

    [Header("Bullet Properties")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform barrelPos;
    [SerializeField] float bulletVelocity;
    [SerializeField] int bulletsPerShot;
    public int damage = 10;

    [Header("muzzle Effect")]
    [SerializeField] public GameObject muzzleFlash;
    [SerializeField] GameObject muzzleSmoke;

    [SerializeField] Transform aimpos;

    [Header("recoil Amount")]
    [SerializeField] public float recoilAmount = 2f; // �ݵ� ����
    [SerializeField] public float recoilRecoverySpeed = 8f; // �ݵ� ���� �ӵ�

    void Update()
    {
        //if (ShouldFire()) Fire(aimpos);
    }

    private void Awake()
    {
        
    }

    

    public bool ShouldFire()
    {
        fireRateTimer += Time.deltaTime;
        if (fireRateTimer < fireRate) return false;
        //if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0)) return true;
        //if (!semiAuto && Input.GetKey(KeyCode.Mouse0)) return true;
        //return false;
        
        return true;
    }

    public void Fire(Transform aimpos,bool isOwner)
    {
        fireRateTimer = 0;

        barrelPos.LookAt(aimpos);
        //barrelPos.localEulerAngles = bloom.BloomAngle(barrelPos); //�Ѿ� ���������� ����


        // Ǯ������ ��ȯ �ʿ�.
        for (int i = 0; i < bulletsPerShot; i++)
        {
            GameObject currentBullet = Instantiate(bullet, barrelPos.position, barrelPos.rotation);
            if (!isOwner)
            {
                Instantiate(muzzleFlash, barrelPos.position, barrelPos.rotation);
                Instantiate(muzzleSmoke, barrelPos.position, barrelPos.rotation);
            }
            
            Bullet bulletScript = currentBullet.GetComponent<Bullet>();
            //bulletScript.weapon = this;

            //bulletScript.dir = barrelPos.transform.forward;

            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            rb.AddForce(barrelPos.forward * bulletVelocity, ForceMode.Impulse);
        }
    }
}
