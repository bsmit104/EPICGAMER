using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public ScoreManager ScoreManager { get; private set; }

    public float SongTime { get; private set; } = 0f;

    private bool _started = false;
    public float startDelay = 3f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ScoreManager = GetComponent<ScoreManager>();
        if (ScoreManager == null) ScoreManager = gameObject.AddComponent<ScoreManager>();
    }

    void Start()
    {
        StartCoroutine(StartGame());
    }

    IEnumerator StartGame()
    {
        yield return new WaitForSeconds(startDelay);
        _started = true;
    }

    // void Update()
    // {
    //     if (!_started) return;
    //     SongTime += Time.deltaTime;
    // }
    void Update()
    {
        if (!_started) return;
        SongTime += Time.deltaTime;

        // Press R to restart
        if (Input.GetKeyDown(KeyCode.R))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
}