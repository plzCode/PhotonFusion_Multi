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
    [SerializeField] private int currentBulletCount= 30;
    [SerializeField] private int maxEquipBulletCount = 30;
    [SerializeField] private int totalBulletCount= 999999;
    int defaultTotalBulletCount=999999;
    public int damage = 10;

    [Header("muzzle Effect")]
    [SerializeField] public GameObject muzzleFlash;
    [SerializeField] GameObject muzzleSmoke;

    [SerializeField] Transform aimpos;

    [Header("recoil Amount")]
    [SerializeField] public float recoilAmount = 2f; // 반동 강도
    [SerializeField] public float recoilRecoverySpeed = 8f; // 반동 복구 속도

    void Update()
    {
        //if (ShouldFire()) Fire(aimpos);
    }

    private void Awake()
    {
        
    }

    public bool NoBullet()
    {
        if(currentBulletCount==0)
        {
            return true;
        }
        return false;
    }

    public void ResetWeapon()
    {
        totalBulletCount = defaultTotalBulletCount;
        currentBulletCount = maxEquipBulletCount;
        UpdateAmmoUI();
    }

    public void Reloading()
    {
        if (totalBulletCount >= maxEquipBulletCount)
        {
            totalBulletCount = totalBulletCount - maxEquipBulletCount;
            currentBulletCount = maxEquipBulletCount;
        }
        else
        {
            currentBulletCount = totalBulletCount;
            totalBulletCount = 0;
        }

        UpdateAmmoUI();
    }

    private void UpdateAmmoUI()
    {
        if (HasInputAuthority)
        {
            InterfaceManager.Instance.ammoDisplay.OnChangeCurrentAmmo(currentBulletCount);
            InterfaceManager.Instance.ammoDisplay.OnChangeReserveAmmo(totalBulletCount);
        }
    }
    public bool ShouldFire()
    {
        fireRateTimer += Time.deltaTime;
        if (fireRateTimer < fireRate) return false;
        if (currentBulletCount <= 0) return false;
        //if (semiAuto && Input.GetKeyDown(KeyCode.Mouse0)) return true;
        //if (!semiAuto && Input.GetKey(KeyCode.Mouse0)) return true;
        //return false;
        
        return true;
    }

    public void Fire(Transform aimpos,bool isOwner,Transform fpsBarrelPos)
    {
        currentBulletCount--;
        UpdateAmmoUI();
        fireRateTimer = 0;

        barrelPos.LookAt(aimpos);
        fpsBarrelPos.LookAt(aimpos);
        //barrelPos.localEulerAngles = bloom.BloomAngle(barrelPos); //총알 퍼짐현상을 구현


        // 풀링으로 변환 필요.
        for (int i = 0; i < bulletsPerShot; i++)
        {
            

            if (!isOwner)
            {
                GameObject currentBullet = Instantiate(bullet, barrelPos.position, barrelPos.rotation);
                Bullet bulletScript = currentBullet.GetComponent<Bullet>();
                //bulletScript.weapon = this;

                //bulletScript.dir = barrelPos.transform.forward;

                Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
                Instantiate(muzzleFlash, barrelPos.position, barrelPos.rotation);
                Instantiate(muzzleSmoke, barrelPos.position, barrelPos.rotation);
                rb.AddForce(barrelPos.forward * bulletVelocity, ForceMode.Impulse);
            }
            else
            {
                GameObject currentBullet = Instantiate(bullet, fpsBarrelPos.position, fpsBarrelPos.rotation);
                Bullet bulletScript = currentBullet.GetComponent<Bullet>();
                //bulletScript.weapon = this;

                //bulletScript.dir = barrelPos.transform.forward;

                Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
                rb.AddForce(fpsBarrelPos.forward * bulletVelocity, ForceMode.Impulse);
            }
            
            
            
        }
    }
}
