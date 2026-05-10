using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60; 
        // Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
    }
}