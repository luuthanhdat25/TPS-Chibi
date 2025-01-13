using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [field: SerializeField] public TrailRenderer Trail { get; private set; }
    [SerializeField] private float velocityValue = 50f;
    private Vector3 direction;
    private Action onHitCallback;

    private bool isMoving = false;

    public void Init(Vector3 startPoint, Vector3 endPoint, Action callback = null)
    {
        transform.position = startPoint;
        direction = (endPoint - startPoint).normalized;
        onHitCallback = callback;
        isMoving = true;
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            transform.position += direction * velocityValue * Time.fixedDeltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        isMoving = false;
        onHitCallback?.Invoke();
        Destroy(gameObject);

        Debug.Log("Bullet hit!");
    }
}
