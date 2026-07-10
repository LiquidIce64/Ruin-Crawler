using UnityEngine;
using System.Collections.Generic;

public class PlatformAttachment : MonoBehaviour
{
    private MovingPlatformZ platform;
    private Rigidbody platformRb;
    private HashSet<Rigidbody> vehiclesOnPlatform = new HashSet<Rigidbody>();
    private Dictionary<Rigidbody, float> heightOffsets = new Dictionary<Rigidbody, float>();

    private void Start()
    {
        platform = GetComponent<MovingPlatformZ>();
        platformRb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (platform == null || platformRb == null) return;

        Vector3 center = platformRb.position;
        bool isMoving = platform.IsMoving;

        foreach (var rb in vehiclesOnPlatform)
        {
            if (rb == null) continue;

            if (isMoving)
            {
                if (!heightOffsets.ContainsKey(rb))
                    heightOffsets[rb] = rb.position.y - center.y;

                float yOffset = heightOffsets[rb];
                Vector3 targetPos = new Vector3(center.x, center.y + yOffset, center.z);

                rb.MovePosition(targetPos);

                rb.linearVelocity = platformRb.linearVelocity;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                heightOffsets.Remove(rb);
            }
        }
    }

    public void HandleTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && rb.TryGetComponent<VehicleController>(out _))
            vehiclesOnPlatform.Add(rb);
    }

    public void HandleTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && rb.TryGetComponent<VehicleController>(out _))
        {
            vehiclesOnPlatform.Remove(rb);
            heightOffsets.Remove(rb);
        }
    }
}