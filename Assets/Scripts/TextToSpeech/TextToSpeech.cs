using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TextToSpeech : MonoBehaviour
{
    #region Variables
    private string beApiUrl = "https://www.be-vrealty.xyz/api/TtsAudio/";

    private string key;
    private string token;

    [TextArea]
    public string transcript;
    [Header("Voice")]
    public Voices.VoiceOption voiceOption;
    public Voices.Emotion emotion;

    [Header("Text")]
    private AudioSource audioSource;
    #endregion
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // key = $"{Voices.GetInitials(transcript)}_{Voices.NonUnicode(voiceOption.ToString().ToLower())}_{emotion.ToString().ToLower()}.mp3";
        // if (File.Exists(Path.Combine(Application.persistentDataPath, key)))
        // {
        //     Debug.Log("Audio file already exists locally. Loading from file.");
        //     Debug.Log("File path: " + Path.Combine(Application.persistentDataPath, key));
        //     StartCoroutine(PlayLocalAudioClip(Path.Combine(Application.persistentDataPath, key)));
        // }
        // else
        //     StartCoroutine(GetSpeechFromS3(transcript, Voices.VoiceId[voiceOption], emotion));
    }
    private void OnTriggerEnter(Collider other) 
    {
       
        Debug.Log("Collision detected with: " + other.gameObject.name);
        if (other.gameObject.CompareTag("Player"))
        {
            key = $"{Voices.GetInitials(transcript)}_{Voices.NonUnicode(voiceOption.ToString().ToLower())}_{emotion.ToString().ToLower()}.mp3";
            if (File.Exists(Path.Combine(Application.persistentDataPath, key)))
            {
                Debug.Log("Audio file already exists locally. Loading from file.");
                Debug.Log("File path: " + Path.Combine(Application.persistentDataPath, key));
                StartCoroutine(PlayLocalAudioClip(Path.Combine(Application.persistentDataPath, key)));
            }
            else
                StartCoroutine(GetSpeechFromS3(transcript, Voices.VoiceId[voiceOption], emotion));
        }       
    }
    IEnumerator GenerateSpeech(string text, string voiceId, Voices.Emotion emotion)
    {
        var req = new TTSRequest
        {
            model_id = "sonic-3-2025-10-27",
            transcript = text,
            voice = new Voice { mode = "id", id = voiceId },
            language = "vi",
            generation_config = new GenerationConfig { volume = 1, speed = 1, emotion = emotion.ToString().ToLower() },
            output_format = new OutputFormat { container = "mp3", encoding = "", sample_rate = 44100, bit_rate = 128000 },
            save = true
        };
        string json = JsonUtility.ToJson(req);
        byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
        string beRequestUrl = beApiUrl + "tts-bytes";
        using (UnityWebRequest www = new UnityWebRequest(beRequestUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonBytes);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "audio/mpeg");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                byte[] audioData = www.downloadHandler.data;
                // Create a unique filename using voice ID and initials of text (max 30 chars)
                string fileName = GetKey(text, emotion);
                // Upload to be-vrealty
                WWWForm form = new WWWForm();
                form.AddBinaryData("file", audioData, fileName, "audio/mpeg");
                Debug.Log("File name: " + fileName);
                string bePostUrl = beApiUrl + "upload-audio/";
                using (UnityWebRequest uploadRequest = UnityWebRequest.Post(bePostUrl, form))
                {
                    yield return uploadRequest.SendWebRequest();
                    if (uploadRequest.result == UnityWebRequest.Result.Success)
                    {
                        Debug.Log("Audio uploaded successfully to be-vrealty");
                        // Parse the response to get the key (expects { "key": "..." })
                        TTSResponse response = JsonUtility.FromJson<TTSResponse>(uploadRequest.downloadHandler.text);
                        if (response == null || string.IsNullOrEmpty(response.result.key))
                        {
                            Debug.LogError("Invalid upload response or missing key: " + uploadRequest.downloadHandler.text);
                            yield break;
                        }
                        key = response.result.key;
                        StartCoroutine(GetSpeechFromS3(text, voiceId, emotion));
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
    // IEnumerator GenerateSpeech(string text, string voiceId)
    // {
    //     string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";
    //     TTSRequest ttsRequest = new TTSRequest
    //     {
    //         text = text,
    //         model_id = "eleven_flash_v2_5"
    //     };

    //     var jsonBody = JsonUtility.ToJson(ttsRequest, true);
    //     byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

    //     using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
    //     {
    //         www.uploadHandler = new UploadHandlerRaw(bodyRaw);
    //         www.downloadHandler = new DownloadHandlerBuffer();
    //         www.SetRequestHeader("xi-api-key", apiKey);
    //         www.SetRequestHeader("Content-Type", "application/json");
    //         www.SetRequestHeader("Accept", "audio/mpeg");

    //         yield return www.SendWebRequest();
    //         if (www.result == UnityWebRequest.Result.Success)
    //         {
    //             byte[] audioData = www.downloadHandler.data;
    //             // Create a unique filename using voice ID and initials of text (max 20 chars)
    //             string fileName = $"tts/{Voices.GetInitials(text)}_{Voices.VoiceId[voiceOption]}.mp3";
    //             // Upload to be-vrealty
    //             WWWForm form = new WWWForm();
    //             form.AddBinaryData("file", audioData, fileName, "audio/mpeg");
    //             Debug.Log("File name: " + fileName);
    //             using (UnityWebRequest uploadRequest = UnityWebRequest.Post(beApiUrl + "upload-audio/", form))
    //             {
    //                 yield return uploadRequest.SendWebRequest();
    //                 if (uploadRequest.result == UnityWebRequest.Result.Success)
    //                 {
    //                     Debug.Log("Audio uploaded successfully to be-vrealty");
    //                     // Parse the response to get the key (expects { "key": "..." })
    //                     TTSResponse response = JsonUtility.FromJson<TTSResponse>(uploadRequest.downloadHandler.text);
    //                     if (response == null || string.IsNullOrEmpty(response.result.key))
    //                     {
    //                         Debug.LogError("Invalid upload response or missing key: " + uploadRequest.downloadHandler.text);
    //                         yield break;
    //                     }
    //                     key = response.result.key;
    //                     StartCoroutine(GetSpeechFromS3(text, voiceId));
    //                 }
    //                 else
    //                 {
    //                     Debug.LogError("Upload to be-vrealty failed: " + uploadRequest.error);
    //                 }
    //             }
    //         }
    //         else
    //         {
    //             Debug.LogError("ElevenLabs TTS request failed: " + www.error);
    //         }
    //     }
    // }
    // string GetKey(string text, string voiceId) => "tts/" + Voices.GetInitials(text) + "_" + voiceId + ".mp3";


    IEnumerator GetSpeechFromS3(string text, string voiceId, Voices.Emotion emotion)
    {
        string key = GetKey(text, emotion);
        Debug.Log("Using generated key to download audio: " + key);
        string clipUrl = beApiUrl + "clip?sourceOrKey=" + UnityWebRequest.EscapeURL(key);

        using (UnityWebRequest downloadClip = UnityWebRequest.Get(clipUrl))
        {
            downloadClip.downloadHandler = new DownloadHandlerBuffer();
            yield return downloadClip.SendWebRequest();

            if (downloadClip.result == UnityWebRequest.Result.Success)
            {
                byte[] clipData = downloadClip.downloadHandler.data;
                // Save to platform default persistent path
                string fileName = $"{Voices.GetInitials(text)}_{Voices.NonUnicode(voiceOption.ToString().ToLower())}_{emotion.ToString().ToLower()}.mp3";
                string filePath = Path.Combine(Application.persistentDataPath, fileName);
                try
                {
                    File.WriteAllBytes(filePath, clipData);
                    Debug.Log($"Saved TTS clip to: {filePath}");
                }
                catch (Exception e)
                {
                    Debug.LogError("Failed to write audio file: " + e.Message);
                    yield break;
                }
                StartCoroutine(PlayLocalAudioClip(filePath));
            }
            else
            {
                Debug.LogWarning("Failed to download clip from be-vrealty: " + downloadClip.error);
                //thường là 404, nếu sai thì tạo mới
                StartCoroutine(GenerateSpeech(text, voiceId, emotion));
            }
        }
    }
    IEnumerator PlayLocalAudioClip(string filePath)
    {
        string fileUri = new System.Uri(filePath).AbsoluteUri;
        using (var audioRequest = UnityWebRequestMultimedia.GetAudioClip(fileUri, AudioType.MPEG))
        {
            yield return audioRequest.SendWebRequest();

            if (audioRequest.result == UnityWebRequest.Result.Success)
            {
                AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
                audioSource.clip = audioClip;
                audioSource.Play();
                Debug.Log("Playing TTS audio from local file.");
            }
            else
            {
                Debug.LogError("Failed to load local audio file: " + audioRequest.error);
            }
        }
    }

    string GetKey(string text, Voices.Emotion emotion) => $"tts/{Voices.GetInitials(text)}_{Voices.NonUnicode(voiceOption.ToString().ToLower())}_{emotion.ToString().ToLower()}.mp3";




}
