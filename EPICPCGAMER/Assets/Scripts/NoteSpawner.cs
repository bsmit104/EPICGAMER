using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoteSpawner : MonoBehaviour
{
    [Header("References")]
    public Camera     gameCamera;
    public GameObject notePrefab;

    [Header("Settings")]
    public int   gameLayerIndex    = 8;
    public int   laneCount         = 7;
    public float songStartDelay    = 3f;

    [Header("Speed Ramp")]
    public float startFallDuration = 2.5f;
    public float minFallDuration   = 0.8f;
    public float rampRate          = 0.04f;

    public float   HitZoneY           { get; private set; }
    public float[] LaneXPositions     { get; private set; }
    public float   CurrentFallDuration => _fallDuration;

    private float   _fallDuration;
    private float[] _laneX;
    private float   _spawnY, _despawnY;

    // Chart is built as a base pattern that repeats forever
    private List<NoteData> _basePattern = new List<NoteData>();
    private float          _patternLength = 0f;   // duration of one full pattern loop
    private int            _nextNote      = 0;
    private int            _loopCount     = 0;
    private float          _songTimer     = 0f;
    private bool           _playing       = false;

    void Start()
    {
        _fallDuration = startFallDuration;

        var confiner       = gameCamera.GetComponent<GameCameraConfiner>();
        var (halfW, halfH) = confiner.GetScreenHalfExtents();

        _spawnY   =  halfH * 0.9f;
        HitZoneY  = -halfH * 0.70f;
        _despawnY = -halfH * 0.85f;

        float usable = halfW * 0.80f;
        _laneX = new float[laneCount];
        LaneXPositions = _laneX;
        for (int i = 0; i < laneCount; i++)
        {
            float t = (laneCount > 1) ? (float)i / (laneCount - 1) : 0.5f;
            _laneX[i] = Mathf.Lerp(-usable, usable, t);
        }

        BuildBasePattern();
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

        _fallDuration = Mathf.Max(minFallDuration,
            startFallDuration - _songTimer * rampRate);

        // Loop forever — when we exhaust the pattern, start it again offset by patternLength
        while (true)
        {
            if (_nextNote >= _basePattern.Count)
            {
                _nextNote = 0;
                _loopCount++;
            }

            float offset   = _loopCount * _patternLength;
            float noteTime = _basePattern[_nextNote].hitTime + offset;

            if (noteTime > _songTimer + _fallDuration) break;

            NoteData d = new NoteData
            {
                laneIndex    = _basePattern[_nextNote].laneIndex,
                hitTime      = noteTime,
                type         = _basePattern[_nextNote].type,
                holdDuration = _basePattern[_nextNote].holdDuration
            };

            Spawn(d);
            _nextNote++;
        }
    }

    void Spawn(NoteData d)
    {
        float   x   = _laneX[d.laneIndex];
        Vector3 lp  = new Vector3(x, _spawnY, gameCamera.nearClipPlane + 5f);
        Vector3 wp  = gameCamera.transform.TransformPoint(lp);
        var     obj = Instantiate(notePrefab, wp, gameCamera.transform.rotation);

        SetLayer(obj, gameLayerIndex);

        var nc = obj.GetComponent<NoteController>();
        nc.Init(d.laneIndex, d.hitTime, _fallDuration, _despawnY, HitZoneY, gameCamera,
                d.type, d.holdDuration);
        nc.AssignLaneColor(GetLaneColor(d.laneIndex));
    }

    void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform c in go.transform) SetLayer(c.gameObject, layer);
    }

    public Color GetLaneColor(int lane)
    {
        Color[] cols =
        {
            new Color(2f,   0.2f, 0.2f),
            new Color(2f,   0.8f, 0.1f),
            new Color(2f,   2f,   0.1f),
            new Color(0.2f, 2f,   0.2f),
            new Color(0.2f, 1.5f, 2f  ),
            new Color(0.4f, 0.4f, 2f  ),
            new Color(2f,   0.2f, 2f  ),
        };
        return cols[Mathf.Clamp(lane, 0, cols.Length - 1)];
    }

    void BuildBasePattern()
    {
        float beat = 0.5f;
        int[] patt = { 0,2,4,6,1,3,5,0,3,6,2,4,0,1,2,3,4,5,6,
                       0,4,1,5,2,6,3,0,6,3,5,2,4,1,0,2,4,6 };

        for (int i = 0; i < patt.Length; i++)
            _basePattern.Add(new NoteData
            {
                laneIndex = patt[i],
                hitTime   = beat * (i + 1)
            });

        // Hold notes
        float[] holdTimes     = { 4f,   7f,   10f,  13f,  16f  };
        int[]   holdLanes     = { 0,    3,    6,    2,    4    };
        float[] holdDurations = { 0.8f, 1.0f, 0.6f, 1.2f, 0.8f };

        for (int i = 0; i < holdTimes.Length; i++)
            _basePattern.Add(new NoteData
            {
                laneIndex    = holdLanes[i],
                hitTime      = holdTimes[i],
                type         = NoteType.Hold,
                holdDuration = holdDurations[i]
            });

        _basePattern.Sort((a, b) => a.hitTime.CompareTo(b.hitTime));

        // Pattern length = last note hit time + a small gap before looping
        _patternLength = _basePattern[_basePattern.Count - 1].hitTime + beat * 4f;
    }
}

