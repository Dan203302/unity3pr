using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class AuthenticationManager : MonoBehaviour
{
    [Header("Authentication Settings")]
    [SerializeField] private bool autoSignIn = true;

    public static AuthenticationManager Instance { get; private set; }

    public bool IsAuthenticated { get; private set; }
    public string PlayerId { get; private set; }
    public string PlayerName { get; private set; }

    private async void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        await InitializeUnityServices();

        if (autoSignIn)
            await SignInAnonymouslyAsync();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync();
            Debug.Log("Unity Services initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Services: {e.Message}");
        }
    }

    public async Task<bool> SignInAnonymouslyAsync()
    {
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                IsAuthenticated = true;
                PlayerId = AuthenticationService.Instance.PlayerId;
                PlayerName = AuthenticationService.Instance.PlayerName ?? "Anonymous";
                Debug.Log("Already signed in. Player ID: " + PlayerId);
                return true;
            }

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            IsAuthenticated = true;
            PlayerId = AuthenticationService.Instance.PlayerId;
            PlayerName = AuthenticationService.Instance.PlayerName ?? "Anonymous";
            Debug.Log($"Signed in! Player ID: {PlayerId}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Sign in failed: {e.Message}");
            IsAuthenticated = false;
            return false;
        }
    }

    public void SignOut()
    {
        AuthenticationService.Instance.SignOut();
        IsAuthenticated = false;
        PlayerId = string.Empty;
        PlayerName = string.Empty;
        Debug.Log("Signed out");
    }

    public async Task UpdatePlayerNameAsync(string newName)
    {
        if (!IsAuthenticated)
        {
            Debug.LogWarning("Cannot update name: not authenticated");
            return;
        }
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(newName);
            PlayerName = newName;
            Debug.Log($"Player name updated to: {PlayerName}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update player name: {e.Message}");
        }
    }

    private void OnEnable()
    {
        if (AuthenticationService.Instance != null)
        {
            AuthenticationService.Instance.SignedIn  += OnSignedIn;
            AuthenticationService.Instance.SignedOut += OnSignedOut;
        }
    }

    private void OnDisable()
    {
        if (AuthenticationService.Instance != null)
        {
            AuthenticationService.Instance.SignedIn  -= OnSignedIn;
            AuthenticationService.Instance.SignedOut -= OnSignedOut;
        }
    }

    private void OnSignedIn()
    {
        IsAuthenticated = true;
        PlayerId   = AuthenticationService.Instance.PlayerId;
        PlayerName = AuthenticationService.Instance.PlayerName;
        Debug.Log("Authentication state changed: Signed In");
    }

    private void OnSignedOut()
    {
        IsAuthenticated = false;
        Debug.Log("Authentication state changed: Signed Out");
    }
}
