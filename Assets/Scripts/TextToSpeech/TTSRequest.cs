[System.Serializable]
public class Voice
{
    public string mode;
    public string id;
}

[System.Serializable]
public class GenerationConfig
{
    public float volume;
    public float speed;
    public string emotion;
}

[System.Serializable]
public class OutputFormat
{
    public string container;
    public string encoding;
    public int sample_rate;
    public int bit_rate;
}

[System.Serializable]
public class TTSRequest
{
    public string model_id;
    public string transcript;
    public Voice voice;
    public string language;
    public GenerationConfig generation_config;
    public OutputFormat output_format;
    public bool save;
}