[System.Serializable]
public class NoteData
{
    public int      laneIndex;
    public float    hitTime;
    public NoteType type         = NoteType.Tap;
    public float    holdDuration = 0f;
}

// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;
 
// public class NoteSpawner : MonoBehaviour
// {
//     [Header("References")]
//     public Camera      gameCamera;
//     public GameObject  notePrefab;
 
//     [Header("Settings")]
//     public int   gameLayerIndex = 8;
//     public int   laneCount      = 7;
//     public float songStartDelay = 3f;
 
//     [Header("Speed Ramp")]
//     public float startFallDuration = 2.5f;
//     public float minFallDuration   = 0.8f;
//     public float rampRate          = 0.04f;
 
//     // Public accessors
//     public float   HitZoneY           { get; private set; }
//     public float[] LaneXPositions     { get; private set; }
//     public float   CurrentFallDuration => _fallDuration;
 
//     private float   _fallDuration;
//     private float[] _laneX;
//     private float   _spawnY, _despawnY;
 
//     private List<NoteData> _chart     = new List<NoteData>();
//     private int            _nextNote  = 0;
//     private float          _songTimer = 0f;
//     private bool           _playing   = false;
 
//     void Start()
//     {
//         _fallDuration = startFallDuration;
 
//         var confiner       = gameCamera.GetComponent<GameCameraConfiner>();
//         var (halfW, halfH) = confiner.GetScreenHalfExtents();
 
//         _spawnY   =  halfH * 0.9f;
//         HitZoneY  = -halfH * 0.70f;
//         _despawnY = -halfH * 0.85f;
 
//         float usable = halfW * 0.80f;
//         _laneX = new float[laneCount];
//         LaneXPositions = _laneX;
//         for (int i = 0; i < laneCount; i++)
//         {
//             float t = (laneCount > 1) ? (float)i / (laneCount - 1) : 0.5f;
//             _laneX[i] = Mathf.Lerp(-usable, usable, t);
//         }
 
//         GenerateDemoChart();
//         StartCoroutine(WaitThenPlay());
//     }
 
//     IEnumerator WaitThenPlay()
//     {
//         yield return new WaitForSeconds(songStartDelay);
//         _playing = true;
//     }
 
//     void Update()
//     {
//         if (!_playing) return;
 
//         _songTimer += Time.deltaTime;
 
//         // Ramp speed up over time
//         _fallDuration = Mathf.Max(minFallDuration,
//             startFallDuration - _songTimer * rampRate);
 
//         while (_nextNote < _chart.Count &&
//                _chart[_nextNote].hitTime <= _songTimer + _fallDuration)
//         {
//             Spawn(_chart[_nextNote]);
//             _nextNote++;
//         }
//     }
 
