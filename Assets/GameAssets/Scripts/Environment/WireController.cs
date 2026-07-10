using UnityEngine;

public class WireController : MonoBehaviour
{
    // все объекты должны быть дочерними элементами объекта с данным скриптом
    private Renderer[] wireSegments;

    private void Start()
    {
        wireSegments = GetComponentsInChildren<Renderer>();
    }

    public void TurnOnGlow()
    {
        if (wireSegments == null) return;

        foreach (Renderer segment in wireSegments)
        {
            if (segment != null)
            {
                segment.material.EnableKeyword("_EMISSION");
            }
        }
    }

    public void TurnOffGlow()
    {
        if (wireSegments == null) return;

        foreach (Renderer segment in wireSegments)
        {
            if (segment != null)
            {
                segment.material.DisableKeyword("_EMISSION");
            }
        }
    }
}
