using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGunController : MonoBehaviour
{
    #region Event
    public EventHandler<OnActiveGunEventArgs> OnActiveGun;
    public EventHandler<OnReloadEventArgs> OnUpdatedReloadTimer;
    public EventHandler<OnUpdatedBulletEventArgs> OnUpdatedBullet;

    public class OnActiveGunEventArgs : EventArgs
    {
        public GunConfig CurrentGunConfig;
        public GunConfig NextGunConfig;
    }

    public class OnReloadEventArgs : EventArgs
    {
        public float ReloadTimerNormalize;
    }

    public class OnUpdatedBulletEventArgs : EventArgs
    {
        public float CurrentBullet;
        public float TotalBullet;
    }
    #endregion

    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private PlayerInputMap inputMap;
    [SerializeField] private List<GunConfig> gunConfigs;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerShootRay playerShootRay;
    [SerializeField] private Transform gunHoldTransform;
    [SerializeField] private LayerMask canShootLayerMarks;
    [SerializeField] private Transform camTransform;
    [SerializeField] private Bullet bulletPrefab;

    private bool infiniteBullet;
    private int indexSelectGun;
    private float reloadTimer;
    private bool isReloading;
    private float firingTimer;
    private float timeDelayShoot;
    private float attackMultiplier = 1;
    private bool canFire;
    private float switchGunTimer;
    public bool IsSwitchingGun { get; private set; }
    
    public bool IsShootPressed { get; private set; }
    public bool IsShooting { get; private set; }
    private Dictionary<GunConfig, GunController> gunDic = new Dictionary<GunConfig, GunController>();

    public void SetInfiniteBullet(bool isTrue) => infiniteBullet = isTrue;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inputMap.OnReloadGun += () => Reload();
        inputMap.OnHoldShootStart += () => IsShootPressed = true;
        inputMap.OnHoldShootCanceled += () => IsShootPressed = false;
        inputMap.OnSwitchGun += () => SwitchGunNext();
        InitializeGunControllers();
        ActiveGun(0);
    }

    private void InitializeGunControllers()
    {
        foreach (var item in gunConfigs)
        {
            GameObject gun = Instantiate(item.Prefab, gunHoldTransform);
            GunController gunController = gun.GetComponent<GunController>();
            gunDic.Add(item, gunController);
        }
    }

    private void ActiveGun(int indexSelectGun)
    {
        if (indexSelectGun < 0 || indexSelectGun >= gunConfigs.Count) return;
        animationController.SetIsTwoHand(CurrentHandHold() == HandHold.TwoHand);
        animationController.SetAttackSpeedMuiltiplier(GetTimeMultiplier(CurrentGunConfig()));

        GunConfig gunConfig = gunConfigs[indexSelectGun];
        foreach (var item in gunDic)
        {
            bool isActiveGun = gunConfig == item.Key;
            item.Value.gameObject.SetActive(isActiveGun);
        }

        //Reset timeDelayShoot
        timeDelayShoot = CurrentGunConfig().TimeDelayShoot();
        firingTimer = timeDelayShoot;
        canFire = false;

        OnActiveGun?.Invoke(this, new OnActiveGunEventArgs
        {
            CurrentGunConfig = gunConfig,
            NextGunConfig = gunConfigs[(indexSelectGun + 1) % gunConfigs.Count]
        });
        OnUpdatedBullet?.Invoke(this, new OnUpdatedBulletEventArgs
        {
            CurrentBullet = CurrentGunController().CurrentBullet,
            TotalBullet = CurrentGunController().TotalBullet
        });
    }

    public void SwitchGunNext()
    {
        if (gunConfigs.Count <= 1) return;

        indexSelectGun = (indexSelectGun + 1) % gunConfigs.Count;

        IsSwitchingGun = true;

        ActiveGun(indexSelectGun);

        isReloading = false;
        reloadTimer = 0;

        if (!CurrentGunController().IsOutOfAllBullet() && CurrentGunController().CurrentBullet == 0)
        {
            Reload();
        }
        OnUpdatedReloadTimer?.Invoke(this, new OnReloadEventArgs
        {
            ReloadTimerNormalize = 0
        });
    }

    private float GetTimeMultiplier(GunConfig config)
    {
        return CalculateAnimationAttackMultiplier(
            config.FireRate * attackMultiplier,
            config.ShootAnimClip.length,
            config.NumberAttackInAnimation);
    }

    public static float CalculateAnimationAttackMultiplier(float fireRateTarget, float animationClipLength, int numberAttackInAnimation = 1)
    {
        float timeAttack = animationClipLength / numberAttackInAnimation;
        return fireRateTarget * timeAttack;
    }

    public bool Reload()
    {
        if (infiniteBullet
            || isReloading
            || CurrentGunController().IsFullCurrentBullet()
            || CurrentGunController().IsOutOfAllBullet()
            || CurrentGunController().IsOutOfTotalBullet()) return false;
        isReloading = true;
        reloadTimer = 0;
        OnUpdatedReloadTimer?.Invoke(this, new OnReloadEventArgs
        {
            ReloadTimerNormalize = 0
        });
        return true;
    }

    private void FixedUpdate()
    {
        HandleReload();
        HandleSwitchGun();
        HandleFiringTime();

        if (IsShootPressed 
            && !isReloading 
            && !IsSwitchingGun
            && !CurrentGunController().IsOutOfAllBullet() 
            && playerController.CanShoot())
        {
            HoldShoot();
            IsShooting = true;
        }
        else
        {
            IsShooting = false;
        }
        animationController.SetIsShooting(IsShooting);
    }

    private void HandleFiringTime()
    {
        if (canFire) return;

        firingTimer += Time.fixedDeltaTime;

        if (firingTimer >= timeDelayShoot)
        {
            canFire = true;
            firingTimer = 0;
        }
    }

    private void HandleSwitchGun()
    {
        if (!IsSwitchingGun) return;

        switchGunTimer += Time.fixedDeltaTime;
        if(switchGunTimer >= CurrentGunConfig().SwitchGunDuration)
        {
            IsSwitchingGun = false;
            switchGunTimer = 0;
        }
    }

    private void HandleReload()
    {
        if (!isReloading) return;

        reloadTimer += Time.fixedDeltaTime;
        if (reloadTimer >= CurrentGunConfig().ReloadDuration)
        {
            isReloading = false;
            reloadTimer = 0;
            CurrentGunController().Reload();
            OnUpdatedBullet?.Invoke(this, new OnUpdatedBulletEventArgs
            {
                CurrentBullet = CurrentGunController().CurrentBullet,
                TotalBullet = CurrentGunController().TotalBullet
            });
        }

        OnUpdatedReloadTimer?.Invoke(this, new OnReloadEventArgs
        {
            ReloadTimerNormalize = reloadTimer / CurrentGunConfig().ReloadDuration
        });
    }

    public void HoldShoot()
    {
        if (isReloading) return;

        if (CurrentGunController().IsOutOfAllBullet()) return;
        int numberOfBullets = GetNumberBulletShoot();

        if (numberOfBullets != 0)
        {
            if (ShootBullet(CurrentShootPosition(), numberOfBullets))
            {
                if (!infiniteBullet)
                {
                    CurrentGunController().DeductCurrentBullet(numberOfBullets);
                }
                if (!CurrentGunController().IsOutOfAllBullet() && CurrentGunController().CurrentBullet == 0)
                {
                    Reload();
                }
                OnUpdatedBullet?.Invoke(this, new OnUpdatedBulletEventArgs
                {
                    CurrentBullet = CurrentGunController().CurrentBullet,
                    TotalBullet = CurrentGunController().TotalBullet
                });
            }
        }
        else
        {
            Reload();
        }
    }

    private int GetNumberBulletShoot()
    {
        return infiniteBullet
            ? CurrentGunConfig().NumberBulletShootOneTime
            : CurrentGunController().GetBulletShoot();
    }

    private bool ShootBullet(Vector3 initalPosition, int numberOfBullet)
    {
        if (canFire)
        {
            SpawnBullets(initalPosition, numberOfBullet);
            canFire = false;
            return true;
        }
        return false;
    }

    private void SpawnBullets(Vector3 initalPosition, int numberOfBullet)
    {
        GunConfig gunConfig = CurrentGunConfig();

        Vector3 endPoint = playerShootRay.GetEndpointSpread(CurrentShootPosition(), gunConfig.BulletSpreadConfig);
        Action hitCallback = null;
        var bullet = Instantiate(bulletPrefab, initalPosition, Quaternion.identity);

        if (playerShootRay.RayCastFromGun)
        {
            hitCallback = () => Instantiate(gunConfig.ImpactParticle, endPoint, Quaternion.LookRotation(playerShootRay.RayCastFormGunNormal));
        }

        bullet.Init(initalPosition, endPoint, hitCallback);
    }

    public Vector3 CurrentShootPosition() => CurrentGunController().ShootingPosition();
    private GunController CurrentGunController() => gunDic[CurrentGunConfig()];
    public GunConfig CurrentGunConfig() => gunConfigs[indexSelectGun];
    public HandHold CurrentHandHold() => CurrentGunConfig().HandHold;
}
