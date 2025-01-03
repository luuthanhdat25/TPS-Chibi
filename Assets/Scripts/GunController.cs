using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private GunConfig gunConfig;

    public int CurrentBullet { get; private set; }
    public int TotalBullet { get; private set; }

    private void Awake()
    {
        if (gunConfig == null) Debug.LogError($"{gameObject.name} doesn't have GunSO");
        TotalBullet = gunConfig.NumberBulletMax;
        CurrentBullet = gunConfig.NumberBulletReload;
    }

    public int GetBulletShoot()
    {
        return CurrentBullet < gunConfig.NumberBulletShootOneTime 
            ? CurrentBullet 
            : gunConfig.NumberBulletShootOneTime;
    }

    public bool CanReload() => TotalBullet > 0;

    public void Reload()
    {
        if (!CanReload()) return;
        int neededBullets = gunConfig.NumberBulletReload - CurrentBullet;
        int numberBulletsReload = Mathf.Min(neededBullets, TotalBullet);

        CurrentBullet += numberBulletsReload;
        TotalBullet -= numberBulletsReload;
    }

    public void DeductCurrentBullet(int valueDeduct)
    {
        CurrentBullet = CurrentBullet >= valueDeduct
            ? CurrentBullet - valueDeduct
            : 0;
    }

    public bool IsFullCurrentBullet() => CurrentBullet == gunConfig.NumberBulletReload;

    public bool IsOutOfAllBullet() => CurrentBullet <= 0 && TotalBullet <= 0;

    public bool IsOutOfTotalBullet() => TotalBullet <= 0;

    public Vector3 ShootingPosition() => shootingPoint.position;
}
