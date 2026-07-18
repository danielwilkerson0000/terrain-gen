using UnityEngine;

/// <summary>
/// THIS WAS LARGELY GENERATED WITH THE HELP OF GOOGLE GEMINI - MANY THANKS.
/// </summary>
public class FramerateManager : MonoBehaviour
{
    void Awake()
    {
        // 1. Turn off VSync (0 = Don't Sync)
        QualitySettings.vSyncCount = 0;

        // 2. Set the target frame rate 
        Application.targetFrameRate = 30;
    }
}