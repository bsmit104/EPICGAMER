using UnityEngine;

public class NoteController : MonoBehaviour
{
    public int      LaneIndex   { get; private set; }
    public float    HitTime     { get; private set; }
    public float    EndNote     { get; private set; }
    public NoteType Type        { get; private set; }
    public bool     WasHit      { get; private set; } = false;
    public bool     IsBeingHeld { get; private set; } = false;

    public const float PERFECT_WINDOW = 0.07f;
    public const float GOOD_WINDOW    = 0.14f;
    public const float BAD_WINDOW     = 0.22f;

    private float  _fallDuration;
    private float  _despawnY;
    private float  _hitZoneY;
    private float  _spawnY;
    private Camera _gameCamera;

    private SpriteRenderer _sr;
    private Color  _baseColor;
    private bool   _flashing   = false;
    private float  _flashTimer = 0f;

    private GameObject     _holdTail;
    private SpriteRenderer _tailSr;
    private float          _holdDuration;
    private float          _holdProgress = 0f;

    public void Init(int lane, float hitTime, float fallDuration,
                     float despawnY, float hitZoneY, Camera cam,
                     NoteType type = NoteType.Tap, float holdDuration = 0f)
    {
        LaneIndex     = lane;
        HitTime       = hitTime;
        Type          = type;
        _holdDuration = holdDuration;
        EndNote       = hitTime + holdDuration;
        _fallDuration = fallDuration;
        _despawnY     = despawnY;
        _hitZoneY     = hitZoneY;
        _gameCamera   = cam;
        _spawnY       = _gameCamera.transform.InverseTransformPoint(transform.position).y;

        if (type == NoteType.Hold && holdDuration > 0f)
            CreateHoldTail();
    }

    void CreateHoldTail()
    {
        _holdTail = new GameObject("HoldTail");
        _holdTail.transform.SetParent(transform);
        _holdTail.layer = gameObject.layer;
        _tailSr = _holdTail.AddComponent<SpriteRenderer>();
        var headSr = GetComponent<SpriteRenderer>();
        if (headSr != null) _tailSr.sprite = headSr.sprite;
        _holdTail.transform.localPosition = Vector3.zero;
        _holdTail.transform.localScale    = Vector3.one;
    }

    public void AssignLaneColor(Color c)
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) { _sr.color = c; _baseColor = c; }
        if (_tailSr != null)
            _tailSr.color = new Color(c.r, c.g, c.b, 0.4f);
    }

    void Update()
    {
        if (_gameCamera == null) return;
        if (_flashing) { HandleFlash(); return; }

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        if (IsBeingHeld)
        {
            HandleHoldUpdate(gm.SongTime);
            return;
        }

        if (WasHit) return;

        // Always keep moving — never stop at hit zone
        float elapsed = gm.SongTime - (HitTime - _fallDuration);
        float localY;

        if (elapsed < 0f)
        {
            // Not yet reached spawn time — sit at spawn
            localY = _spawnY;
        }
        else
        {
            // Continue falling past hit zone all the way to despawn
            float totalDistance = _spawnY - _despawnY;
            float totalTime     = _fallDuration * (totalDistance / Mathf.Abs(_hitZoneY - _spawnY));
            localY = _spawnY - (elapsed / totalTime) * totalDistance;
        }

        Vector3 lp = _gameCamera.transform.InverseTransformPoint(transform.position);
        lp.y = localY;
        transform.position = _gameCamera.transform.TransformPoint(lp);

        if (Type == NoteType.Hold && _holdTail != null)
            UpdateTailScale(gm.SongTime);

        // Destroy when past despawn — register miss only if never hit
        if (localY <= _despawnY)
        {
            if (!WasHit)
                GameManager.Instance?.ScoreManager?.RegisterMiss();
            Destroy(gameObject);
        }
    }

    void UpdateTailScale(float songTime)
    {
        float unitsPerSecond = Mathf.Abs(_hitZoneY - _spawnY) / _fallDuration;
        float tailLength     = _holdDuration * unitsPerSecond;
        float headWorldH     = transform.lossyScale.y;
        if (headWorldH > 0f)
        {
            float scaleY = tailLength / headWorldH;
            _holdTail.transform.localScale    = new Vector3(0.7f, scaleY, 1f);
            _holdTail.transform.localPosition = new Vector3(0f, 0.5f + scaleY * 0.5f, 0f);
        }
    }

    void HandleHoldUpdate(float songTime)
    {
        _holdProgress = Mathf.Clamp01((songTime - HitTime) / _holdDuration);

        if (_sr != null)
        {
            float pulse = Mathf.Sin(Time.time * 18f) * 0.3f + 0.7f;
            _sr.color = Color.Lerp(_baseColor, Color.white * 2.5f, _holdProgress * pulse);
        }

        if (_holdTail != null)
        {
            float unitsPerSecond  = Mathf.Abs(_hitZoneY - _spawnY) / _fallDuration;
            float remainingLength = (_holdDuration * (1f - _holdProgress)) * unitsPerSecond;
            float headWorldH      = transform.lossyScale.y;
            if (headWorldH > 0f)
            {
                float scaleY = remainingLength / headWorldH;
                _holdTail.transform.localScale    = new Vector3(0.7f, Mathf.Max(0.001f, scaleY), 1f);
                _holdTail.transform.localPosition = new Vector3(0f, 0.5f + scaleY * 0.5f, 0f);
            }
        }

        if (songTime >= EndNote)
        {
            IsBeingHeld = false;
            _flashing   = true;
            GameManager.Instance?.ScoreManager?.RegisterHit(HitRating.Perfect);
            return;
        }

        // Safety destroy if stuck
        if (songTime > EndNote + 0.5f)
            Destroy(gameObject);
    }

    public void ReleaseHold(float songTime)
    {
        if (!IsBeingHeld) return;
        IsBeingHeld = false;

        float remaining = EndNote - songTime;
        if (remaining > 0.2f)
        {
            GameManager.Instance?.ScoreManager?.RegisterMiss();
            if (_sr != null) _sr.color = Color.red;
            if (_holdTail != null) Destroy(_holdTail);
            Destroy(gameObject, 0.15f);
        }
    }

    void HandleFlash()
    {
        _flashTimer += Time.deltaTime;
        if (_sr != null)
        {
            float pulse = Mathf.Sin(_flashTimer * 40f) * 0.5f + 0.5f;
            _sr.color = Color.Lerp(_baseColor, Color.white * 3f, pulse * 0.8f);
        }
        if (_flashTimer > 0.2f) Destroy(gameObject);
    }

    public HitRating TryHit(float songTime)
    {
        if (WasHit) return HitRating.None;

        float diff  = Mathf.Abs(songTime - HitTime);
        HitRating r = HitRating.None;
        if      (diff <= PERFECT_WINDOW) r = HitRating.Perfect;
        else if (diff <= GOOD_WINDOW)    r = HitRating.Good;
        else if (diff <= BAD_WINDOW)     r = HitRating.Bad;

        if (r != HitRating.None)
        {
            WasHit = true;
            if (Type == NoteType.Hold)
            {
                IsBeingHeld = true;
                GameManager.Instance?.ScoreManager?.RegisterHit(r);
            }
            else
            {
                _flashing = true;
            }
        }
        return r;
    }

    public float GetTimingDiff(float songTime) => Mathf.Abs(songTime - HitTime);
}

