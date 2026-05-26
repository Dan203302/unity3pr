using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class NetworkLauncher : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text statusText;

    async void Start()
    {
        await InitializeServicesAsync();
    }

    public void StartHostLocal()
    {
        if (NetworkManager.Singleton.IsListening) { SetStatus("Уже запущен."); return; }
        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetConnectionData("127.0.0.1", 7777);
        bool ok = NetworkManager.Singleton.StartHost();
        SetStatus(ok ? "Локальный Host запущен." : "Ошибка запуска Host.");
    }

    public void StartClientLocal()
    {
        if (NetworkManager.Singleton.IsListening) { SetStatus("Уже запущен."); return; }
        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetConnectionData("127.0.0.1", 7777);
        bool ok = NetworkManager.Singleton.StartClient();
        SetStatus(ok ? "Локальный Client подключён." : "Ошибка подключения Client.");
    }

    public async void StartHostRelay()
    {
        if (NetworkManager.Singleton.IsListening) { SetStatus("Уже запущен."); return; }
        SetStatus("Создание Relay...");
        if (RelayManager.Instance != null)
        {
            string code = await RelayManager.Instance.CreateRelay(4);
            SetStatus(code != null ? $"Relay Host запущен.\nJoin Code: {code}" : "Ошибка Relay.");
        }
        else
        {
            await InitializeServicesAsync();
            try
            {
                Allocation alloc = await RelayService.Instance.CreateAllocationAsync(3);
                string code = await RelayService.Instance.GetJoinCodeAsync(alloc.AllocationId);
                NetworkManager.Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(new RelayServerData(alloc, "dtls"));
                bool ok = NetworkManager.Singleton.StartHost();
                SetStatus(ok ? $"Relay Host запущен.\nJoin Code: {code}" : "Ошибка запуска Host.");
            }
            catch (System.Exception e) { SetStatus("Ошибка Relay: " + e.Message); }
        }
    }

    public async void StartClientRelay()
    {
        if (NetworkManager.Singleton.IsListening) { SetStatus("Уже запущен."); return; }
        string code = joinCodeInput != null ? joinCodeInput.text.Trim().ToUpper() : "";
        if (string.IsNullOrEmpty(code)) { SetStatus("Введите Join Code."); return; }

        SetStatus("Подключение через Relay...");
        if (RelayManager.Instance != null)
        {
            bool ok = await RelayManager.Instance.JoinRelay(code);
            SetStatus(ok ? "Client подключён через Relay." : "Ошибка подключения.");
        }
        else
        {
            await InitializeServicesAsync();
            try
            {
                JoinAllocation alloc = await RelayService.Instance.JoinAllocationAsync(code);
                NetworkManager.Singleton.GetComponent<UnityTransport>()
                    .SetRelayServerData(new RelayServerData(alloc, "dtls"));
                bool ok = NetworkManager.Singleton.StartClient();
                SetStatus(ok ? "Client подключён через Relay." : "Ошибка подключения Client.");
            }
            catch (System.Exception e) { SetStatus("Ошибка Relay: " + e.Message); }
        }
    }

    public void CopyJoinCode()
    {
        if (RelayManager.Instance != null)
            RelayManager.Instance.CopyJoinCodeToClipboard();
        SetStatus("Join Code скопирован!");
    }

    private async Task InitializeServicesAsync()
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
            await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
        Debug.Log("[NetworkLauncher] " + msg);
    }
}
