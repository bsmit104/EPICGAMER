using UnityEngine;
using System.Collections;

public class LaneIndicatorSetup : MonoBehaviour
{
    public GameObject notePrefab;
    public Camera gameCamera;
    public int gameLayerIndex = 8;

    void Start()
    {
        StartCoroutine(Setup());
    }

    IEnumerator Setup()
    {
        NoteSpawner spawner = null;
        while (spawner == null || spawner.LaneXPositions == null)
        {
            spawner = FindFirstObjectByType<NoteSpawner>();
            yield return null;
        }

        for (int i = 0; i < spawner.LaneXPositions.Length; i++)
        {
            float x = spawner.LaneXPositions[i];
            float y = spawner.HitZoneY;

            Vector3 lp = new Vector3(x, y, gameCamera.nearClipPlane + 5f);
            Vector3 wp = gameCamera.transform.TransformPoint(lp);

            GameObject obj = Instantiate(notePrefab, wp, gameCamera.transform.rotation);
            obj.layer = gameLayerIndex;

            // Dim the color so it doesn't look like a real note
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = spawner.GetLaneColor(i);
                sr.color = new Color(c.r * 0.2f, c.g * 0.2f, c.b * 0.2f, 0.5f);
            }

            // Make sure it never moves — disable NoteController if present
            var nc = obj.GetComponent<NoteController>();
            if (nc != null) nc.enabled = false;
        }
    }
}