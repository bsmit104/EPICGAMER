using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Text References")]
    public TextMeshProUGUI accuracyLabel;
    public TextMeshProUGUI comboLabel;
    public TextMeshProUGUI speedLabel;
    public TextMeshProUGUI ultimateScoreLabel;  // only shows in Ultimate mode
    public TextMeshProUGUI ultimateTimeLabel;   // only shows in Ultimate mode

    [Header("Combo Shake")]
    public float shakeIntensity = 3f;
    public float shakeFalloff   = 8f;

    [Header("Milestone Colors")]
    public Color color10 = new Color(0.2f, 1f,   0.2f);
    public Color color25 = new Color(1f,   0.8f, 0.1f);
    public Color color50 = new Color(1f,   0.2f, 2f  );

    private ScoreManager      _score;
    private NoteSpawner       _spawner;
    private CRTPassDriver     _crt;
    private GameModeManager   _modeMgr;

    private int     _lastCombo    = 0;
    private float   _shakeCurrent = 0f;
    private Vector3 _comboBasePos;

    void Start()
    {
        _score   = FindFirstObjectByType<ScoreManager>();
        _spawner = FindFirstObjectByType<NoteSpawner>();
        _crt     = FindFirstObjectByType<CRTPassDriver>();
        _modeMgr = GameModeManager.Instance;

        if (comboLabel != null)
            _comboBasePos = comboLabel.rectTransform.anchoredPosition;
    }

    void Update()
    {
        if (_score == null) return;

        if (accuracyLabel != null)
            accuracyLabel.text = $"{_score.Accuracy:F1}%";

        if (speedLabel != null && _spawner != null)
        {
            float mult = _spawner.startFallDuration / _spawner.CurrentFallDuration;
            speedLabel.text = $"SPD x{mult:F1}";
        }

        // Ultimate Gamer live score
        bool isUltimate = _modeMgr != null &&
                          _modeMgr.CurrentMode == GameMode.UltimateGamer;

        if (ultimateScoreLabel != null)
        {
            ultimateScoreLabel.gameObject.SetActive(isUltimate);
            if (isUltimate)
                ultimateScoreLabel.text = $"SCORE\n{_modeMgr.UltimateScore:F0}";
        }

        if (ultimateTimeLabel != null)
        {
            ultimateTimeLabel.gameObject.SetActive(isUltimate);
            if (isUltimate)
            {
                int mins = (int)(_modeMgr.UltimateTime / 60f);
                int secs = (int)(_modeMgr.UltimateTime % 60f);
                ultimateTimeLabel.text = $"{mins:00}:{secs:00}";
            }
        }

        // Combo
        if (comboLabel != null)
        {
            int combo = _score.Combo;
            comboLabel.text = combo >= 2 ? $"x{combo}" : "";

            float scale = 1f + Mathf.Min(combo / 100f, 0.6f);
            comboLabel.rectTransform.localScale = Vector3.one * scale;

            if (combo > _lastCombo && combo >= 2)
            {
                _shakeCurrent = shakeIntensity * Mathf.Min(1f, combo / 20f);
                CheckMilestone(combo);
            }

            if (_shakeCurrent > 0f)
            {
                _shakeCurrent -= Time.deltaTime * shakeFalloff;
                float ox = (Random.value - 0.5f) * 2f * _shakeCurrent;
                float oy = (Random.value - 0.5f) * 2f * _shakeCurrent;
                comboLabel.rectTransform.anchoredPosition =
                    _comboBasePos + new Vector3(ox, oy, 0);
            }
            else
            {
                comboLabel.rectTransform.anchoredPosition = _comboBasePos;
            }

            _lastCombo = combo;
        }
    }

    void CheckMilestone(int combo)
    {
        if (_crt == null) return;
        if      (combo == 50) _crt.PulseVignette(color50, 0.6f);
        else if (combo == 25) _crt.PulseVignette(color25, 0.4f);
        else if (combo == 10) _crt.PulseVignette(color10, 0.3f);
        else if (combo % 10 == 0 && combo > 50)
            _crt.PulseVignette(Color.white, 0.5f);
    }
}

