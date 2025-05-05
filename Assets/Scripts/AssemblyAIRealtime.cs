using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.Net.NativeWebSocket;
using System.Text;
using System.Threading.Tasks;
using Oculus.Interaction.HandGrab;
using UnityEngine.InputSystem;
using System.Linq;

// Handles real-time speech-to-text transcription using AssemblyAI's WebSocket API.
// Captures microphone input, sends it for transcription, receives results,
// and passes them to a UI for display and BSL sign translation.
public class AssemblyAIRealtime : MonoBehaviour
{
    public MicrophoneRecorder micRecorder;
    public string apiKey = "f3c03309258a4d3e8eecc6707d46bdfb";
    public SignDisplayUI signDisplayUI;

    private WebSocket websocket;
    private bool isConnected = false;
    
    public TMPro.TextMeshProUGUI subtitleText;

    private string transcriptBuffer = "";
    private readonly object transcriptLock = new object();

    private bool isPaused = false;
    public TMPro.TextMeshProUGUI pauseButtonText;
    

    // Initiates WebSocket connection to AssemblyAI on application start.
    void Start()
    {
        _ = ConnectToAssemblyAI();
        
    }

    // Establishes WebSocket connection to AssemblyAI API using provided API key.
    // Configures handlers for opening, messaging, errors, and closing.
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
    
    // Parses incoming messages from AssemblyAI's WebSocket.
    // Extracts final transcriptions and updates the internal buffer.
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

    // Updates subtitle display if not paused. Clears buffer and triggers sign rendering.
    void Update()
    {
        // Debug log for mouse scroll value
        if (Mouse.current != null && Mouse.current.scroll.ReadValue().y != 0)
        {
            Debug.Log($"[MouseScroll] Scroll value: {Mouse.current.scroll.ReadValue().y}");
        }
        if (!isPaused)
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
    }

    // Class used for deserializing transcript messages from AssemblyAI
    [Serializable]
    private class TranscriptMessage
    {
        public string message_type;
        public string text;
    }

    // Continuously captures audio from the microphone and sends it to AssemblyAI as base64-encoded PCM chunks.
    // Pauses when transcription is paused.
    IEnumerator SendAudioChunks()
    {
        Debug.Log("SendAudioChunks coroutine started");

        while (true)
        {
            if (isPaused)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

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

    // Translates given transcript text to corresponding BSL signs
    // Prioritizes phrases, then words, then falls back to fingerspelling
    void DisplaySignsFor(string text)
    {
        List<(Sprite, string)> signsToShow = new List<(Sprite, string)>();
        Debug.Log($"[DisplaySignsFor] Input text: {text}");

        text = text.ToLower().Trim();
        text = System.Text.RegularExpressions.Regex.Replace(text, @"[^a-zA-Z0-9\s']", "");

        List<string> words = new List<string>(text.Split(' '));
        int i = 0;

        while (i < words.Count)
        {
            bool matched = false;

            for (int len = words.Count - i; len > 0; len--)
            {
                string segment = string.Join(" ", words.GetRange(i, len));
                if (SignManager.Instance.TryGetPhrase(segment, out var phraseSprite))
                {
                    Debug.Log($"[DisplaySignsFor] Found phrase: {segment}");
                    signsToShow.Add((phraseSprite, segment));
                    i += len;
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                string word = words[i].Trim().ToLower();
                Debug.Log($"[DisplaySignsFor] Processing word: {word}");

                if (SignManager.Instance.TryGetSign(word, out var wordSprite))
                {
                    Debug.Log($"[DisplaySignsFor] Found word: {word}");
                    signsToShow.Add((wordSprite, word));
                }
                else
                {
                    var fingerSpelling = SignManager.Instance.GetFingerspelling(word);
                    if (fingerSpelling.Count > 0)
                    {
                        Debug.Log($"[DisplaySignsFor] Using fingerspelling for: {word}");
                        signsToShow.AddRange(fingerSpelling.Select(sprite => (sprite, word)));
                    }
                    else
                    {
                        Debug.LogWarning($"[DisplaySignsFor] No sign or fingerspelling for: {word}");
                    }
                }

                i++;
            }
        }

        signDisplayUI.DisplaySigns(signsToShow);
    }

    // Toggles the pause state of speech recognition. When paused, audio is no longer streamed to AssemblyAI
    // Reconnection is attempted if unpaused and the WebSocket is not currently connected
    public void TogglePause()
    {
        isPaused = !isPaused;

        Debug.Log(isPaused ? "[TogglePause] Speech recognition paused." : "[TogglePause] Speech recognition resumed.");

        if (pauseButtonText != null)
        {
            pauseButtonText.text = isPaused ? "Unpause Speech Recognition" : "Pause Speech Recognition";
        }

        if (!isPaused)
        {
            // If unpausing and WebSocket is not connected, reconnect
            if (websocket == null || websocket.State != WebSocketState.Open)
            {
                Debug.Log("[TogglePause] Reconnecting WebSocket...");
                _ = ConnectToAssemblyAI(); // fire-and-forget
            }
        }
    }

    // Ensures the WebSocket connection is closed gracefully on application quit.
    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}
