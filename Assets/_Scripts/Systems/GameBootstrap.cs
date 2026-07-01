using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Tooltip("0 = no VSync, 1 = sync to monitor refresh (recommended)")]
    [SerializeField] int vSyncCount = 1;

    [Tooltip("Only used when vSyncCount is 0. -1 = platform default.")]
    [SerializeField] int targetFrameRate = -1;

    void Awake()
    {
        QualitySettings.vSyncCount = vSyncCount;
        Application.targetFrameRate = vSyncCount == 0 ? targetFrameRate : -1;
        DontDestroyOnLoad(gameObject);
    }
}