// using UnityEngine;
// using TMPro;

// /// Attach to a GameObject called "GameUI" in the scene.
// /// Needs 4 TMP text objects and access to the spawner for lane positions.
// public class GameUI : MonoBehaviour
// {
//     [Header("Text References")]
//     public TextMeshProUGUI accuracyLabel;   // e.g.  "100.0%"
//     public TextMeshProUGUI comboLabel;      // e.g.  "x24"
//     public TextMeshProUGUI speedLabel;      // e.g.  "SPEED x1.4"

//     [Header("Combo Shake")]
//     public float shakeIntensity = 3f;
//     public float shakeFalloff   = 8f;

//     [Header("Milestone Colors")]  // vignette pulse colors at combo milestones
//     public Color color10  = new Color(0.2f, 1f,   0.2f);
//     public Color color25  = new Color(1f,   0.8f, 0.1f);
//     public Color color50  = new Color(1f,   0.2f, 2f  );

//     private ScoreManager  _score;
//     private NoteSpawner   _spawner;
//     private CRTPassDriver _crt;

//     private int   _lastCombo   = 0;
//     private float _shakeCurrent= 0f;
//     private Vector3 _comboBasePos;

//     void Start()
//     {
//         _score   = FindFirstObjectByType<ScoreManager>();
//         _spawner = FindFirstObjectByType<NoteSpawner>();
//         _crt     = FindFirstObjectByType<CRTPassDriver>();

//         if (comboLabel != null) _comboBasePos = comboLabel.rectTransform.anchoredPosition;
//     }

//     void Update()
//     {
//         if (_score == null) return;

//         // Accuracy label
//         if (accuracyLabel != null)
//             accuracyLabel.text = $"{_score.Accuracy:F1}%";

//         // Speed label
//         if (speedLabel != null && _spawner != null)
//         {
//             float mult = _spawner.startFallDuration / _spawner.CurrentFallDuration;
//             speedLabel.text = $"SPD x{mult:F1}";
//         }

//         // Combo label — only show at 2+
//         if (comboLabel != null)
//         {
//             int combo = _score.Combo;
//             comboLabel.text = combo >= 2 ? $"x{combo}" : "";

//             // Scale grows with combo
//             float scale = 1f + Mathf.Min(combo / 100f, 0.6f);
//             comboLabel.rectTransform.localScale = Vector3.one * scale;

//             // Shake on new hit
//             if (combo > _lastCombo && combo >= 2)
//             {
//                 _shakeCurrent = shakeIntensity * Mathf.Min(1f, combo / 20f);
//                 CheckMilestone(combo);
//             }

//             // Apply shake
//             if (_shakeCurrent > 0f)
//             {
//                 _shakeCurrent -= Time.deltaTime * shakeFalloff;
//                 float ox = (Random.value - 0.5f) * 2f * _shakeCurrent;
//                 float oy = (Random.value - 0.5f) * 2f * _shakeCurrent;
//                 comboLabel.rectTransform.anchoredPosition =
//                     _comboBasePos + new Vector3(ox, oy, 0);
//             }
//             else
//             {
//                 comboLabel.rectTransform.anchoredPosition = _comboBasePos;
//             }

//             _lastCombo = combo;
//         }
//     }

//     void CheckMilestone(int combo)
//     {
//         if (_crt == null) return;
//         if      (combo == 50) _crt.PulseVignette(color50, 0.6f);
//         else if (combo == 25) _crt.PulseVignette(color25, 0.4f);
//         else if (combo == 10) _crt.PulseVignette(color10, 0.3f);
//         else if (combo % 10 == 0 && combo > 50)
//             _crt.PulseVignette(Color.white, 0.5f);
//     }
// }