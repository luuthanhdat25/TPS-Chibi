using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GunStatusUI : MonoBehaviour
{
    [SerializeField] private Image usingGun;
    [SerializeField] private Image nextGun;
    [SerializeField] private TextMeshProUGUI gunName;
    [SerializeField] private TextMeshProUGUI bulletCountText;
    [SerializeField] private PlayerGunController playerGunController;
    [SerializeField] private GameObject reloadBarGameObject;
    [SerializeField] private Image reloadBarImage;

    private void Awake()
    {
        playerGunController.OnUpdatedReloadTimer += PlayerGunSelector_OnUpdatedReloadTimer;
        playerGunController.OnSwitchGun += PlayerGunSelector_OnSwitchGun;
        playerGunController.OnUpdatedBullet += PlayerGunSelector_OnUpdatedBullet;
        reloadBarGameObject.SetActive(false);
    }

    private void PlayerGunSelector_OnUpdatedBullet(object sender, PlayerGunController.OnUpdatedBulletEventArgs e)
    {
        bulletCountText.text = e.CurrentBullet + "/" + e.TotalBullet;
    }

    private void PlayerGunSelector_OnUpdatedReloadTimer(object sender, PlayerGunController.OnReloadEventArgs e)
    {
        reloadBarImage.fillAmount = e.ReloadTimerNormalize;
        reloadBarGameObject.SetActive(e.ReloadTimerNormalize != 0 && e.ReloadTimerNormalize != 1);
    }

    private void PlayerGunSelector_OnSwitchGun(object sender, PlayerGunController.OnSwitchGunEventArgs e)
    {
        gunName.text = e.CurrentGunConfig.Name;
        usingGun.sprite = e.CurrentGunConfig.DisplayIcon;
        nextGun.sprite = e.NextGunConfig.DisplayIcon;
    }
}