public enum NoteType { Tap, Hold }

// using UnityEngine;

// public class NoteController : MonoBehaviour
// {
//     public int LaneIndex { get; private set; }
//     public float HitTime { get; private set; }
//     public bool WasHit { get; private set; } = false;

//     public const float PERFECT_WINDOW = 0.07f;
//     public const float GOOD_WINDOW = 0.14f;
//     public const float BAD_WINDOW = 0.22f;

//     private float _fallDuration;
//     private float _despawnY;
//     private float _hitZoneY;
//     private float _spawnY;
//     private Camera _gameCamera;

//     private SpriteRenderer _sr;
//     private Color _baseColor;
//     private bool _flashing = false;
//     private float _flashTimer = 0f;

//     public void Init(int lane, float hitTime, float fallDuration,
//                      float despawnY, float hitZoneY, Camera cam)
//     {
//         LaneIndex = lane;
//         HitTime = hitTime;
//         _fallDuration = fallDuration;
//         _despawnY = despawnY;
//         _hitZoneY = hitZoneY;
//         _gameCamera = cam;
//         _spawnY = _gameCamera.transform.InverseTransformPoint(transform.position).y;
//     }

//     public void AssignLaneColor(Color c)
//     {
//         _sr = GetComponent<SpriteRenderer>();
//         if (_sr != null) { _sr.color = c; _baseColor = c; }
//     }

//     void Update()
//     {
//         if (_gameCamera == null) return;  // ADD THIS LINE
//         if (WasHit) { HandleFlash(); return; }

//         // ... rest of Update stays exactly the same

//         GameManager gm = GameManager.Instance;
//         if (gm == null) return;

//         float elapsed = gm.SongTime - (HitTime - _fallDuration);
//         float t = Mathf.Clamp01(elapsed / _fallDuration);
//         float localY = Mathf.Lerp(_spawnY, _hitZoneY, t);

//         // Apply position in camera-local space
//         Vector3 lp = _gameCamera.transform.InverseTransformPoint(transform.position);
//         lp.y = localY;
//         transform.position = _gameCamera.transform.TransformPoint(lp);

//         // Despawn past hit zone
//         if (localY <= _despawnY)
//         {
//             GameManager.Instance?.ScoreManager?.RegisterMiss();
//             Destroy(gameObject);
//         }
//     }

//     void HandleFlash()
//     {
//         _flashTimer += Time.deltaTime;
//         if (_sr != null)
//         {
//             float pulse = Mathf.Sin(_flashTimer * 40f) * 0.5f + 0.5f;
//             _sr.color = Color.Lerp(_baseColor, Color.white * 3f, pulse * 0.8f);
//         }
//         if (_flashTimer > 0.2f) Destroy(gameObject);
//     }

//     public HitRating TryHit(float songTime)
//     {
//         if (WasHit) return HitRating.None;
//         float diff = Mathf.Abs(songTime - HitTime);
//         HitRating r = HitRating.None;
//         if (diff <= PERFECT_WINDOW) r = HitRating.Perfect;
//         else if (diff <= GOOD_WINDOW) r = HitRating.Good;
//         else if (diff <= BAD_WINDOW) r = HitRating.Bad;
//         if (r != HitRating.None) WasHit = true;
//         return r;
//     }

//     public float GetTimingDiff(float songTime) => Mathf.Abs(songTime - HitTime);
// }