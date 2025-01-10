using UnityEngine;

[CreateAssetMenu(fileName = "GunConfig", menuName = "Config/Gun")]
public class GunConfig : ScriptableObject
{
    public string Name;
    public Sprite DisplayIcon;
    public GameObject Prefab;
    public Sprite CrossHairIcon;
    //public SoundSO ShootSoundSO;
    [Space]
    [Header("Animation")]
    public AnimationClip ShootAnimClip;
    public int NumberAttackInAnimation = 1;
    public HandHold HandHold;

    [Space]
    [Header("Stats")]
    [Range(0.01f, 10f)] public float FireRate = 0.5f; // number bullet per 60 seconds
    [Range(1, 500)] public int Damage = 5;
    [Range(0.1f, 50f)] public float ReloadDuration = 5f;
    [Range(0f, 20f)] public float SwitchGunDuration = 1f;
    [Range(1, 100)] public int NumberBulletShootOneTime = 1;
    [Range(1, 500)] public int NumberBulletReload = 30;
    [Range(1, 500)] public int NumberBulletMax = 60;

    [Space]
    [Header("Bullet Effect")]
    public BulletSpreadConfig BulletSpreadConfig;
    public ParticleSystem ShootingParticle;
    public ParticleSystem ImpactParticle;

    public float TimeDelayShoot() => 1f / FireRate;
}

[System.Serializable]
public struct BulletSpreadConfig
{
    public bool IsSpread;
    public Vector3 BulletSpreadVariance;
}

public enum HandHold
{
    TwoHand,
    OneHand
}