public interface IWinchInteractable
{
    public bool AutoDetach => true; 
    public void Interact();

    public void OnDetach() { }
}
