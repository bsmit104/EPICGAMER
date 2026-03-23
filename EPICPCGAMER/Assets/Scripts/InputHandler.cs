using UnityEngine;
using System.Collections.Generic;

public class InputHandler : MonoBehaviour
{
    private static readonly KeyCode[] Keys =
    {
        KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
        KeyCode.J, KeyCode.K, KeyCode.L
    };

    private NoteSpawner  _spawner;
    private HandAnimator _hands;
    private GameManager  _gm;
    private HitParticles _particles;

    // Track which note is currently being held per lane
    private NoteController[] _heldNotes = new NoteController[7];

    void Start()
    {
        _spawner   = FindFirstObjectByType<NoteSpawner>();
        _hands     = FindFirstObjectByType<HandAnimator>();
        _gm        = GameManager.Instance;
        _particles = FindFirstObjectByType<HitParticles>();
    }

    void Update()
    {
        for (int lane = 0; lane < Keys.Length; lane++)
        {
            if (Input.GetKeyDown(Keys[lane]))  PressLane(lane);
            if (Input.GetKeyUp(Keys[lane]))    ReleaseLane(lane);
        }
    }

    void PressLane(int lane)
    {
        _hands?.TriggerFlop(lane);

        NoteController best  = null;
        float          bestD = float.MaxValue;

        foreach (var nc in FindObjectsByType<NoteController>(FindObjectsSortMode.None))
        {
            if (nc.LaneIndex != lane || nc.WasHit) continue;
            float noteLocalY = _spawner.gameCamera
                .transform.InverseTransformPoint(nc.transform.position).y;
            if (Mathf.Abs(noteLocalY - _spawner.HitZoneY) > 2f) continue;
            float d = nc.GetTimingDiff(_gm.SongTime);
            if (d < bestD) { bestD = d; best = nc; }
        }

        if (best != null)
        {
            HitRating r = best.TryHit(_gm.SongTime);
            if (r != HitRating.None)
            {
                _gm.ScoreManager.RegisterHit(r);
                _particles?.SpawnBurst(lane, _spawner.HitZoneY,
                    _spawner.LaneXPositions[lane],
                    _spawner.GetLaneColor(lane), r);

                // Track held note so we can release it
                if (best.Type == NoteType.Hold)
                    _heldNotes[lane] = best;
            }
            else _gm.ScoreManager.RegisterMiss();
        }
        else _gm.ScoreManager.RegisterMiss();
    }

    void ReleaseLane(int lane)
    {
        if (_heldNotes[lane] != null)
        {
            _heldNotes[lane].ReleaseHold(_gm.SongTime);
            _heldNotes[lane] = null;
        }
    }
}

// using UnityEngine;

// public class InputHandler : MonoBehaviour
// {
//     private static readonly KeyCode[] Keys =
//     {
//         KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.F,
//         KeyCode.J, KeyCode.K, KeyCode.L
//     };

//     private NoteSpawner  _spawner;
//     private HandAnimator _hands;
//     private GameManager  _gm;
//     private HitParticles _particles;

//     void Start()
//     {
//         _spawner   = FindFirstObjectByType<NoteSpawner>();
//         _hands     = FindFirstObjectByType<HandAnimator>();
//         _gm        = GameManager.Instance;
//         _particles = FindFirstObjectByType<HitParticles>();
//     }

//     void Update()
//     {
//         for (int lane = 0; lane < Keys.Length; lane++)
//             if (Input.GetKeyDown(Keys[lane]))
//                 ProcessLane(lane);
//     }

//     void ProcessLane(int lane)
//     {
//         _hands?.TriggerFlop(lane);

//         NoteController best  = null;
//         float          bestD = float.MaxValue;

//         foreach (var nc in FindObjectsByType<NoteController>(FindObjectsSortMode.None))
//         {
//             if (nc.LaneIndex != lane || nc.WasHit) continue;
//             float noteLocalY = _spawner.gameCamera
//                 .transform.InverseTransformPoint(nc.transform.position).y;
//             if (Mathf.Abs(noteLocalY - _spawner.HitZoneY) > 2f) continue;
//             float d = nc.GetTimingDiff(_gm.SongTime);
//             if (d < bestD) { bestD = d; best = nc; }
//         }

//         if (best != null)
//         {
//             HitRating r = best.TryHit(_gm.SongTime);
//             if (r != HitRating.None)
//             {
//                 _gm.ScoreManager.RegisterHit(r);

//                 // Spawn particles at hit position
//                 _particles?.SpawnBurst(
//                     lane,
//                     _spawner.HitZoneY,
//                     _spawner.LaneXPositions[lane],
//                     _spawner.GetLaneColor(lane),
//                     r);
//             }
//             else _gm.ScoreManager.RegisterMiss();
//         }
//         else _gm.ScoreManager.RegisterMiss();
//     }
// }
