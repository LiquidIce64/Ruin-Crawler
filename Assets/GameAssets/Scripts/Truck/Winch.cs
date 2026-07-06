using System.Collections.Generic;
using UnityEngine;

public class Winch : MonoBehaviour
{
    private static int defaultLayerMask;
    private static int winchLayerMask;

    [SerializeField] private SpringJoint joint;
    private float maxDistance = 15f;
    private float minDistance = 1f;
    private float winchSpeed = 2f;
    private GameObject rope;
    private readonly Collider[] hitColliders = new Collider[10]; //pre-allocating memory for performance
    private float springForce;
    [HideInInspector] public bool pull = false;
    [HideInInspector] public bool extend = false;

    private float ropeAnimSpeed = 2f;
    private float ropeAnimT = 0f;
    private float lastRopeDistance = 0f;

    private float autoShortenTimer = 0f;
    private bool autoShortenActive = false;

    public float MaxDistance => maxDistance;
    public bool IsAttached => joint.connectedBody != null;

    private void Awake()
    {
        rope = transform.GetChild(0).gameObject;
        rope.SetActive(false);
        defaultLayerMask = LayerMask.GetMask("Default");
        winchLayerMask = LayerMask.GetMask("WinchInteractable");
        joint.anchor = transform.localPosition;
        springForce = joint.spring;
        joint.spring = 0f;
    }

    public IEnumerable<Rigidbody> FindAttachmentPoints()
    {
        int hits = Physics.OverlapSphereNonAlloc(transform.position, maxDistance, hitColliders, winchLayerMask);
        for (int i = 0; i < hits; i++)
        {
            var rigidBody = hitColliders[i].attachedRigidbody;
            if (rigidBody == null)
            {
                Debug.LogWarning("WinchInteractable has no attached RigidBody");
                continue;
            }

            // Check for line of sight
            Vector3 dir = rigidBody.position - transform.position;
            if (Physics.Raycast(transform.position, dir.normalized, dir.magnitude, defaultLayerMask)) continue;

            yield return rigidBody;
        }
    }

    public void Attach(Rigidbody attachmentPoint)
    {
        if (attachmentPoint == null)
        {
            Detach();
            return;
        }
        if (attachmentPoint.gameObject.TryGetComponent(out IWinchInteractable component))
        {
            component.Interact();
            if (component.AutoDetach)
            {
                transform.LookAt(attachmentPoint.position);
                lastRopeDistance = Vector3.Distance(transform.position, attachmentPoint.position);
                rope.SetActive(true);
                Detach();
                return;
            }
        }

        joint.connectedBody = attachmentPoint;
        joint.spring = springForce;
        float initialDistance = Vector3.Distance(transform.position, attachmentPoint.position);
        joint.maxDistance = Mathf.Clamp(initialDistance, minDistance, maxDistance);
        autoShortenActive = true;
        autoShortenTimer = 2f;
        rope.SetActive(true);
    }

    public void Detach()
    {
        if (joint.connectedBody != null && joint.connectedBody.gameObject.TryGetComponent(out IWinchInteractable component))
        {
            component.OnDetach();
        }

        pull = false;
        extend = false;
        joint.connectedBody = null;
        joint.spring = 0f;
        ropeAnimT = 1f;
    }

    private void Update()
    {
        if (joint.connectedBody == null)
        {
            ropeAnimT = Mathf.Clamp(ropeAnimT - ropeAnimSpeed * Time.deltaTime, 0f, 1f);
            transform.localScale = new Vector3(1f, 1f, lastRopeDistance * ropeAnimT * ropeAnimT);
            rope.SetActive(ropeAnimT > 0f);
            return;
        }
        transform.LookAt(joint.connectedBody.position);
        float actualDistance = Vector3.Distance(transform.position, joint.connectedBody.position);
        transform.localScale = new Vector3(1f, 1f, actualDistance);
        lastRopeDistance = actualDistance;

        // Автоматическое укорачивание только первые 2 секунды
        if (autoShortenActive)
        {
            autoShortenTimer -= Time.deltaTime;
            if (autoShortenTimer > 0f && actualDistance < joint.maxDistance)
            {
                joint.maxDistance = Mathf.Max(actualDistance, minDistance);
            }
            if (autoShortenTimer <= 0f)
            {
                autoShortenActive = false;
            }
        }

        if (extend && joint.maxDistance < maxDistance)
            joint.maxDistance = Mathf.Min(joint.maxDistance + winchSpeed * Time.deltaTime, maxDistance);
        else if (pull && joint.maxDistance > minDistance)
            joint.maxDistance = Mathf.Max(joint.maxDistance - winchSpeed * Time.deltaTime, minDistance);
    }
}