//     void Spawn(NoteData d)
//     {
//         float    x   = _laneX[d.laneIndex];
//         Vector3  lp  = new Vector3(x, _spawnY, gameCamera.nearClipPlane + 5f);
//         Vector3  wp  = gameCamera.transform.TransformPoint(lp);
//         var      obj = Instantiate(notePrefab, wp, gameCamera.transform.rotation);
 
//         SetLayer(obj, gameLayerIndex);
 
//         var nc = obj.GetComponent<NoteController>();
//         nc.Init(d.laneIndex, d.hitTime, _fallDuration, _despawnY, HitZoneY, gameCamera,
//                 d.type, d.holdDuration);
//         nc.AssignLaneColor(GetLaneColor(d.laneIndex));
//     }
 
//     void SetLayer(GameObject go, int layer)
//     {
//         go.layer = layer;
//         foreach (Transform c in go.transform) SetLayer(c.gameObject, layer);
//     }
 
//     public Color GetLaneColor(int lane)
//     {
//         Color[] cols =
//         {
//             new Color(2f,   0.2f, 0.2f),
//             new Color(2f,   0.8f, 0.1f),
//             new Color(2f,   2f,   0.1f),
//             new Color(0.2f, 2f,   0.2f),
//             new Color(0.2f, 1.5f, 2f  ),
//             new Color(0.4f, 0.4f, 2f  ),
//             new Color(2f,   0.2f, 2f  ),
//         };
//         return cols[Mathf.Clamp(lane, 0, cols.Length - 1)];
//     }
 
//     void GenerateDemoChart()
//     {
//         float beat = 0.5f;
//         int[] patt = { 0,2,4,6,1,3,5,0,3,6,2,4,0,1,2,3,4,5,6,
//                        0,4,1,5,2,6,3,0,6,3,5,2,4,1,0,2,4,6 };
 
//         for (int i = 0; i < patt.Length; i++)
//             _chart.Add(new NoteData { laneIndex = patt[i], hitTime = beat * (i + 1) });
 
//         // Hold notes scattered through the chart
//         float[] holdTimes     = { 4f,   7f,   10f,  13f,  16f  };
//         int[]   holdLanes     = { 0,    3,    6,    2,    4    };
//         float[] holdDurations = { 0.8f, 1.0f, 0.6f, 1.2f, 0.8f };
 
//         for (int i = 0; i < holdTimes.Length; i++)
//         {
//             _chart.Add(new NoteData
//             {
//                 laneIndex    = holdLanes[i],
//                 hitTime      = holdTimes[i],
//                 type         = NoteType.Hold,
//                 holdDuration = holdDurations[i]
//             });
//         }
 
//         _chart.Sort((a, b) => a.hitTime.CompareTo(b.hitTime));
//     }
// }
 
// [System.Serializable]
// public class NoteData
// {
//     public int      laneIndex;
//     public float    hitTime;
//     public NoteType type         = NoteType.Tap;
//     public float    holdDuration = 0f;
// }



















// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

// public class NoteSpawner : MonoBehaviour
// {
//     [Header("References")]
//     public Camera      gameCamera;
//     public GameObject  notePrefab;

//     [Header("Settings")]
//     public int   gameLayerIndex = 8;
//     public int   laneCount      = 7;
//     public float songStartDelay = 3f;

//     [Header("Speed Ramp")]
//     public float startFallDuration = 2.5f;   // slow at start
//     public float minFallDuration   = 0.8f;   // fastest it ever gets
//     public float rampRate          = 0.04f;  // seconds reduced per second of playtime

//     // Public accessors
//     public float   HitZoneY        { get; private set; }
//     public float[] LaneXPositions  { get; private set; }
//     public float   CurrentFallDuration => _fallDuration;

//     private float   _fallDuration;
//     private float[] _laneX;
//     private float   _spawnY, _despawnY;

//     private List<NoteData> _chart     = new List<NoteData>();
//     private int            _nextNote  = 0;
//     private float          _songTimer = 0f;
//     private bool           _playing   = false;

