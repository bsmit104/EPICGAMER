using UnityEngine;
using System.Collections;

public class HandAnimator : MonoBehaviour
{
    [Header("Hand Transforms")]
    public Transform handLeft;    // Lanes 0-3 (A S D F)
    public Transform handRight;   // Lanes 4-6 (J K L)

    [Header("Flop Settings")]
    public float flopDownY        = 0.05f;  // World units hand drops on press
    public float flopDownDuration = 0.05f;  // Seconds to slam down
    public float flopUpDuration   = 0.18f;  // Seconds to return up

    [Header("Idle Bob")]
    public float idleBobAmp  = 0.004f;
    public float idleBobFreq = 0.9f;

    private Vector3 _leftRest, _rightRest;
    private Coroutine _leftFlop, _rightFlop;

    void Start()
    {
        if (handLeft  != null) _leftRest  = handLeft.position;
        if (handRight != null) _rightRest = handRight.position;
    }

    void Update()
    {
        float bob = Mathf.Sin(Time.time * idleBobFreq * Mathf.PI * 2f) * idleBobAmp;

        if (handLeft  != null && _leftFlop  == null)
            handLeft.position  = _leftRest  + Vector3.up * bob;
        if (handRight != null && _rightFlop == null)
            handRight.position = _rightRest + Vector3.up * bob;
    }

    public void TriggerFlop(int lane)
    {
        bool isLeft = lane <= 3;

        if (isLeft && handLeft != null)
        {
            if (_leftFlop != null) StopCoroutine(_leftFlop);
            _leftFlop = StartCoroutine(Flop(handLeft, _leftRest, true));
        }
        else if (!isLeft && handRight != null)
        {
            if (_rightFlop != null) StopCoroutine(_rightFlop);
            _rightFlop = StartCoroutine(Flop(handRight, _rightRest, false));
        }
    }

    IEnumerator Flop(Transform hand, Vector3 restPos, bool isLeft)
    {
        Vector3 downPos = restPos - Vector3.up * flopDownY;

        // Slam down
        float t = 0f;
        while (t < flopDownDuration)
        {
            hand.position = Vector3.Lerp(restPos, downPos, t / flopDownDuration);
            t += Time.deltaTime;
            yield return null;
        }
        hand.position = downPos;

        // Spring back up
        t = 0f;
        while (t < flopUpDuration)
        {
            float spring = 1f - Mathf.Pow(1f - (t / flopUpDuration), 3f);
            hand.position = Vector3.Lerp(downPos, restPos, spring);
            t += Time.deltaTime;
            yield return null;
        }
        hand.position = restPos;

        if (isLeft) _leftFlop  = null;
        else        _rightFlop = null;
    }
}