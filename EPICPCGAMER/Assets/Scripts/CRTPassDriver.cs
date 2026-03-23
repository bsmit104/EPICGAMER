using UnityEngine;

/// Attach to GameCamera. Animates the CRTMat shader properties at runtime.
public class CRTPassDriver : MonoBehaviour
{
    public Material crtMaterial;

    // Vignette pulse state
    private float _pulseAmount  = 0f;
    private Color _pulseColor   = Color.white;
    private float _pulseDecay   = 4f;

    static readonly int _scanID    = Shader.PropertyToID("_ScanlineScroll");
    static readonly int _brightID  = Shader.PropertyToID("_Brightness");
    static readonly int _vigColID  = Shader.PropertyToID("_VignetteColor");
    static readonly int _vigPulseID= Shader.PropertyToID("_VignettePulse");

    void Update()
    {
        if (crtMaterial == null) return;

        // Scroll scanlines
        crtMaterial.SetFloat(_scanID, Time.time * 0.25f % 1f);

        // Subtle flicker
        float flicker = 1f + Mathf.Sin(Time.time * 47f) * 0.008f;
        crtMaterial.SetFloat(_brightID, flicker);

        // Vignette pulse decay
        _pulseAmount = Mathf.Max(0f, _pulseAmount - Time.deltaTime * _pulseDecay);
        crtMaterial.SetFloat(_vigPulseID, _pulseAmount);
    }

    public void PulseVignette(Color color, float amount)
    {
        _pulseColor  = color;
        _pulseAmount = amount;
        if (crtMaterial != null)
            crtMaterial.SetColor(_vigColID, color);
    }
}