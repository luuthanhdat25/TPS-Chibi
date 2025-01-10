using System;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [field: SerializeField] public TrailRenderer Trail { get; private set; }
    [SerializeField] private float velocityValue = 50f;

    public void Init(Vector3 startPoint, Vector3 endPoint, Action callback = null)
    {
        float distanceTravel = Vector3.Distance(startPoint, endPoint);
        float timeTravel = distanceTravel / velocityValue;
        StartCoroutine(MoveToEndPoint(startPoint, endPoint, timeTravel, callback));
    }

    private IEnumerator MoveToEndPoint(Vector3 startPoint, Vector3 endPoint, float timeTravel, Action callback)
    {
        float elapsedTime = 0;

        while (elapsedTime < timeTravel)
        {
            transform.position = Vector3.Lerp(startPoint, endPoint, elapsedTime / timeTravel);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endPoint;
        callback?.Invoke();
        Destroy(gameObject);
    }
}
