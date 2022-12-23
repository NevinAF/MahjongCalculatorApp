using UnityEngine;
using UnityEngine.Rendering;

public class PowerManager : Manager<PowerManager>
{
    [SerializeField] bool _enableVSync;

    public bool AppSleeping { get => enabled; set => enabled = value; }

    private void OnEnable()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        OnDemandRendering.renderFrameInterval = 60;
    }

    private void OnDisable()
    {
        QualitySettings.vSyncCount = _enableVSync ? 1 : 0;
        Application.targetFrameRate = 0;
        OnDemandRendering.renderFrameInterval = 0;
    }
    
    void Update()
    {
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            AppSleeping = false;
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        AppSleeping = !focus;
    }
}
