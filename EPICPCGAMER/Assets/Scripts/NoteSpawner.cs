using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    [Header("References")]
    public Camera gameCamera;
    public GameObject notePrefab;

    [Header("Settings")]
    public int   gameLayerIndex = 8;
    public int   laneCount      = 7;
    public float songStartDelay = 3f;
    public float fallDuration   = 2f;

    // Computed from confiner
    private float[] _laneX;
    private float   _spawnY, _hitZoneY, _despawnY;

    private List<NoteData> _chart = new List<NoteData>();
    private int   _nextNote = 0;
    private float _songTimer = 0f;
    private bool  _playing   = false;

    // Public so InputHandler can read hitZoneY
    public float HitZoneY => _hitZoneY;
    public float[] LaneXPositions => _laneX;

    void Start()
    {
        var confiner = gameCamera.GetComponent<GameCameraConfiner>();
        var (halfW, halfH) = confiner.GetScreenHalfExtents();

        _spawnY   =  halfH * 0.9f;
        _hitZoneY = -halfH * 0.70f;
        _despawnY = -halfH * 0.85f;

        float usable = halfW * 0.80f;
        _laneX = new float[laneCount];
        for (int i = 0; i < laneCount; i++)
        {
            float t = (laneCount > 1) ? (float)i / (laneCount - 1) : 0.5f;
            _laneX[i] = Mathf.Lerp(-usable, usable, t);
        }

        GenerateDemoChart();
        StartCoroutine(WaitThenPlay());
    }

    IEnumerator WaitThenPlay()
    {
        yield return new WaitForSeconds(songStartDelay);
        _playing = true;
    }

    void Update()
    {
        if (!_playing) return;
        _songTimer += Time.deltaTime;

        while (_nextNote < _chart.Count &&
               _chart[_nextNote].hitTime <= _songTimer + fallDuration)
        {
            Spawn(_chart[_nextNote]);
            _nextNote++;
        }
    }

    void Spawn(NoteData d)
    {
        float x        = _laneX[d.laneIndex];
        Vector3 lp     = new Vector3(x, _spawnY, gameCamera.nearClipPlane + 5f);
        Vector3 wp     = gameCamera.transform.TransformPoint(lp);
        GameObject obj = Instantiate(notePrefab, wp, gameCamera.transform.rotation);

        SetLayer(obj, gameLayerIndex);

        var nc = obj.GetComponent<NoteController>();
        nc.Init(d.laneIndex, d.hitTime, fallDuration, _despawnY, _hitZoneY, gameCamera);
        nc.AssignLaneColor(GetLaneColor(d.laneIndex));
    }

    void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform c in go.transform) SetLayer(c.gameObject, layer);
    }

    Color GetLaneColor(int lane)
    {
        Color[] cols =
        {
            new Color(2f, 0.2f, 0.2f),
            new Color(2f, 0.8f, 0.1f),
            new Color(2f, 2f,   0.1f),
            new Color(0.2f, 2f, 0.2f),
            new Color(0.2f, 1.5f, 2f),
            new Color(0.4f, 0.4f, 2f),
            new Color(2f, 0.2f, 2f),
        };
        return cols[Mathf.Clamp(lane, 0, cols.Length - 1)];
    }

    void GenerateDemoChart()
    {
        float beat   = 0.5f; // 120 BPM
        int[] patt   = { 0,2,4,6,1,3,5,0,3,6,2,4,0,1,2,3,4,5,6 };
        for (int i = 0; i < patt.Length; i++)
            _chart.Add(new NoteData { laneIndex = patt[i], hitTime = beat * (i + 1) });
        _chart.Sort((a, b) => a.hitTime.CompareTo(b.hitTime));
    }
}

[System.Serializable]
public class NoteData
{
    public int   laneIndex;
    public float hitTime;
}