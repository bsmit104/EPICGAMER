using UnityEngine;

public class NoteController : MonoBehaviour
{
    public int   LaneIndex  { get; private set; }
    public float HitTime    { get; private set; }
    public bool  WasHit     { get; private set; } = false;

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

    public void Init(int lane, float hitTime, float fallDuration,
                     float despawnY, float hitZoneY, Camera cam)
    {
        LaneIndex     = lane;
        HitTime       = hitTime;
        _fallDuration = fallDuration;
        _despawnY     = despawnY;
        _hitZoneY     = hitZoneY;
        _gameCamera   = cam;
        _spawnY       = _gameCamera.transform.InverseTransformPoint(transform.position).y;
    }

    public void AssignLaneColor(Color c)
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) { _sr.color = c; _baseColor = c; }
    }

    void Update()
    {
        if (WasHit) { HandleFlash(); return; }

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        float elapsed = gm.SongTime - (HitTime - _fallDuration);
        float t       = Mathf.Clamp01(elapsed / _fallDuration);
        float localY  = Mathf.Lerp(_spawnY, _hitZoneY, t);

        // Apply position in camera-local space
        Vector3 lp = _gameCamera.transform.InverseTransformPoint(transform.position);
        lp.y = localY;
        transform.position = _gameCamera.transform.TransformPoint(lp);

        // Despawn past hit zone
        if (localY <= _despawnY)
        {
            GameManager.Instance?.ScoreManager?.RegisterMiss();
            Destroy(gameObject);
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
        float diff = Mathf.Abs(songTime - HitTime);
        HitRating r = HitRating.None;
        if      (diff <= PERFECT_WINDOW) r = HitRating.Perfect;
        else if (diff <= GOOD_WINDOW)    r = HitRating.Good;
        else if (diff <= BAD_WINDOW)     r = HitRating.Bad;
        if (r != HitRating.None) WasHit = true;
        return r;
    }

    public float GetTimingDiff(float songTime) => Mathf.Abs(songTime - HitTime);
}