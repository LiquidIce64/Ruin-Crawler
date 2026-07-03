using UnityEngine;

public class GravityVolume : MonoBehaviour
{
    [SerializeField] private Vector3 gravityVector = Vector3.zero;

    private void OnTriggerStay(Collider other)
    {
        if (other.attachedRigidbody == null) return;
        other.attachedRigidbody.AddForce(gravityVector - Physics.gravity, ForceMode.Acceleration);
    }
}
