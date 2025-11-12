using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;


public static class Voices
{
    public enum VoiceOption
    {
        Nữ,
        Nam
    }
    public enum Emotion
    {
        neutral,
        happy,
        excited,
        enthusiastic,
        elated,
        euphoric,
        triumphant,
        amazed,
        surprised,
        flirtatious,
        curious,
        content,
        peaceful,
        serene,
        calm,
        grateful,
        affectionate,
        trust,
        sympathetic,
        anticipation,
        mysterious,
        angry,
        mad,
        outraged,
        frustrated,
        agitated,
        threatened,
        disgusted,
        contempt,
        envious,
        sarcastic,
        ironic,
        sad,
        dejected,
        melancholic,
        disappointed,
        hurt,
        guilty,
        bored,
        tired,
        rejected,
        nostalgic,
        wistful,
        apologetic,
        hesitant,
        insecure,
        confused,
        resigned,
        anxious,
        panicked,
        alarmed,
        scared,
        proud,
        confident,
        distant,
        skeptical,
        contemplative,
        determined

    }
    public static readonly Dictionary<VoiceOption, string> VoiceId = new Dictionary<VoiceOption, string>
    {
        { VoiceOption.Nữ, "b8cd71e3-bc14-4538-a530-d6314731c036" },
        { VoiceOption.Nam, "0e58d60a-2f1a-4252-81bd-3db6af45fb41" },
    };



    /// <summary>
    /// Lấy chuỗi ký tự đầu tiên của mỗi từ trong chuỗi đầu vào, tối đa 30 ký tự.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GetInitials(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string[] words = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        StringBuilder sb = new StringBuilder();
        int dialogLength = 30;
        if (words.Length < 30)
        {
            dialogLength = words.Length;
        }
        for (int i = 0; i < dialogLength; i++)
        {
            char firstChar = words[i][0];
            string normalized = NonUnicode(firstChar.ToString().ToLower());
            sb.Append(normalized);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Chuyển chuỗi có dấu thành không dấu và loại bỏ khoảng trắng, dấu phẩy, dấu chấm.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string NonUnicode(string str)
    {
        str = str.ToLower();
        str = Regex.Replace(str, "[àáạảãâầấậẩẫăằắặẳẵ]", "a");
        str = Regex.Replace(str, "[èéẹẻẽêềếệểễ]", "e");
        str = Regex.Replace(str, "[ìíịỉĩ]", "i");
        str = Regex.Replace(str, "[òóọỏõôồốộổỗơờớợởỡ]", "o");
        str = Regex.Replace(str, "[ùúụủũưừứựửữ]", "u");
        str = Regex.Replace(str, "[ỳýỵỷỹ]", "y");
        str = Regex.Replace(str, "đ", "d");
        str = Regex.Replace(str, " ", "");
        str = str.Replace(",", "");
        str = str.Replace(".", "");
        return str;
    }
}
