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
    [SerializeField] private bool markLevelCompletion = true;

    public void OnTrigger()
    {
        if (!enabled) return;
        if (markLevelCompletion) SaveManager.MarkLevelCompleted(gameObject.scene.name);

        if (sceneName != null) SceneManager.LoadScene(sceneName);
        else Debug.LogError("No scene specified");
    }
}
