using System.Collections.Generic;
using UnityEngine;

public class Winch : MonoBehaviour
{
    private static int layerMask;

    [SerializeField] private SpringJoint joint;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float winchSpeed = 1f;
    private GameObject rope;
    private readonly Collider[] hitColliders = new Collider[10]; //pre-allocating memory for performance
    private float springForce;
    [HideInInspector] public bool pull = false;

    public float MaxDistance => maxDistance;
    public bool IsAttached => joint.connectedBody != null;

    private void Awake()
    {
        rope = transform.GetChild(0).gameObject;
        rope.SetActive(false);
        layerMask = LayerMask.GetMask("WinchPoints");
        joint.anchor = transform.localPosition;
        springForce = joint.spring;
        joint.spring = 0f;
    }

    public IEnumerable<Rigidbody> FindAttachmentPoints()
    {
        int hits = Physics.OverlapSphereNonAlloc(transform.position, maxDistance, hitColliders, layerMask);
        for (int i = 0; i < hits; i++)
        {
            yield return hitColliders[i].attachedRigidbody;
        }
    }

    public void Attach(Rigidbody attachmentPoint)
    {
        if (attachmentPoint == null)
        {
            Detach();
            return;
        }
        joint.connectedBody = attachmentPoint;
        joint.spring = springForce;
        joint.maxDistance = Vector3.Distance(transform.position, attachmentPoint.position);
        rope.SetActive(true);
    }

    public void Detach()
    {
        pull = false;
        joint.connectedBody = null;
        joint.spring = 0f;
        rope.SetActive(false);
    }

    private void Update()
    {
        if (joint.connectedBody == null) return;
        transform.LookAt(joint.connectedBody.position);
        transform.localScale = new Vector3(1f, 1f, Vector3.Distance(transform.position, joint.connectedBody.position));

        if (pull && joint.maxDistance > minDistance)
            joint.maxDistance = Mathf.Max(joint.maxDistance - winchSpeed * Time.deltaTime, minDistance);
    }
}
