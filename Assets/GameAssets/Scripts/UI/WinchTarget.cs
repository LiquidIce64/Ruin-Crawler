using UnityEngine;

public class WinchTarget : MonoBehaviour
{
    [SerializeField] private GameObject sprite;
    [SerializeField] private GameObject frontWinchKey;
    [SerializeField] private GameObject backWinchKey;
    private Rigidbody attachmentPoint;
    private bool frontWinchAvailable = false;
    private bool backWinchAvailable = false;

    public Rigidbody AttachmentPoint => attachmentPoint;
    public bool FrontWinchAvailable => frontWinchAvailable;
    public bool BackWinchAvailable => backWinchAvailable;

    public void HandleUpdate(Winch frontWinch, Winch backWinch)
    {
        attachmentPoint = null;
        float closestDistance = 0f;
        Vector3 closestScreenPos = Vector3.zero;
        foreach (var point in frontWinch.FindAttachmentPoints())
        {
            var screenPos = Camera.main.WorldToViewportPoint(point.position);
            if (screenPos.z < 0f) continue;
            screenPos.z = 0f;
            screenPos -= Vector3.one / 2;
            float distance = screenPos.sqrMagnitude;
            if (attachmentPoint == null || distance < closestDistance)
            {
                attachmentPoint = point;
                closestDistance = distance;
                closestScreenPos = screenPos;
            }
        }
        foreach (var point in backWinch.FindAttachmentPoints())
        {
            var screenPos = Camera.main.WorldToViewportPoint(point.position);
            if (screenPos.z < 0f) continue;
            screenPos.z = 0f;
            screenPos -= Vector3.one / 2;
            float distance = screenPos.sqrMagnitude;
            if (attachmentPoint == null || distance < closestDistance)
            {
                attachmentPoint = point;
                closestDistance = distance;
                closestScreenPos = screenPos;
            }
        }

        if (attachmentPoint == null)
        {
            frontWinchAvailable = false;
            backWinchAvailable = false;
            sprite.SetActive(false);
        }
        else
        {
            frontWinchAvailable = !frontWinch.IsAttached && Vector3.Distance(
                frontWinch.transform.position, attachmentPoint.position) <= frontWinch.MaxDistance;
            backWinchAvailable = !backWinch.IsAttached && Vector3.Distance(
                backWinch.transform.position, attachmentPoint.position) <= backWinch.MaxDistance;
            sprite.SetActive(frontWinchAvailable || backWinchAvailable);
            transform.localPosition = closestScreenPos * (transform.parent as RectTransform).rect.size;
        }
        frontWinchKey.SetActive(frontWinchAvailable);
        backWinchKey.SetActive(backWinchAvailable);
    }
}
