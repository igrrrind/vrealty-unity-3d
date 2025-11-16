using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class WavUtility
{
    // Convert byte[] (WAV) → AudioClip
    public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples = 0, string name = "wav")
    {
        int channels = fileBytes[22];
        int sampleRate = BitConverter.ToInt32(fileBytes, 24);

        int pos = 12; // Skip header
        while (!(fileBytes[pos] == 100 && fileBytes[pos + 1] == 97 && fileBytes[pos + 2] == 116 && fileBytes[pos + 3] == 97))
        {
            pos += 4;
            int size = BitConverter.ToInt32(fileBytes, pos);
            pos += 4 + size;
        }

        pos += 8;
        int subchunk2 = fileBytes.Length - pos;
        int samples = subchunk2 / 2;

        float[] data = new float[samples];

        int i = 0;
        while (pos < fileBytes.Length)
        {
            short sample = BitConverter.ToInt16(fileBytes, pos);
            data[i] = sample / 32768f;
            pos += 2;
            i++;
        }

        AudioClip audioClip = AudioClip.Create(name, samples, channels, sampleRate, false);
        audioClip.SetData(data, offsetSamples);
        return audioClip;
    }

    // Convert AudioClip → WAV byte[] (optional)
    public static byte[] FromAudioClip(AudioClip audioClip)
    {
        var samples = new float[audioClip.samples];
        audioClip.GetData(samples, 0);

        MemoryStream stream = new MemoryStream();
        const int HEADER_SIZE = 44;

        stream.Seek(HEADER_SIZE, SeekOrigin.Begin);

        foreach (float sample in samples)
        {
            short s = (short)(sample * 32767);
            stream.Write(BitConverter.GetBytes(s), 0, 2);
        }

        byte[] data = stream.ToArray();

        stream.Seek(0, SeekOrigin.Begin);
        stream.Write(Encoding.ASCII.GetBytes("RIFF"), 0, 4);
        stream.Write(BitConverter.GetBytes(data.Length - 8), 0, 4);
        stream.Write(Encoding.ASCII.GetBytes("WAVE"), 0, 4);
        stream.Write(Encoding.ASCII.GetBytes("fmt "), 0, 4);
        stream.Write(BitConverter.GetBytes(16), 0, 4);
        stream.Write(BitConverter.GetBytes((short)1), 0, 2);
        stream.Write(BitConverter.GetBytes((short)audioClip.channels), 0, 2);
        stream.Write(BitConverter.GetBytes(audioClip.frequency), 0, 4);
        stream.Write(BitConverter.GetBytes(audioClip.frequency * audioClip.channels * 2), 0, 4);
        stream.Write(BitConverter.GetBytes((short)(audioClip.channels * 2)), 0, 2);
        stream.Write(BitConverter.GetBytes((short)16), 0, 2);
        stream.Write(Encoding.ASCII.GetBytes("data"), 0, 4);
        stream.Write(BitConverter.GetBytes(samples.Length * 2), 0, 4);

        return stream.ToArray();
    }
}
