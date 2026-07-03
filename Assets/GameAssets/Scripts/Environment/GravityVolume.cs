using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class GravityVolume : MonoBehaviour
{
    [SerializeField] private Vector3 gravityVector = Vector3.zero;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.purple;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }

    private void OnValidate()
    {
        var particleSystem = GetComponent<ParticleSystem>();
        var forceModule = particleSystem.forceOverLifetime;
        forceModule.xMultiplier = gravityVector.x;
        forceModule.yMultiplier = gravityVector.y;
        forceModule.zMultiplier = gravityVector.z;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody == null) return;
        other.attachedRigidbody.AddForce(gravityVector - Physics.gravity, ForceMode.Acceleration);
    }
}
