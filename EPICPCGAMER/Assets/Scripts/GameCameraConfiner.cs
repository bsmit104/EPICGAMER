using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GameCameraConfiner : MonoBehaviour
{
    [Tooltip("The ScreenQuad child of the monitor. Its lossyScale defines screen size.")]
    public Transform screenQuad;

    public float nearClipOffset = 0.01f;
    public float gameWorldDepth = 20f;

    private Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        _cam.orthographic = true;
    }

    void Start()
    {
        RecalculateFrustum();
    }

    public void RecalculateFrustum()
    {
        if (screenQuad == null || _cam == null) return;

        float worldH = screenQuad.lossyScale.y;
        float worldW = screenQuad.lossyScale.x;

        // Position camera in front of the screen, looking at it straight-on
        Vector3 pos = screenQuad.position + screenQuad.forward * (nearClipOffset + gameWorldDepth * 0.5f);
        transform.position = pos;
        transform.rotation = screenQuad.rotation;

        _cam.orthographicSize = worldH * 0.5f;
        _cam.aspect           = worldW / worldH;
        _cam.nearClipPlane    = nearClipOffset;
        _cam.farClipPlane     = nearClipOffset + gameWorldDepth;
    }

    public (float halfW, float halfH) GetScreenHalfExtents()
    {
        if (screenQuad == null) return (0.16f, 0.12f);
        return (screenQuad.lossyScale.x * 0.5f, screenQuad.lossyScale.y * 0.5f);
    }
}