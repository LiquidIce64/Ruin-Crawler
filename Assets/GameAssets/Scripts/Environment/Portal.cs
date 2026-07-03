using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Portal : MonoBehaviour, IVehicleTrigger
{
#if UNITY_EDITOR
    [SerializeField] private SceneAsset scene = null;
    private void OnValidate()
    {
        sceneName = scene != null ? scene.name : null;
    }
#endif

    [SerializeField][HideInInspector] private string sceneName;

    public void OnTrigger()
    {
        if (sceneName != null) SceneManager.LoadScene(sceneName);
        else Debug.LogError("No scene specified");
    }
}
