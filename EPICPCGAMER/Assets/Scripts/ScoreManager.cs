using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int Score    { get; private set; } = 0;
    public int Combo    { get; private set; } = 0;
    public int Misses   { get; private set; } = 0;

    public void RegisterHit(HitRating rating)
    {
        Combo++;
        int pts = 0;
        if (rating == HitRating.Perfect) pts = 300;
        else if (rating == HitRating.Good) pts = 150;
        else if (rating == HitRating.Bad)  pts = 50;
        Score += pts * Mathf.Max(1, Combo / 5);
        Debug.Log($"HIT {rating} | Combo {Combo} | Score {Score}");
    }

    public void RegisterMiss()
    {
        Combo = 0;
        Misses++;
        Debug.Log($"MISS | Score {Score}");
    }
}

public enum HitRating { None, Bad, Good, Perfect }