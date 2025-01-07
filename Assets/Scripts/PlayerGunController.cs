using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class PlayerGunController : MonoBehaviour
{
    public EventHandler<OnSwitchGunEventArgs> OnSwitchGun;
    public EventHandler<OnReloadEventArgs> OnUpdatedReloadTimer;
    public EventHandler<OnUpdatedBulletEventArgs> OnUpdatedBullet;

    public class OnSwitchGunEventArgs : EventArgs
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

    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private PlayerInputMap inputMap;
    [SerializeField] private List<GunConfig> gunConfigs;
    [SerializeField] private Transform gunHoldTransform;
    [SerializeField] private LayerMask canShootLayerMarks;
    [SerializeField] private Transform camTransform;

    private bool infiniteBullet;
    private int indexSelectGun;
    private float reloadTimer;
    private bool isReloading;
    private float firingTimer;
    private float timeDelayShoot;
    private float attackMultiplier = 1;
    public bool IsHoldShoot { get; private set; }
    public bool IsShooting { get; private set; }
    private Dictionary<GunConfig, GunController> gunDic = new Dictionary<GunConfig, GunController>();

    public void SetInfiniteBullet(bool isTrue) => infiniteBullet = isTrue;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        inputMap.OnReloadGun += () => Reload();
        inputMap.OnHoldShootStart += () => IsHoldShoot = true;
        inputMap.OnHoldShootCanceled += () => IsHoldShoot = false;
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
        timeDelayShoot = FireRateToTimeDelayShoot(CurrentGunConfig().FireRate); 
        firingTimer = timeDelayShoot;

        OnSwitchGun?.Invoke(this, new OnSwitchGunEventArgs
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

    private float FireRateToTimeDelayShoot(float fireRate) => 1f / fireRate;

    public void SwitchGunNext()
    {
        if (gunConfigs.Count <= 1) return;

        indexSelectGun = (indexSelectGun + 1) % gunConfigs.Count;
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

        if (IsHoldShoot && !isReloading && !CurrentGunController().IsOutOfAllBullet())
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
        firingTimer += Time.fixedDeltaTime;

        if (firingTimer >= timeDelayShoot)
        {
            SpawnBullets(initalPosition, numberOfBullet);
            firingTimer = 0;
            return true;
        }
        return false;
    }

    private void SpawnBullets(Vector3 initalPosition, int numberOfBullet)
    {
        Debug.Log($"Shoot " + numberOfBullet + " bullet");
        GunConfig gunConfig = CurrentGunConfig();
        //Instantiate(gunConfig.ShootingParticle, initalPosition, Quaternion.identity);
        
        if (Physics.Raycast(camTransform.position, 
            GetDirection(camTransform.forward, gunConfig.BulletSpread, gunConfig.BulletSpreadVariance), 
            out RaycastHit hit, 
            float.MaxValue, 
            canShootLayerMarks))
        {
            TrailRenderer trail = Instantiate(gunConfig.BulletTrail, initalPosition, Quaternion.identity);
            StartCoroutine(SpawnTrail(trail, hit, gunConfig.ImpactParticle));
        }
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit, ParticleSystem impactParticle)
    {
        float time = 0;
        Vector3 startPosition = trail.transform.position;

        while (time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += Time.fixedDeltaTime / trail.time;
            yield return null;
        }

        trail.transform.position = hit.point;
        Instantiate(impactParticle, hit.point, Quaternion.LookRotation(hit.normal));
        
        Destroy(trail.gameObject, trail.time);
    }

    private Vector3 GetDirection(Vector3 initialDirection, bool addBulletSpread, Vector3 spreadVarian)
    {
        if (!addBulletSpread) return initialDirection;
        initialDirection += new Vector3(
            UnityEngine.Random.Range(-spreadVarian.x, spreadVarian.x),
            UnityEngine.Random.Range(-spreadVarian.y, spreadVarian.y),
            UnityEngine.Random.Range(-spreadVarian.z, spreadVarian.z)
            );
        return initialDirection;
    }

    private Vector3 CurrentShootPosition() => CurrentGunController().ShootingPosition();
    private GunController CurrentGunController() => gunDic[CurrentGunConfig()];
    public GunConfig CurrentGunConfig() => gunConfigs[indexSelectGun];
    public HandHold CurrentHandHold() => CurrentGunConfig().HandHold;
}
