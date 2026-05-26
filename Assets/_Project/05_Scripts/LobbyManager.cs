using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    private Lobby currentLobby;
    private float lobbyHeartbeatTimer;
    private float lobbyUpdateTimer;

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

    private void Update()
    {
        HandleLobbyHeartbeat();
        HandleLobbyPolling();
    }

    public async Task<Lobby> CreateLobby(string lobbyName, int maxPlayers, bool isPrivate = false)
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = GetPlayer()
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log($"Created lobby: {currentLobby.Name}, ID: {currentLobby.Id}");
            return currentLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to create lobby: {e.Message}");
            return null;
        }
    }

    public async Task<List<Lobby>> FindLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 10,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);
            return response.Results;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to find lobbies: {e.Message}");
            return new List<Lobby>();
        }
    }

    public async Task<Lobby> JoinLobby(string lobbyId)
    {
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions { Player = GetPlayer() };
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
            Debug.Log($"Joined lobby: {currentLobby.Name}");
            return currentLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby: {e.Message}");
            return null;
        }
    }

    public async Task<Lobby> JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions { Player = GetPlayer() };
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            Debug.Log($"Joined lobby by code: {currentLobby.Name}");
            return currentLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to join lobby by code: {e.Message}");
            return null;
        }
    }

    public async void LeaveLobby()
    {
        if (currentLobby == null) return;
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(
                currentLobby.Id, AuthenticationManager.Instance.PlayerId);
            currentLobby = null;
            Debug.Log("Left lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to leave lobby: {e.Message}");
        }
    }

    private async void HandleLobbyHeartbeat()
    {
        if (currentLobby == null || !IsLobbyHost()) return;
        lobbyHeartbeatTimer -= Time.deltaTime;
        if (lobbyHeartbeatTimer <= 0f)
        {
            lobbyHeartbeatTimer = 15f;
            try { await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id); }
            catch (LobbyServiceException e) { Debug.LogError($"Heartbeat failed: {e.Message}"); }
        }
    }

    private async void HandleLobbyPolling()
    {
        if (currentLobby == null) return;
        lobbyUpdateTimer -= Time.deltaTime;
        if (lobbyUpdateTimer <= 0f)
        {
            lobbyUpdateTimer = 1.1f;
            try { currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id); }
            catch (LobbyServiceException e) { Debug.LogError($"Lobby poll failed: {e.Message}"); }
        }
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Public,
                    AuthenticationManager.Instance?.PlayerName ?? "Anonymous") }
            }
        };
    }

    private bool IsLobbyHost() =>
        currentLobby != null &&
        AuthenticationManager.Instance != null &&
        currentLobby.HostId == AuthenticationManager.Instance.PlayerId;

    private void OnDestroy() => LeaveLobby();
}
