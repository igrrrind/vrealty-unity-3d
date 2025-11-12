[System.Serializable]
public class TTSResponse
{
    public int statusCode;
    public string message;
    public bool isSuccess;
    public UploadResult result;
}

[System.Serializable]
public class UploadResult
{
    public string viewUrl;
    public string key;
}