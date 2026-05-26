using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkObject))]
public class NetworkPlayer : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Visual")]
    [SerializeField] private Renderer playerRenderer;
    [SerializeField] private GameObject nameTagObject;

    private readonly NetworkVariable<int> colorIndex =
        new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private readonly NetworkVariable<Vector3> netPosition =
        new(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private readonly NetworkVariable<Quaternion> netRotation =
        new(Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private static readonly Color[] PlayerColors =
        { Color.red, Color.blue, Color.green, Color.yellow };

    private Transform realPlayer;
    private float logTimer;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            colorIndex.Value = (int)(OwnerClientId % (ulong)PlayerColors.Length);

        ApplyColor(colorIndex.Value);
        colorIndex.OnValueChanged += (_, newVal) => ApplyColor(newVal);

        if (IsOwner)
        {
            // Ищем через компонент — надёжнее чем по тегу
            var fpc = Object.FindFirstObjectByType<FirstPersonController>();
            if (fpc != null)
            {
                realPlayer = fpc.transform;
                netPosition.Value = realPlayer.position;
                netRotation.Value = realPlayer.rotation;
                transform.position = realPlayer.position;
                transform.rotation = realPlayer.rotation;
                Debug.Log($"[NetworkPlayer] Owner {OwnerClientId} нашёл Player: {realPlayer.position}");
            }
            else
            {
                Debug.LogWarning($"[NetworkPlayer] Owner {OwnerClientId}: FirstPersonController не найден!");
            }

            // Скрываем свой NetworkPlayer — мы и так уже видим себя через реального Player
            foreach (var r in GetComponentsInChildren<Renderer>())
                r.enabled = false;

            if (nameTagObject != null) nameTagObject.SetActive(false);
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            if (realPlayer != null)
            {
                netPosition.Value = realPlayer.position;
                netRotation.Value = realPlayer.rotation;
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, netPosition.Value, Time.deltaTime * 15f);
            transform.rotation = Quaternion.Slerp(transform.rotation, netRotation.Value, Time.deltaTime * 15f);
        }

        // Лог каждые 3 сек
        logTimer -= Time.deltaTime;
        if (logTimer <= 0f)
        {
            logTimer = 3f;
            Debug.Log($"[NetworkPlayer] ClientID={OwnerClientId} | IsOwner={IsOwner} | pos={netPosition.Value:F2} | realPlayer={(realPlayer != null ? realPlayer.position.ToString("F2") : "NULL")}");
        }
    }

    private void ApplyColor(int index)
    {
        if (playerRenderer == null) return;
        int safe = Mathf.Clamp(index, 0, PlayerColors.Length - 1);
        playerRenderer.material.color = PlayerColors[safe];
    }
}
