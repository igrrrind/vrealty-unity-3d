using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using System.Collections;

public class NetworkManagerUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Toggle autoModeToggle;
    [SerializeField] private GameObject manualPanel;
    [SerializeField] private GameObject statusText;
    
    [Header("Auto Mode Settings")]
    [SerializeField] private float connectionTimeout = 3f;
    
    private bool isAutoMode = false;

    private void Awake()
    {
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            HideUI();
        });
        
        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            HideUI();
        });
        
        if (autoModeToggle != null)
        {
            autoModeToggle.onValueChanged.AddListener(OnAutoModeToggled);
        }
    }

    private void Start()
    {
        // Check if auto mode should be enabled by default
        if (autoModeToggle != null && autoModeToggle.isOn)
        {
            StartAutoMode();
        }
    }

    private void OnAutoModeToggled(bool isOn)
    {
        isAutoMode = isOn;
        
        if (isOn)
        {
            StartAutoMode();
        }
        else
        {
            ShowManualUI();
        }
    }

    private void StartAutoMode()
    {
        if (manualPanel != null)
            manualPanel.SetActive(false);
            
        UpdateStatus("Đang tìm lobby...");
        StartCoroutine(AutoConnectRoutine());
    }

    private IEnumerator AutoConnectRoutine()
    {
        // Try to join as client first
        UpdateStatus("Đang thử kết nối...");
        NetworkManager.Singleton.StartClient();
        
        float timeElapsed = 0f;
        
        // Wait for connection or timeout
        while (timeElapsed < connectionTimeout)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                UpdateStatus("Đã join lobby!");
                HideUI();
                yield break;
            }
            
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        
        // If connection failed, shutdown and become host
        NetworkManager.Singleton.Shutdown();
        
        // Wait a frame for shutdown to complete
        yield return new WaitForSeconds(0.5f);
        
        UpdateStatus("Không tìm thấy lobby, đang tạo lobby...");
        NetworkManager.Singleton.StartHost();
        
        yield return new WaitForSeconds(0.5f);
        
        if (NetworkManager.Singleton.IsHost)
        {
            UpdateStatus("Đã tạo lobby! Đang chờ người chơi khác...");
        }
        
        HideUI();
    }

    private void ShowManualUI()
    {
        if (manualPanel != null)
            manualPanel.SetActive(true);
        if (statusText != null)
            statusText.SetActive(false);
    }

    private void HideUI()
    {
        if (hostButton != null)
            hostButton.gameObject.SetActive(false);
        if (clientButton != null)
            clientButton.gameObject.SetActive(false);
        if (autoModeToggle != null)
            autoModeToggle.gameObject.SetActive(false);
        if (manualPanel != null)
            manualPanel.SetActive(false);
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.SetActive(true);
            var textComponent = statusText.GetComponent<TMPro.TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = message;
            }
        }
        Debug.Log($"[Network] {message}");
    }
}
