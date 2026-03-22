using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class MonitorScreenController : MonoBehaviour
{
    [Header("References")]
    public Renderer screenQuadRenderer;
    public Light monitorAreaLight;
    public RenderTexture gameRenderTexture;

    [Header("Settings")]
    public float baseEmission = 2.5f;
    public float baseLightLux = 900f;
    public float hitPulseExtra = 400f;
    public float pulseDecay = 8f;

    private Material _mat;
    private HDAdditionalLightData _hdLight;
    private float _pulse = 0f;

    void Start()
    {
        _mat = screenQuadRenderer.material;
        _hdLight = monitorAreaLight?.GetComponent<HDAdditionalLightData>();

        if (_mat != null && gameRenderTexture != null)
        {
            _mat.SetTexture("_BaseColorMap", gameRenderTexture);
            _mat.SetTexture("_EmissiveColorMap", gameRenderTexture);
            _mat.EnableKeyword("_EMISSION");
            _mat.SetColor("_EmissiveColor", Color.white * baseEmission);
        }
    }

    void Update()
    {
        _pulse = Mathf.Max(0f, _pulse - Time.deltaTime * pulseDecay);
        if (monitorAreaLight != null)
            monitorAreaLight.intensity = baseLightLux + _pulse;
    }
    // void Update()
    // {
    //     _pulse = Mathf.Max(0f, _pulse - Time.deltaTime * pulseDecay);
    //     if (_hdLight != null)
    //         _hdLight.intensity = baseLightLux + _pulse;
    // }

    public void TriggerPulse() => _pulse = hitPulseExtra;
}