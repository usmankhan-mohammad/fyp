using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.Net.NativeWebSocket;
using System.Text;
using System.Threading.Tasks;

public class AssemblyAIRealtime : MonoBehaviour
{
    public MicrophoneRecorder micRecorder;
    public string apiKey = "f3c03309258a4d3e8eecc6707d46bdfb";
    public SignDisplayUI signDisplayUI;

    private WebSocket websocket;
    private bool isConnected = false;
    
    // private string fullTranscript = "";
    public TMPro.TextMeshProUGUI subtitleText;

    private string transcriptBuffer = "";
    private readonly object transcriptLock = new object();
    

    void Start()
    {
        _ = ConnectToAssemblyAI();
    }

    private async Task ConnectToAssemblyAI()
    {
        string url = "wss://api.assemblyai.com/v2/realtime/ws?sample_rate=16000";

        var headers = new Dictionary<string, string>
        {
            { "Authorization", apiKey }
        };
        websocket = new WebSocket(url, headers);
        websocket.OnOpen += () => {
            Debug.Log("WebSocket connection opened");
            isConnected = true;
            Debug.Log("Connected — starting audio stream");
            StartCoroutine(SendAudioChunks());
        };

        websocket.OnError += (e) => Debug.LogError("WebSocket Error: " + e);

        websocket.OnClose += (e) => Debug.Log("WebSocket Closed");

        websocket.OnMessage += OnWebSocketMessage;

        await websocket.Connect();
    }
    
    private void OnWebSocketMessage(byte[] bytes, int offset, int length)
    {
        Task.Run(() =>
        {
            string message = Encoding.UTF8.GetString(bytes, offset, length);
            //Debug.Log("Full JSON: " + message);

            try
            {
                var json = JsonUtility.FromJson<TranscriptMessage>(message);
                if (json != null && !string.IsNullOrEmpty(json.text) &&
                    json.message_type == "FinalTranscript")
                {
                    Debug.Log("Transcript: " + json.text);
                    lock (transcriptLock)
                    {
                        if (!string.IsNullOrEmpty(json.text))
                        {
                            // Just show the full final message as it was transcribed
                            transcriptBuffer = json.text;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to parse message: " + e.Message);
            }
        });
    }

    void Update()
    {
        lock (transcriptLock)
        {
            if (!string.IsNullOrEmpty(transcriptBuffer))
            {
                subtitleText.text = transcriptBuffer;
                transcriptBuffer = "";
                if (signDisplayUI != null)
                {
                    DisplaySignsFor(subtitleText.text);
                }
            }
        }
    }

    [Serializable]
    private class TranscriptMessage
    {
        public string message_type;
        public string text;
    }

    IEnumerator SendAudioChunks()
    {
        Debug.Log("SendAudioChunks coroutine started");

        while (true)
        {
            byte[] chunk = micRecorder.GetMicDataAsPCM();
            if (chunk != null && chunk.Length > 0)
            {
                short sample0 = BitConverter.ToInt16(chunk, 0);
                short sample1 = BitConverter.ToInt16(chunk, 2);
                short sample2 = BitConverter.ToInt16(chunk, 4);
                //Debug.Log($"🎤 Sending audio chunk: {chunk.Length} bytes | First 3 samples: {sample0}, {sample1}, {sample2}");

                string encodedAudio = Convert.ToBase64String(chunk);
                string json = "{\"audio_data\":\"" + encodedAudio + "\"}";
                websocket.SendText(json);
            }
            else
            {
                Debug.Log("No audio chunk to send");
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    void DisplaySignsFor(string text)
    {
        List<Sprite> signsToShow = new List<Sprite>();

        Debug.Log($"[DisplaySignsFor] Input text: {text}");

        string[] words = text.Split(' ');
        foreach (string word in words)
        {
            string cleanWord = word.ToLower().TrimEnd('.', ',', '!', '?');
            Debug.Log($"[DisplaySignsFor] Processing word: {cleanWord}");

            if (SignManager.Instance.TryGetSign(cleanWord, out var sign))
            {
                Debug.Log($"[DisplaySignsFor] Found sign for: {cleanWord}");
                signsToShow.Add(sign);
            }
            else
            {
                var fingerSpelling = SignManager.Instance.GetFingerspelling(cleanWord);
                if (fingerSpelling.Count > 0)
                {
                    Debug.Log($"[DisplaySignsFor] Using fingerspelling for: {cleanWord}");
                    signsToShow.AddRange(fingerSpelling);
                }
                else
                {
                    Debug.LogWarning($"[DisplaySignsFor] No sign or fingerspelling for: {cleanWord}");
                }
            }
        }

        signDisplayUI.DisplaySigns(signsToShow);
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}
