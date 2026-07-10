using UnityEngine;

public class PlatformTriggerForwarder : MonoBehaviour
{
    public PlatformAttachment attachment;

    private void OnTriggerEnter(Collider other) => attachment?.HandleTriggerEnter(other);
    private void OnTriggerExit(Collider other) => attachment?.HandleTriggerExit(other);
}