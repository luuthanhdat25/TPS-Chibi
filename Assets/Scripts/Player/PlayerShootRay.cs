using UnityEngine;

public class PlayerShootRay : MonoBehaviour
{
    [field: SerializeField] public float RayDistance = 100f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerGunController playerGunController;
    [SerializeField] private PlayerInputMap playerInputMap;
    [SerializeField] private Transform aimShootTransform;
    [SerializeField] private LayerMask canShootMark;

    [Header("Shoot error target")]
    [SerializeField] private RectTransform shootErrorIconRect;
    [SerializeField] private Canvas canvas;
    [SerializeField] private float matchRayThrehold = 0.5f;

    private RectTransform shootErrorIconParentRect;
    private Transform camTransform;
    
    private Ray rayFromCam;
    private RaycastHit rayHitFromCam;
    private Ray rayFormGun;
    private RaycastHit rayHitFromGun;
    private Vector3 endPoint;
    public bool RayCastFromCam { get; private set; }
    public bool RayCastFromGun { get; private set; }
    public Vector3 RayCastFormGunNormal => rayHitFromGun.normal;
    private bool isHoldShoot = false;


    private void Awake()
    {
        shootErrorIconParentRect = shootErrorIconRect.parent as RectTransform;
        camTransform = mainCamera.transform;
        rayFromCam = new Ray();
        rayFormGun = new Ray();
    }

    private void Start()
    {
        playerInputMap.OnHoldShootStart += () => isHoldShoot = true;
        playerInputMap.OnHoldShootCanceled += () => isHoldShoot = false;
    }

    private void Update()
    {
        rayFromCam.origin = camTransform.position;
        rayFromCam.direction = camTransform.forward;
        RayCastFromCam = Physics.Raycast(rayFromCam, out rayHitFromCam, RayDistance, canShootMark);
        Vector3 maxRayPoint = rayFromCam.GetPoint(RayDistance);
        Vector3 endpointRayCamera = RayCastFromCam ? rayHitFromCam.point : maxRayPoint;

        if (isHoldShoot)  //Player rig aim shoot transform 
        {
            aimShootTransform.position = endpointRayCamera;
        }

        Vector3 bulletInitalPosition = playerGunController.CurrentShootPosition();
        rayFormGun.origin = bulletInitalPosition;
        rayFormGun.direction = endpointRayCamera - bulletInitalPosition;
        RayCastFromGun = Physics.Raycast(rayFormGun, out rayHitFromGun, RayDistance, canShootMark);

        endPoint = RayCastFromGun ? rayHitFromGun.point : maxRayPoint;

        bool isMatchRay = (!RayCastFromCam && !RayCastFromGun) || Vector3.Distance(rayHitFromCam.point, rayHitFromGun.point) <= matchRayThrehold;
        shootErrorIconRect.gameObject.SetActive(!isMatchRay);

        if (!isMatchRay)
        {
            shootErrorIconRect.SetLocalPositionFromWorldPosition(
                rayHitFromGun.point, 
                shootErrorIconParentRect, 
                canvas.renderMode, 
                mainCamera);
        }
    }

    public Vector3 GetEndpointSpread(Vector3 bulletInitalPosition, BulletSpreadConfig bulletSpreadConfig)
    {
        if (!bulletSpreadConfig.IsSpread) return endPoint;
        Ray rayFromCamSpread = new Ray(GetSpreadDirection(camTransform.position, bulletSpreadConfig), camTransform.forward);
        bool rayCastFromCam = Physics.Raycast(rayFromCamSpread, out RaycastHit rayHitFromCamSpread, RayDistance, canShootMark);

        Vector3 maxRayPoint = rayFromCamSpread.GetPoint(RayDistance);
        Vector3 endpointRayCamera = rayCastFromCam ? rayHitFromCamSpread.point : maxRayPoint;
        aimShootTransform.position = endpointRayCamera;

        Ray rayFormGunSpread = new Ray(bulletInitalPosition, endpointRayCamera - bulletInitalPosition);
        bool rayCastFromGunSpread = Physics.Raycast(rayFormGunSpread, out RaycastHit rayHitFromGunSpread, RayDistance, canShootMark);

        return rayCastFromGunSpread ? rayHitFromGunSpread.point : maxRayPoint;
    }

    private Vector3 GetSpreadDirection(Vector3 initialDirection, BulletSpreadConfig bulletSpreadConfig)
    {
        if (!bulletSpreadConfig.IsSpread) return initialDirection;
        var spreadVarian = bulletSpreadConfig.BulletSpreadVariance;

        initialDirection += new Vector3(
            Random.Range(-spreadVarian.x, spreadVarian.x),
            Random.Range(-spreadVarian.y, spreadVarian.y),
            Random.Range(-spreadVarian.z, spreadVarian.z)
            );
        return initialDirection;
    }
}
