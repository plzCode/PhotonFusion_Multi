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
    public float damage = 20;

    [SerializeField] Transform aimpos;

    
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

    public void Fire(Transform aimpos)
    {
        fireRateTimer = 0;

        barrelPos.LookAt(aimpos);
        //barrelPos.localEulerAngles = bloom.BloomAngle(barrelPos); //총알 퍼짐현상을 구현


        // 풀링으로 변환 필요.
        for (int i = 0; i < bulletsPerShot; i++)
        {
            GameObject currentBullet = Instantiate(bullet, barrelPos.position, barrelPos.rotation);

            Bullet bulletScript = currentBullet.GetComponent<Bullet>();
            //bulletScript.weapon = this;

            //bulletScript.dir = barrelPos.transform.forward;

            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            rb.AddForce(barrelPos.forward * bulletVelocity, ForceMode.Impulse);
        }
    }
}
