using UnityEngine;

public class MousePositionObject : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask targetMark;

    private void Update()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetMark))
        {
            transform.position = hit.point;
        }
    }
}
