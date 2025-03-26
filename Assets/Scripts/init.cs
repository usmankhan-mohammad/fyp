using UnityEngine;
using Google.Cloud.Speech.V1;
using Grpc.Auth;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using Google.Protobuf;

public class GoogleStreamingSpeech : MonoBehaviour
{
    private SpeechClient.StreamingRecognizeStream stream;
    private AudioClip micClip;
    private const int sampleRate = 16000;
    private int chunkSize = 1024;

    async void Start()
    {
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", Application.streamingAssetsPath + "/key.json");

        var speech = await SpeechClient.CreateAsync();
        stream = speech.StreamingRecognize();

        await stream.WriteAsync(new StreamingRecognizeRequest
        {
            StreamingConfig = new StreamingRecognitionConfig
            {
                Config = new RecognitionConfig
                {
                    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    SampleRateHertz = sampleRate,
                    LanguageCode = "en-GB"
                },
                InterimResults = true
            }
        });

        micClip = Microphone.Start(null, true, 10, sampleRate);
        StartCoroutine(StreamMicrophone());
        _ = ReadResponses();
    }

    IEnumerator StreamMicrophone()
    {
        int lastPos = 0;
        float[] sampleBuffer = new float[chunkSize];

        while (true)
        {
            int curPos = Microphone.GetPosition(null);
            int samplesAvailable = (curPos + micClip.samples - lastPos) % micClip.samples;

            if (samplesAvailable >= chunkSize)
            {
                micClip.GetData(sampleBuffer, lastPos % micClip.samples);
                byte[] pcmBytes = ConvertToPCM(sampleBuffer);

                await stream.WriteAsync(new StreamingRecognizeRequest
                {
                    AudioContent = ByteString.CopyFrom(pcmBytes)
                });

                lastPos = (lastPos + chunkSize) % micClip.samples;
            }

            yield return null;
        }
    }

    async Task ReadResponses()
    {
        while (await stream.ResponseStream.MoveNext(default))
        {
            var response = stream.ResponseStream.Current;
            foreach (var result in response.Results)
            {
                foreach (var alt in result.Alternatives)
                {
                    Debug.Log($"🎙️ Transcribed: {alt.Transcript}");
                    // You can call your SignLanguageRenderer.Translate(alt.Transcript) here
                }
            }
        }
    }

    byte[] ConvertToPCM(float[] samples)
    {
        byte[] bytes = new byte[samples.Length * 2];
        for (int i = 0; i < samples.Length; ++i)
        {
            short val = (short)(samples[i] * short.MaxValue);
            bytes[i * 2] = (byte)(val & 0xFF);
            bytes[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
        }
        return bytes;
    }
}