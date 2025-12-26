using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIMangager : MonoBehaviour
{
    [SerializeField] private TMP_Text buttonText;
    void UpdateButtonText(string newText)
    {
        // Cập nhật văn bản nút trong giao diện người dùng
        Debug.Log($"Button text updated to: {newText}");
        buttonText.text = newText;
    }

    void UpdateButtonTextTime(string newText, float time)
    {
        string previousText = buttonText.text;
        // Cập nhật văn bản nút trong giao diện người dùng trong một khoảng thời gian nhất định
        Debug.Log($"Button text updated to: {newText} for {time} seconds");
        buttonText.text = newText;
        StartCoroutine(RestoreButtonText(previousText, time));
    }
    IEnumerator RestoreButtonText(string previousText, float time)
    {
        yield return new WaitForSeconds(time);
        buttonText.text = previousText;
    }
}
