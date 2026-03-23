using UnityEngine;
using System.Collections;

/// Attach to a GameObject called "HitParticles" in the scene.
/// Uses simple instantiated quads — no Particle System component needed.
public class HitParticles : MonoBehaviour
{
    [Header("Particle Prefab")]
    public GameObject particlePrefab;  // tiny quad, same setup as NotePrefab
    public Camera     gameCamera;
    public int        gameLayerIndex = 8;

    [Header("Burst Settings")]
    public int   perfectCount = 12;
    public int   goodCount    = 7;
    public int   badCount     = 3;
    public float speed        = 0.8f;
    public float lifetime     = 0.35f;

    public void SpawnBurst(int laneIndex, float hitZoneY, float laneX,
                           Color color, HitRating rating)
    {
        int count = rating == HitRating.Perfect ? perfectCount
                  : rating == HitRating.Good    ? goodCount
                  :                               badCount;

        for (int i = 0; i < count; i++)
            StartCoroutine(Particle(laneX, hitZoneY, color));
    }

    IEnumerator Particle(float laneX, float hitZoneY, Color color)
    {
        // Spawn at hit zone in GameCamera local space
        Vector3 lp = new Vector3(laneX, hitZoneY, gameCamera.nearClipPlane + 5f);
        Vector3 wp = gameCamera.transform.TransformPoint(lp);

        var obj = Instantiate(particlePrefab, wp, gameCamera.transform.rotation);
        obj.layer = gameLayerIndex;

        var sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = color;

        // Random outward direction
        float angle  = Random.Range(0f, Mathf.PI * 2f);
        Vector3 dir  = gameCamera.transform.TransformDirection(
                           new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f));

        float elapsed = 0f;
        Vector3 startPos = obj.transform.position;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / lifetime;

            // Move outward, slow down
            obj.transform.position = startPos + dir * speed * (1f - t) * elapsed;

            // Shrink and fade
            float scale = Mathf.Lerp(0.015f, 0f, t);
            obj.transform.localScale = Vector3.one * scale;
            if (sr != null) sr.color = new Color(color.r, color.g, color.b, 1f - t);

            yield return null;
        }

        Destroy(obj);
    }
}