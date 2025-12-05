[System.Serializable]
public class TTSResponse
{
    public int statusCode;
    public string message;
    public bool isSuccess;
    public object? result;
}

[System.Serializable]
public class UploadResult
{
    public string viewUrl;
    public string key;
}
[System.Serializable]
public class TtsAudioDbResponse
{
    public string id;
    public string propertyId;
    public string name;
    public string transcript;
    public string language;
    public string voiceId;
    public string emotion;
    public string createdAt;
}
