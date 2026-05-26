using UnityEngine;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    private string joinCode;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<string> CreateRelay(int maxPlayers = 4)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("[RelayManager] Already listening, skipping CreateRelay.");
            return null;
        }
        try
        {
            Debug.Log("Creating relay...");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
            Debug.Log($"Allocation created: {allocation.AllocationId}");

            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Join code: {joinCode}");

            var relayServerData = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started with Relay");
                return joinCode;
            }

            Debug.LogError("Failed to start host with Relay");
            return null;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay creation failed: {e.Message}");
            return null;
        }
    }

    public async Task<bool> JoinRelay(string code)
    {
        try
        {
            Debug.Log($"Joining relay with code: {code}");
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);
            Debug.Log($"Joined allocation: {joinAllocation.AllocationId}");

            var relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started with Relay");
                return true;
            }

            Debug.LogError("Failed to start client with Relay");
            return false;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError($"Relay join failed: {e.Message}");
            return false;
        }
    }

    public void Disconnect()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Disconnected from Relay");
        }
    }

    public string GetJoinCode() => joinCode;

    public void CopyJoinCodeToClipboard()
    {
        GUIUtility.systemCopyBuffer = joinCode;
        Debug.Log($"Join code copied to clipboard: {joinCode}");
    }
}
