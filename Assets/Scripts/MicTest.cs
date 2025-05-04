using System.Collections;
using UnityEngine;

public class MicTest : MonoBehaviour
{
    public MicrophoneRecorder micRecorder;

    void Start()
    {
        StartCoroutine(SampleMicData());
    }

    IEnumerator SampleMicData()
    {
        yield return new WaitForSeconds(1f); // Give mic some time to start

        while (true)
        {
            byte[] pcmData = micRecorder.GetMicDataAsPCM();

            if (pcmData != null && pcmData.Length > 0)
            {
                //Debug.Log($"🎧 PCM Chunk Length: {pcmData.Length} bytes");
                // You could send this to Google later
            }
            else
            {
                Debug.LogWarning("No mic data yet.");
            }

            yield return new WaitForSeconds(0.1f); // Wait 100ms before next chunk
        }
    }
}