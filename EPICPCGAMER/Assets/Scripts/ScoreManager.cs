using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int Combo { get; private set; } = 0;
    public int MaxCombo { get; private set; } = 0;
    public int TotalNotes { get; private set; } = 0;
    public int HitNotes { get; private set; } = 0;
    public float Accuracy { get; private set; } = 100f;

    public void RegisterHit(HitRating rating)
    {
        Combo++;
        TotalNotes++;
        HitNotes++;
        if (Combo > MaxCombo) MaxCombo = Combo;
        RecalcAccuracy();
    }

    public void RegisterMiss()
    {
        Combo = 0;
        TotalNotes++;
        RecalcAccuracy();
    }

    void RecalcAccuracy()
    {
        Accuracy = TotalNotes == 0 ? 100f : (HitNotes / (float)TotalNotes) * 100f;
    }

    public void Reset()
    {
        Combo = 0;
        MaxCombo = 0;
        TotalNotes = 0;
        HitNotes = 0;
        Accuracy = 100f;
    }
}

public enum HitRating { None, Bad, Good, Perfect }

// using UnityEngine;

// public class ScoreManager : MonoBehaviour
// {
//     public int Score    { get; private set; } = 0;
//     public int Combo    { get; private set; } = 0;
//     public int Misses   { get; private set; } = 0;

//     public void RegisterHit(HitRating rating)
//     {
//         Combo++;
//         int pts = 0;
//         if (rating == HitRating.Perfect) pts = 300;
//         else if (rating == HitRating.Good) pts = 150;
//         else if (rating == HitRating.Bad)  pts = 50;
//         Score += pts * Mathf.Max(1, Combo / 5);
//         Debug.Log($"HIT {rating} | Combo {Combo} | Score {Score}");
//     }

//     public void RegisterMiss()
//     {
//         Combo = 0;
//         Misses++;
//         Debug.Log($"MISS | Score {Score}");
//     }
// }

// public enum HitRating { None, Bad, Good, Perfect }