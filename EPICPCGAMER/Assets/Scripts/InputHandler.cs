using UnityEngine;

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

    void Start()
    {
        _spawner = FindFirstObjectByType<NoteSpawner>();
        _hands   = FindFirstObjectByType<HandAnimator>();
        _gm      = GameManager.Instance;
    }

    void Update()
    {
        for (int lane = 0; lane < Keys.Length; lane++)
            if (Input.GetKeyDown(Keys[lane]))
                ProcessLane(lane);
    }

    void ProcessLane(int lane)
    {
        // Flop the hand regardless of whether a note was hit
        _hands?.TriggerFlop(lane);

        // Find closest unhit note in this lane near the hit zone
        NoteController best  = null;
        float          bestD = float.MaxValue;

        foreach (var nc in FindObjectsByType<NoteController>(FindObjectsSortMode.None))
        {
            if (nc.LaneIndex != lane || nc.WasHit) continue;

            float noteLocalY = _spawner.gameCamera
                .transform.InverseTransformPoint(nc.transform.position).y;

            // Only consider notes within 2 units of the hit zone
            if (Mathf.Abs(noteLocalY - _spawner.HitZoneY) > 2f) continue;

            float d = nc.GetTimingDiff(_gm.SongTime);
            if (d < bestD) { bestD = d; best = nc; }
        }

        if (best != null)
        {
            HitRating r = best.TryHit(_gm.SongTime);
            if (r != HitRating.None) _gm.ScoreManager.RegisterHit(r);
            else                     _gm.ScoreManager.RegisterMiss();
        }
        else
        {
            _gm.ScoreManager.RegisterMiss();
        }
    }
}