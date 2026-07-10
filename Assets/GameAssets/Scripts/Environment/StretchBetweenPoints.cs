using UnityEngine;

[ExecuteInEditMode]
public class StretchBetweenPoints : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float originalLength = 7.159444f;
    public float stretchMultiplier = 1f;

    void Update()
    {
        if (startPoint == null || endPoint == null) return;

        float currentDistance = Vector3.Distance(startPoint.position, endPoint.position);

        transform.position = (startPoint.position + endPoint.position) / 2f;
        transform.LookAt(endPoint.position);

        float globalParentScaleZ = transform.parent != null ? transform.parent.lossyScale.z : 1f;

        float targetScaleZ = (currentDistance / originalLength) * stretchMultiplier;

        if (globalParentScaleZ != 0)
        {
            targetScaleZ /= globalParentScaleZ;
        }

        transform.localScale = new Vector3(1f, 1f, targetScaleZ);
    }
}
