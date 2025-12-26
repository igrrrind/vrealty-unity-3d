using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Vivox;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    private Lobby _currentLobby;
    private const string RelayKey = "MyRelayJoinCode";
    private bool isVoiceMuted = false;

    async void Start()
    {
        // 1. Khởi tạo toàn bộ dịch vụ
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        await VivoxService.Instance.InitializeAsync();

        Debug.Log("Services Ready. Starting Auto-Join...");
        await FindOrCreateGame();
    }

    void Update()
    {
        // Toggle voice chat with K key
        if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleVoiceChat();
        }
    }

    private async Task FindOrCreateGame()
    {
        try
        {
            // 2. Thử Quick Join vào một lobby bất kỳ
            _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            // Check if the lobby contains the Relay join code in its data
            if (!_currentLobby.Data.TryGetValue(RelayKey, out var relayJoinCodeObject))
            {
                Debug.LogError("Relay join code not found in lobby data.");
                return;
            }
            var relayJoinCode = relayJoinCodeObject.Value;
            Debug.Log($"Retrieved Relay join code: {relayJoinCode}");

            // Join the Relay allocation using the join code
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            Debug.Log("Successfully joined Relay allocation.");

            // Set Relay server data for the UnityTransport - MUST USE WSS for WebGL
            var connectionType = "wss";
            var relayServerData = joinAllocation.ToRelayServerData(connectionType);
            
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(relayServerData);
            Debug.Log($"Relay server data set successfully with connection type: {connectionType}");

            // Start the client
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started successfully.");

            await JoinVivox(relayJoinCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"LobbyServiceException: {e.Message}");
            // 3. Nếu không tìm thấy Lobby nào (Lỗi 16001), chúng ta sẽ làm Host
            await CreateLobby("MyAutoLobby", false);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"RelayServiceException: {e.Message}");
        }
    }
    public async Task CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            var options = new CreateLobbyOptions();
            options.IsPrivate = isPrivate;

            // Tạo Lobby thông qua LobbyService
            _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 4, options);

            // Lấy danh sách vùng (Regions) và chọn vùng đầu tiên
            var regions = await RelayService.Instance.ListRegionsAsync();
            var region = regions[0].Id;

            // Tạo Allocation cho Relay
            var hostAllocation = await RelayService.Instance.CreateAllocationAsync(4, region);

            // Lấy Join Code từ Allocation
            var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

            // ĐỐI VỚI WEBGL: Cần đổi "udp" thành "wss" để chạy được trên trình duyệt
            var connectionType = "wss";

            var relayServerData = hostAllocation.ToRelayServerData(connectionType);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // Cập nhật Join Code vào dữ liệu của Lobby để người khác có thể thấy
            await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
            {
                { RelayKey, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
            }
            });
            InvokeRepeating(nameof(SendLobbyHeartbeat), 15, 15);

            // Bắt đầu Host và chuyển Scene
            NetworkManager.Singleton.StartHost();
            await JoinVivox(relayJoinCode);

            Debug.Log($"Created new game as Host. Lobby ID: {_currentLobby.Id}");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    private async Task CreateGame()
    {
        // Tạo Relay
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4); // Cho phép tối đa 4 người
        string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // Cấu hình Transport với WSS (WebSocket Secure)
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        // Tìm endpoint WSS từ allocation
        var wssEndpoint = allocation.ServerEndpoints.Find(e => e.ConnectionType == "wss");
        if (wssEndpoint != null)
        {
            transport.SetHostRelayData(wssEndpoint.Host, (ushort)wssEndpoint.Port,
                allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, true);
        }
        // else
        // {
        //     // Fallback to UDP if WSS not available
        //     Debug.LogWarning("WSS endpoint not found, using UDP");
        //     transport.SetHostRelayData(allocation.RelayServer.IpV4, (ushort)allocation.RelayServer.Port,
        //         allocation.AllocationIdBytes, allocation.Key, allocation.ConnectionData, false);
        // }

        // Tạo Lobby và lưu Relay Join Code vào Data
        var options = new CreateLobbyOptions
        {
            IsPrivate = false,
            Data = new Dictionary<string, DataObject> {
                { RelayKey, new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
            }
        };

        _currentLobby = await LobbyService.Instance.CreateLobbyAsync("MyAutoLobby", 10, options);

        // Bắt đầu gửi Heartbeat để Lobby không bị xóa (Quan trọng!)
        InvokeRepeating(nameof(SendLobbyHeartbeat), 15, 15);

        NetworkManager.Singleton.StartHost();
        await JoinVivox(relayJoinCode);

        Debug.Log($"Created new game as Host. Lobby ID: {_currentLobby.Id}");
    }

    private async void SendLobbyHeartbeat()
    {
        if (_currentLobby != null)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
        }
    }

    private async Task JoinVivox(string channelName)
    {
        await VivoxService.Instance.LoginAsync();
        await VivoxService.Instance.JoinGroupChannelAsync(channelName, ChatCapability.AudioOnly);
        Debug.Log("Vivox Voice Joined!");
    }

    private void ToggleVoiceChat()
    {
        isVoiceMuted = !isVoiceMuted;
        // VivoxService.Instance.SetInputDeviceMuted(isVoiceMuted);
        Debug.Log($"Voice chat {(isVoiceMuted ? "muted" : "unmuted")}");
    }
}