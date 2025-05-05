using UnityEngine;
using System;
using System.IO;

// Captures microphone input from the default audio device in Unity.
// Continuously records audio into a looping AudioClip and provides audio data as 16-bit PCM byte arrays for real-time streaming.
public class MicrophoneRecorder : MonoBehaviour
{
    public int sampleRate = 16000;
    private AudioClip micClip;
    private string micDevice;
    private int lastSamplePosition = 0;

    // Initializes microphone recording
    void Start()
    {
        StartMicrophone();
    }

    // Starts the microphone using the default device and sets up a looping AudioClip buffer
    // If no microphones are found or recording fails, logs error messages
    void StartMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone detected!");
            return;
        }

        Debug.Log("🎤 Available microphones:");
        foreach (var device in Microphone.devices)
        {
            Debug.Log($" - {device}");
        }

        micDevice = Microphone.devices[0]; // Use default
        Debug.Log($"🎙️ Using microphone: {micDevice}");
        micClip = Microphone.Start(micDevice, true, 10, sampleRate); // 10 sec loop, 16kHz

        if (micClip == null)
        {
            Debug.LogError("❌ Failed to start microphone recording.");
            return;
        }

        Debug.Log($"🎧 micClip created with frequency: {micClip.frequency}, channels: {micClip.channels}, samples: {micClip.samples}");
        Debug.Log("🎤 Microphone started.");
    }

    // Returns new audio data captured from the microphone since the last call as a 16-bit PCM byte array
    // Handles looping by resetting sample index if necessary.
    public byte[] GetMicDataAsPCM()
    {
        if (micClip == null) return null;

        int currentPosition = Microphone.GetPosition(micDevice);

        if (currentPosition < lastSamplePosition)
        {
            // Loop wrapped
            lastSamplePosition = 0;
        }

        int sampleDiff = currentPosition - lastSamplePosition;

        if (sampleDiff <= 0)
            return null;

        float[] samples = new float[sampleDiff];
        micClip.GetData(samples, lastSamplePosition);
        lastSamplePosition = currentPosition;

        return ConvertToPCM16(samples);
    }

    // Converts a float array of audio samples (range -1.0f to 1.0f) to 16-bit PCM format
    // Each float is clamped and scaled to a signed short, then written in little endian byte order
    byte[] ConvertToPCM16(float[] samples)
    {
        byte[] pcm = new byte[samples.Length * 2];

        for (int i = 0; i < samples.Length; i++)
        {
            float clamped = Mathf.Clamp(samples[i], -1f, 1f);
            short val = (short)(clamped * short.MaxValue);
            pcm[i * 2] = (byte)(val & 0xff);         // Little endian
            pcm[i * 2 + 1] = (byte)((val >> 8) & 0xff);
        }

        if (samples.Length > 10)
        {
            string debug = "🎧 First 10 sample values: ";
            for (int i = 0; i < 10; i++)
            {
                float clamped = Mathf.Clamp(samples[i], -1f, 1f);
                short val = (short)(clamped * short.MaxValue);
                debug += val + " ";
            }
            //Debug.Log(debug);
        }

        return pcm;
    }
}