//     void Start()
//     {
//         _fallDuration = startFallDuration;

//         var confiner       = gameCamera.GetComponent<GameCameraConfiner>();
//         var (halfW, halfH) = confiner.GetScreenHalfExtents();

//         _spawnY   =  halfH * 0.9f;
//         HitZoneY  = -halfH * 0.70f;
//         _despawnY = -halfH * 0.85f;

//         float usable = halfW * 0.80f;
//         _laneX = new float[laneCount];
//         LaneXPositions = _laneX;
//         for (int i = 0; i < laneCount; i++)
//         {
//             float t = (laneCount > 1) ? (float)i / (laneCount - 1) : 0.5f;
//             _laneX[i] = Mathf.Lerp(-usable, usable, t);
//         }

//         GenerateDemoChart();
//         StartCoroutine(WaitThenPlay());
//     }

//     IEnumerator WaitThenPlay()
//     {
//         yield return new WaitForSeconds(songStartDelay);
//         _playing = true;
//     }

//     void Update()
//     {
//         if (!_playing) return;

//         _songTimer += Time.deltaTime;

//         // Ramp speed up over time
//         _fallDuration = Mathf.Max(minFallDuration,
//             startFallDuration - _songTimer * rampRate);

//         while (_nextNote < _chart.Count &&
//                _chart[_nextNote].hitTime <= _songTimer + _fallDuration)
//         {
//             Spawn(_chart[_nextNote]);
//             _nextNote++;
//         }
//     }

//     void Spawn(NoteData d)
//     {
//         float    x   = _laneX[d.laneIndex];
//         Vector3  lp  = new Vector3(x, _spawnY, gameCamera.nearClipPlane + 5f);
//         Vector3  wp  = gameCamera.transform.TransformPoint(lp);
//         var      obj = Instantiate(notePrefab, wp, gameCamera.transform.rotation);

//         SetLayer(obj, gameLayerIndex);

//         var nc = obj.GetComponent<NoteController>();
//         nc.Init(d.laneIndex, d.hitTime, _fallDuration, _despawnY, HitZoneY, gameCamera);
//         nc.AssignLaneColor(GetLaneColor(d.laneIndex));
//     }

//     void SetLayer(GameObject go, int layer)
//     {
//         go.layer = layer;
//         foreach (Transform c in go.transform) SetLayer(c.gameObject, layer);
//     }

//     public Color GetLaneColor(int lane)
//     {
//         Color[] cols =
//         {
//             new Color(2f,   0.2f, 0.2f),
//             new Color(2f,   0.8f, 0.1f),
//             new Color(2f,   2f,   0.1f),
//             new Color(0.2f, 2f,   0.2f),
//             new Color(0.2f, 1.5f, 2f  ),
//             new Color(0.4f, 0.4f, 2f  ),
//             new Color(2f,   0.2f, 2f  ),
//         };
//         return cols[Mathf.Clamp(lane, 0, cols.Length - 1)];
//     }

//     // Endlessly re-generate chart so the game never stops
//     void GenerateDemoChart()
//     {
//         float beat  = 0.5f;
//         int[] patt  = { 0,2,4,6,1,3,5,0,3,6,2,4,0,1,2,3,4,5,6,
//                          0,4,1,5,2,6,3,0,6,3,5,2,4,1,0,2,4,6 };
//         for (int i = 0; i < patt.Length; i++)
//             _chart.Add(new NoteData { laneIndex = patt[i], hitTime = beat * (i + 1) });

//         // Chords at the end of each phrase
//         for (int i = 0; i < 6; i++)
//         {
//             float t = beat * (patt.Length + 2 + i * 4);
//             _chart.Add(new NoteData { laneIndex = 0, hitTime = t });
//             _chart.Add(new NoteData { laneIndex = 3, hitTime = t });
//             _chart.Add(new NoteData { laneIndex = 6, hitTime = t });
//         }
//         _chart.Sort((a, b) => a.hitTime.CompareTo(b.hitTime));
//     }
// }

// [System.Serializable]
// public class NoteData { public int laneIndex; public float hitTime; }

