using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovableObject : MonoBehaviour, IWinchInteractable
{
    public bool AutoDetach => false;

    public void Interact()
    {
        Debug.Log("Лебедка прицепилась к объекту!");
    }
}