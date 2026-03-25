using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ChartNote
{
    public int   lane;
    public float beat;
    public float hold = 0f;
}

[System.Serializable]
public class ChartFile
{
    public string          levelName         = "Level";
    public float           bpm               = 120f;
    public float           oneStarAccuracy   = 60f;
    public float           twoStarAccuracy   = 80f;
    public float           threeStarAccuracy = 95f;
    public float           startFallDuration = 2.5f;
    public float           minFallDuration   = 0.8f;
    public float           rampRate          = 0.04f;
    public List<ChartNote> notes             = new List<ChartNote>();
}

[CreateAssetMenu(fileName = "LevelData", menuName = "KeyBanger/Level Data")]
public class LevelData : ScriptableObject
{
    [Tooltip("Drag a .json chart file here")]
    public TextAsset chartFile;

    [HideInInspector] public string         levelName         = "Level";
    [HideInInspector] public float          bpm               = 120f;
    [HideInInspector] public float          oneStarAccuracy   = 60f;
    [HideInInspector] public float          twoStarAccuracy   = 80f;
    [HideInInspector] public float          threeStarAccuracy = 95f;
    [HideInInspector] public float          startFallDuration = 2.5f;
    [HideInInspector] public float          minFallDuration   = 0.8f;
    [HideInInspector] public float          rampRate          = 0.04f;
    [HideInInspector] public List<NoteData> notes             = new List<NoteData>();

    public void Parse()
    {
        if (chartFile == null) { Debug.LogError($"No chart file assigned to {name}"); return; }

        ChartFile chart = new ChartFile();
        JsonUtility.FromJsonOverwrite(chartFile.text, chart);

        levelName         = chart.levelName;
        bpm               = chart.bpm;
        oneStarAccuracy   = chart.oneStarAccuracy;
        twoStarAccuracy   = chart.twoStarAccuracy;
        threeStarAccuracy = chart.threeStarAccuracy;
        startFallDuration = chart.startFallDuration;
        minFallDuration   = chart.minFallDuration;
        rampRate          = chart.rampRate;

        float secondsPerBeat = 60f / bpm;
        notes.Clear();

        foreach (var cn in chart.notes)
        {
            notes.Add(new NoteData
            {
                laneIndex    = cn.lane,
                hitTime      = cn.beat * secondsPerBeat,
                type         = cn.hold > 0f ? NoteType.Hold : NoteType.Tap,
                holdDuration = cn.hold * secondsPerBeat
            });
        }
    }
}