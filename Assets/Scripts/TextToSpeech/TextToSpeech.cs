using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeech : MonoBehaviour
{
    #region Variables
    private string beApiUrl = "https://www.be-vrealty.xyz/api/TtsAudio/";
    private string idBackEnd;
    
    [Header("Property Settings")]
    public ScenePropertyId scenePropertyId;
    private string propId;
    
    private string key;
    private string token;
    private bool hasPlayed = false;
    private static int concurrentRequests = 0;
    private static int maxConcurrentRequests = 2;

    private string transcript;
    [Header("Voice")]
    public Voices.VoiceOption voiceOption;
    public Voices.Emotion emotion;

    [Header("Text")]
    private AudioSource audioSource;
    #endregion

    // Global queue for playback requests across all instances
    private class PlayRequest { public string filePath; public Transform origin; }
    private static Queue<PlayRequest> playQueue = new Queue<PlayRequest>();
    private static bool queueProcessorRunning = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        // Get propertyId from ScenePropertyId component if assigned
        if (scenePropertyId != null && !string.IsNullOrEmpty(scenePropertyId.propertyId))
        {
            propId = scenePropertyId.propertyId;
        }
        
        // Fetch property TTS audios and match with object name to set idBackEnd
        if (!string.IsNullOrEmpty(propId))
        {
            StartCoroutine(FetchTranscriptByPropertyAndName(propId));
        }
        else
        {
            StartCoroutine(GetSpeechFromS3(transcript, Voices.VoiceId[voiceOption], emotion));
        }
    }
    // kích hoạt khi player chạm vào vùng trigger, khi kích hoạt, ktra xem file đã có chưa, nếu có thì play luôn, nếu chưa thì tạo mới
    private void OnTriggerEnter(Collider other)
    {
        // Check if already played OR not a player
        if (hasPlayed || !other.CompareTag("Player")) return;
        
        // Mark as played immediately to prevent multiple triggers
        hasPlayed = true;
        
        string localPath = GetLocalFilePath();
        if (File.Exists(localPath))
        {
            EnqueuePlayback(localPath, this.transform);
        }
        else
        {
            StartCoroutine(GetSpeechFromS3(transcript, Voices.VoiceId[voiceOption], emotion));
        }
    }

    // Helper method to fallback to local transcript
    // COMMENTED OUT - Can be reverted if needed
    // void FallbackToLocalTranscript(string reason)
    // {
    //     if (!string.IsNullOrEmpty(reason))
    //         Debug.LogWarning(reason);
    //     StartCoroutine(GetSpeechFromS3(transcript, Voices.VoiceId[voiceOption], emotion));
    // }

    // Fetch transcript by property ID and object name
    IEnumerator FetchTranscriptByPropertyAndName(string propId)
    {
        string url = $"{beApiUrl}property/{propId}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Accept", "application/json");
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                // COMMENTED OUT FALLBACK - Can be reverted if needed
                // FallbackToLocalTranscript($"Failed to fetch property TTS audios: {request.error}");
                Debug.LogError($"Failed to fetch property TTS audios: {request.error}");
                yield break;
            }

            string jsonResponse = request.downloadHandler.text;
            var wrapperResponse = JsonUtility.FromJson<TTSResponse>(jsonResponse);
            
            if (wrapperResponse == null || !wrapperResponse.isSuccess)
            {
                // COMMENTED OUT FALLBACK - Can be reverted if needed
                // FallbackToLocalTranscript("Failed to fetch property TTS audios: " + (wrapperResponse != null ? wrapperResponse.message : "Invalid response"));
                Debug.LogError("Failed to fetch property TTS audios: " + (wrapperResponse != null ? wrapperResponse.message : "Invalid response"));
                yield break;
            }

            // Extract the result array
            int resultStart = jsonResponse.IndexOf("\"result\":[");
            if (resultStart < 0)
            {
                // COMMENTED OUT FALLBACK - Can be reverted if needed
                // FallbackToLocalTranscript("No result field found in response");
                Debug.LogError("No result field found in response");
                yield break;
            }

            resultStart = jsonResponse.IndexOf("[", resultStart);
            int resultEnd = jsonResponse.LastIndexOf("]");
            
            if (resultStart < 0 || resultEnd <= resultStart)
            {
                // COMMENTED OUT FALLBACK - Can be reverted if needed
                // FallbackToLocalTranscript("Failed to extract result array from response");
                Debug.LogError("Failed to extract result array from response");
                yield break;
            }

            string arrayJson = jsonResponse.Substring(resultStart + 1, resultEnd - resultStart - 1);
            string objectName = gameObject.name.Trim();
            
            // Simple split by "},{" to get individual objects
            string[] items = arrayJson.Split(new string[] { "},{" }, StringSplitOptions.None);
            
            foreach (string item in items)
            {
                string jsonItem = item.StartsWith("{") ? item : "{" + item;
                jsonItem = jsonItem.EndsWith("}") ? jsonItem : jsonItem + "}";
                
                TtsAudioDbResponse audioData = JsonUtility.FromJson<TtsAudioDbResponse>(jsonItem);
                
                if (audioData != null && !string.IsNullOrEmpty(audioData.name) &&
                    audioData.name.Trim().Equals(objectName, StringComparison.OrdinalIgnoreCase))
                {
                    // Found match, use data directly
                    idBackEnd = audioData.id;
                    transcript = audioData.transcript;
                    
                    // Parse voice option and emotion from response
                    if (System.Enum.TryParse(audioData.voiceId, true, out Voices.VoiceOption parsedVoice))
                        voiceOption = parsedVoice;

                    if (System.Enum.TryParse(audioData.emotion, true, out Voices.Emotion parsedEmotion))
                        emotion = parsedEmotion;
                    
                    Debug.Log($"Matched object '{objectName}' - Transcript: {transcript}");
                    
                    // Start TTS workflow with fetched data
                    StartCoroutine(GetSpeechFromS3(transcript, Voices.VoiceId[voiceOption], emotion));
                    yield break;
                }
            }
            
            // No match found
            // COMMENTED OUT FALLBACK - Can be reverted if needed
            // FallbackToLocalTranscript($"No TTS Audio match found for object name: {objectName}. Using local transcript.");
            Debug.LogError($"No TTS Audio match found for object name: {objectName}");
        }
    }

    // enqueue a playback and ensure single queue processor runs
    void EnqueuePlayback(string filePath, Transform origin)
    {
        Debug.Log("Enqueuing TTS playback: " + filePath);
        playQueue.Enqueue(new PlayRequest { filePath = filePath, origin = origin });
        if (!queueProcessorRunning)
        {
            queueProcessorRunning = true;
            StartCoroutine(ProcessPlaybackQueue());
        }
    }

    IEnumerator ProcessPlaybackQueue()
    {
        // process until queue empty
        while (playQueue.Count > 0)
        {
            PlayRequest req = playQueue.Dequeue();
            // PlayLocalAudioClip now yields until the clip playback finishes
            yield return StartCoroutine(PlayLocalAudioClip(req.filePath, req.origin));
            // wait 1s after the ongoing audio has ended before next
            yield return new WaitForSeconds(0.5f);
        }

        queueProcessorRunning = false;
    }

    //nhận 1 global slot, chỉ 2 slot đồng thời
    IEnumerator AcquireRequestSlot()
    {
        while (true)
        {
            int prev = Interlocked.Increment(ref concurrentRequests);
            if (prev <= maxConcurrentRequests)
                yield break; // slot acquired

            Interlocked.Decrement(ref concurrentRequests); // rollback
            yield return null; // wait a frame then retry
        }
    }

    void ReleaseRequestSlot()
    {
        Interlocked.Decrement(ref concurrentRequests);
        if (concurrentRequests < 0) concurrentRequests = 0;
    }

    // tạo speech mới thông qua api và upload lên be-vrealty
    IEnumerator GenerateSpeech(string text, string voiceId, Voices.Emotion emotion)
    {
        var req = new TTSRequest
        {
            model_id = "sonic-3-2025-10-27",
            transcript = text,
            voice = new Voice { mode = "id", id = voiceId },
            language = "vi",
            generation_config = new GenerationConfig { volume = 2, speed = 1, emotion = emotion.ToString().ToLower() },
            output_format = new OutputFormat { container = "wav", encoding = "pcm_s16le", sample_rate = 44100, bit_rate = 128000 },
            save = true
        };
        string json = JsonUtility.ToJson(req);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        string beRequestUrl = beApiUrl + "tts-bytes";
        // wait for an available global slot
        yield return StartCoroutine(AcquireRequestSlot());
        try
        {
            using (UnityWebRequest www = new UnityWebRequest(beRequestUrl, "POST"))
            {
                www.uploadHandler = new UploadHandlerRaw(jsonBytes);
                www.downloadHandler = new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Accept", "audio/wav");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    byte[] audioData = www.downloadHandler.data;
                    // Create a unique filename using voice ID and initials of text (max 30 chars)
                    string fileName = GetKey(text, emotion);
                    // Upload to be-vrealty
                    WWWForm form = new WWWForm();
                    form.AddBinaryData("file", audioData, fileName, "audio/wav");
                    // Debug.Log("File name: " + fileName);
                    string bePostUrl = beApiUrl + "upload-audio/";
                    using (UnityWebRequest uploadRequest = UnityWebRequest.Post(bePostUrl, form))
                    {
                        yield return uploadRequest.SendWebRequest();
                        if (uploadRequest.result == UnityWebRequest.Result.Success)
                        {
                            Debug.Log("Audio uploaded successfully to be-vrealty");
                            // Parse the response to get the key
                            TTSResponse response = JsonUtility.FromJson<TTSResponse>(uploadRequest.downloadHandler.text);

                            if (response != null && response.isSuccess && response.result != null)
                            {
                                string resultJson = JsonUtility.ToJson(response.result);
                                UploadResult uploadResult = JsonUtility.FromJson<UploadResult>(resultJson);

                                if (uploadResult != null && !string.IsNullOrEmpty(uploadResult.key))
                                {
                                    key = uploadResult.key;
                                    StartCoroutine(GetSpeechFromS3(text, voiceId, emotion));
                                }
                                else
                                {
                                    Debug.LogError("Invalid upload result or missing key");
                                }
                            }
                            else
                            {
                                Debug.LogError("Upload failed: " + (response != null ? response.message : "Invalid response"));
                            }
                        }
                        else
                        {
                            Debug.LogError("Upload to be-vrealty failed: " + uploadRequest.error);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Cartesia TTS request failed: " + www.error);
                }
            }
        }
        finally
        {
            ReleaseRequestSlot();
        }
    }

    // tải audio từ be-vrealty s3 về nếu có
    IEnumerator GetSpeechFromS3(string text, string voiceId, Voices.Emotion emotion)
    {
        string key = GetKey(text, emotion);
        string clipUrl = beApiUrl + "clip?sourceOrKey=" + UnityWebRequest.EscapeURL(key);

        using (UnityWebRequest downloadClip = UnityWebRequest.Get(clipUrl))
        {
            downloadClip.downloadHandler = new DownloadHandlerBuffer();
            yield return downloadClip.SendWebRequest();

            if (downloadClip.result == UnityWebRequest.Result.Success)
            {
                string filePath = GetLocalFilePath();
                
                // Create directory if not exists
                string directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                try
                {
                    File.WriteAllBytes(filePath, downloadClip.downloadHandler.data);
                    Debug.Log($"Saved TTS clip to: {filePath}");
                    
                    // If this obj collides with player while just created, play it
                    if (!hasPlayed)
                    {
                        var playerObj = GameObject.FindGameObjectWithTag("Player");
                        if (playerObj != null)
                        {
                            var thisCollider = GetComponent<Collider>();
                            var playerCollider = playerObj.GetComponent<Collider>();
                            if (thisCollider != null && playerCollider != null && 
                                thisCollider.bounds.Intersects(playerCollider.bounds))
                            {
                                hasPlayed = true; // Mark as played before enqueueing
                                EnqueuePlayback(filePath, this.transform);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to write audio file: " + e.Message);
                }
            }
            else
            {
                Debug.Log("Audio clip not found on S3, generating new clip...");
                StartCoroutine(GenerateSpeech(text, voiceId, emotion));
            }
        }
    }
    // IEnumerator PlayLocalAudioClip(string filePath, Transform origin)
    // {
    //     string fileUri = new System.Uri(filePath).AbsoluteUri;
    //     using (var audioRequest = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.MPEG))
    //     {
    //         yield return audioRequest.SendWebRequest();

    //         if (audioRequest.result == UnityWebRequest.Result.Success)
    //         {
    //             AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
    //             SFXManager.instance.PlaySFXClip(audioClip, origin, 2.0f);
    //             Debug.Log("Playing TTS audio from local file: " + filePath);
    //             // yield until clip finished
    //             if (audioClip != null && audioClip.length > 0f)
    //                 yield return new WaitForSeconds(audioClip.length);
    //             else
    //                 yield return null;
    //         }
    //         else
    //         {
    //             Debug.LogError("Failed to load local audio file: " + audioRequest.error);
    //         }
    //     }
    // }
    IEnumerator PlayLocalAudioClip(string filePath, Transform origin)
    {
#if UNITY_WEBGL
        // 1. Read bytes from IDBFS file
        byte[] audioBytes = File.ReadAllBytes(filePath);
        if (audioBytes == null || audioBytes.Length == 0)
        {
            Debug.LogError("Failed to read local audio bytes: " + filePath);
            yield break;
        }

        // 2. Convert bytes → AudioClip
        AudioClip audioClip = WavUtility.ToAudioClip(audioBytes, 0, Path.GetFileNameWithoutExtension(filePath));
        if (audioClip == null)
        {
            Debug.LogError("Failed to parse AudioClip from bytes");
            yield break;
        }

        // 3. Play SFX
        SFXManager.instance.PlaySFXClip(audioClip, origin, 2.0f);

        hasPlayed = true;

        // 4. Wait until finished
        yield return new WaitForSeconds(audioClip.length);
#else
    // --------- Non-WebGL (Windows, Mac, Editor) ---------
    string fileUri = new System.Uri(filePath).AbsoluteUri;
    using (var audioRequest = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.WAV))
    {
        yield return audioRequest.SendWebRequest();

        if (audioRequest.result == UnityWebRequest.Result.Success)
        {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
            SFXManager.instance.PlaySFXClip(audioClip, origin, 2.0f);

            hasPlayed = true;

            if (audioClip != null && audioClip.length > 0f)
                yield return new WaitForSeconds(audioClip.length);
        }
        else
        {
            Debug.LogError("Failed to load local audio file: " + audioRequest.error);
        }
    }
#endif
    }


    string GetKey(string text, Voices.Emotion emotion) => $"tts/{Voices.GetInitials(text)}_{Voices.NonUnicode(voiceOption.ToString().ToLower())}_{emotion.ToString().ToLower()}.wav";
    
    string GetLocalFilePath()
    {
        string fileName = $"{Voices.GetInitials(transcript)}_{Voices.NonUnicode(voiceOption.ToString().ToLower())}_{emotion.ToString().ToLower()}.wav";
        return Path.Combine(Application.persistentDataPath, fileName);
    }
}