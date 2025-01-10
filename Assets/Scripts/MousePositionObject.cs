using UnityEngine;

public class MousePositionObject : MonoBehaviour
{
    [SerializeField] private PlayerInputMap playerInputMap;
    [SerializeField] private PlayerShootRay playerShootRay;
    [SerializeField] private Camera cam;
    [SerializeField] private float maxGetPoint = 300f;
    public RectTransform uiElement;
    private bool isHoldShoot = false;

    private void Start()
    {
        playerInputMap.OnHoldShootStart += () => isHoldShoot = true;
        playerInputMap.OnHoldShootCanceled += () => isHoldShoot = false;
    }

    private void Update()
    {
        if (!isHoldShoot) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            transform.position = hit.point;
        }
        else
        {
            transform.position = ray.GetPoint(playerShootRay.RayDistance);
        }
    }
}
