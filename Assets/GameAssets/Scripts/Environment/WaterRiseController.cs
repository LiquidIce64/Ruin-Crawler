using System.Collections;
using UnityEngine;

public class WaterRiseController : MonoBehaviour
{
    [SerializeField] private Transform waterTransform;
    [SerializeField] private float riseHeight = 100f;
    [SerializeField] private float riseSpeed = 1.5f;
    [SerializeField] private bool isRising = false;

    private float startY;
    private float targetY;
    private Coroutine riseCoroutine;

    private void Start()
    {
        if (waterTransform != null)
            startY = waterTransform.position.y;
        targetY = startY;
    }

    public void StartRise()
    {
        targetY = startY + riseHeight;
        if (!isRising)
        {
            isRising = true;
            if (riseCoroutine != null) StopCoroutine(riseCoroutine);
            riseCoroutine = StartCoroutine(MoveWater());
        }
    }

    public void StopRise()
    {
        targetY = startY;
        if (!isRising)
        {
            isRising = true;
            if (riseCoroutine != null) StopCoroutine(riseCoroutine);
            riseCoroutine = StartCoroutine(MoveWater());
        }
    }

    private IEnumerator MoveWater()
    {
        while (Mathf.Abs(waterTransform.position.y - targetY) > 0.01f)
        {
            float newY = Mathf.MoveTowards(waterTransform.position.y, targetY, riseSpeed * Time.deltaTime);
            waterTransform.position = new Vector3(waterTransform.position.x, newY, waterTransform.position.z);
            yield return null;
        }
        waterTransform.position = new Vector3(waterTransform.position.x, targetY, waterTransform.position.z);
        isRising = false;
    }
}