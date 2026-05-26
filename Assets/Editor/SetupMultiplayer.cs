using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class SetupMultiplayer
{
    [MenuItem("Tools/Setup Multiplayer")]
    public static void Run()
    {
        // ── NetworkManager ──────────────────────────────────────────
        GameObject nmGo = GameObject.Find("NetworkManager");
        if (nmGo == null) nmGo = new GameObject("NetworkManager");

        if (nmGo.GetComponent<NetworkManager>() == null)
            nmGo.AddComponent<NetworkManager>();
        var transport = nmGo.GetComponent<UnityTransport>();
        if (transport == null) transport = nmGo.AddComponent<UnityTransport>();
        if (nmGo.GetComponent<NetworkLauncher>() == null)
            nmGo.AddComponent<NetworkLauncher>();
        if (nmGo.GetComponent<AuthenticationManager>() == null)
            nmGo.AddComponent<AuthenticationManager>();
        if (nmGo.GetComponent<RelayManager>() == null)
            nmGo.AddComponent<RelayManager>();
        if (nmGo.GetComponent<LobbyManager>() == null)
            nmGo.AddComponent<LobbyManager>();

        // Assign transport to NetworkManager
        var nm = nmGo.GetComponent<NetworkManager>();
        var nmSo = new SerializedObject(nm);
        var transportProp = nmSo.FindProperty("NetworkConfig.NetworkTransport");
        if (transportProp != null)
        {
            transportProp.objectReferenceValue = transport;
            nmSo.ApplyModifiedProperties();
            Debug.Log("[Multiplayer] UnityTransport assigned to NetworkManager.");
        }

        // ── Canvas ──────────────────────────────────────────────────
        GameObject canvasGo = GameObject.Find("MultiplayerCanvas");
        if (canvasGo == null)
        {
            canvasGo = new GameObject("MultiplayerCanvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            ((CanvasScaler)canvasGo.GetComponent<CanvasScaler>()).referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        // ── Panel ───────────────────────────────────────────────────
        GameObject panel = canvasGo.transform.Find("MPPanel")?.gameObject;
        if (panel == null) panel = new GameObject("MPPanel");
        panel.transform.SetParent(canvasGo.transform, false);
        if (panel.GetComponent<RectTransform>() == null) panel.AddComponent<RectTransform>();
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(1, 0);
        panelRt.anchorMax = new Vector2(1, 0);
        panelRt.pivot = new Vector2(1, 0);
        panelRt.anchoredPosition = new Vector2(-10, 10);
        panelRt.sizeDelta = new Vector2(340, 340);
        var panelImg = panel.GetComponent<Image>() ?? panel.AddComponent<Image>();
        panelImg.color = new Color(0.05f, 0.05f, 0.1f, 0.9f);

        // ── Status text ─────────────────────────────────────────────
        GameObject statusGo = MakeTMP(panel, "StatusText", "Статус: ожидание...",
            new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
            new Vector2(0, -24), new Vector2(-16, 40), 13, Color.white);

        // ── Join Code input ─────────────────────────────────────────
        GameObject inputGo = panel.transform.Find("JoinCodeInput")?.gameObject;
        if (inputGo == null) inputGo = new GameObject("JoinCodeInput");
        inputGo.transform.SetParent(panel.transform, false);
        if (inputGo.GetComponent<RectTransform>() == null) inputGo.AddComponent<RectTransform>();
        var inputRt = inputGo.GetComponent<RectTransform>();
        inputRt.anchorMin = new Vector2(0, 1); inputRt.anchorMax = new Vector2(1, 1);
        inputRt.pivot = new Vector2(0.5f, 1);
        inputRt.anchoredPosition = new Vector2(0, -72); inputRt.sizeDelta = new Vector2(-16, 32);
        var inputImg = inputGo.GetComponent<Image>() ?? inputGo.AddComponent<Image>();
        inputImg.color = Color.white;
        var tmpInput = inputGo.GetComponent<TMP_InputField>() ?? inputGo.AddComponent<TMP_InputField>();
        tmpInput.placeholder = MakeTMP(inputGo, "Placeholder", "Join Code...",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, 13, new Color(0.5f, 0.5f, 0.5f)).GetComponent<TMP_Text>();
        tmpInput.textComponent = MakeTMP(inputGo, "InputText", "",
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, 13, Color.black).GetComponent<TMP_Text>();

        // ── Buttons ─────────────────────────────────────────────────
        MakeButton(panel, "Btn_HostLocal",   "Host (Локальный)",   new Vector2(0, -115));
        MakeButton(panel, "Btn_ClientLocal", "Client (Локальный)", new Vector2(0, -157));
        MakeButton(panel, "Btn_HostRelay",   "Host (Relay)",       new Vector2(0, -199));
        MakeButton(panel, "Btn_ClientRelay", "Client (Relay)",     new Vector2(0, -241));

        // ── Wire buttons → NetworkLauncher ───────────────────────────
        NetworkLauncher launcher = nmGo.GetComponent<NetworkLauncher>();
        WireBtn(panel, "Btn_HostLocal",   launcher, "StartHostLocal");
        WireBtn(panel, "Btn_ClientLocal", launcher, "StartClientLocal");
        WireBtn(panel, "Btn_HostRelay",   launcher, "StartHostRelay");
        WireBtn(panel, "Btn_ClientRelay", launcher, "StartClientRelay");
        MakeButton(panel, "Btn_CopyCode", "Скопировать Join Code", new Vector2(0, -278));
        WireBtn(panel, "Btn_CopyCode", launcher, "CopyJoinCode");

        // ── Assign serialized refs ───────────────────────────────────
        var so = new SerializedObject(launcher);
        var statusTmp = statusGo.GetComponent<TMP_Text>();
        if (statusTmp) so.FindProperty("statusText").objectReferenceValue = statusTmp;
        so.FindProperty("joinCodeInput").objectReferenceValue = tmpInput;
        so.ApplyModifiedProperties();

        // ── NetworkPlayer Prefab ─────────────────────────────────────
        SetupNetworkPlayerPrefab(nmGo);

        EditorUtility.SetDirty(nmGo);
        EditorUtility.SetDirty(canvasGo);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("[Multiplayer] Setup complete!");
    }

    static void SetupNetworkPlayerPrefab(GameObject nmGo)
    {
        const string prefabPath = "Assets/_Project/NetworkPlayer.prefab";

        // Delete old prefab to rebuild with correct mesh
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
            AssetDatabase.DeleteAsset(prefabPath);

        // Root object
        GameObject root = new GameObject("NetworkPlayer");

        // Try to get Base_Mesh from Player in scene
        GameObject playerGo = GameObject.Find("Player");
        GameObject meshSource = null;
        if (playerGo != null)
        {
            Transform baseMesh = playerGo.transform.Find("Base_Mesh");
            if (baseMesh != null)
                meshSource = baseMesh.gameObject;
        }

        if (meshSource != null)
        {
            // Duplicate Base_Mesh as child
            GameObject meshCopy = Object.Instantiate(meshSource);
            meshCopy.name = "Base_Mesh";
            meshCopy.transform.SetParent(root.transform, false);
            meshCopy.transform.localPosition = Vector3.zero;
            meshCopy.transform.localRotation = Quaternion.identity;
            meshCopy.transform.localScale = Vector3.one;

            // Assign first renderer found
            var rend = meshCopy.GetComponentInChildren<Renderer>();
            root.AddComponent<Unity.Netcode.NetworkObject>();
            var np = root.AddComponent<NetworkPlayer>();
            if (rend != null)
            {
                var so = new SerializedObject(np);
                so.FindProperty("playerRenderer").objectReferenceValue = rend;
                so.ApplyModifiedProperties();
            }
        }
        else
        {
            // Fallback: capsule
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.name = "Body";
            capsule.transform.SetParent(root.transform, false);
            root.AddComponent<Unity.Netcode.NetworkObject>();
            var np = root.AddComponent<NetworkPlayer>();
            var so = new SerializedObject(np);
            so.FindProperty("playerRenderer").objectReferenceValue = capsule.GetComponent<Renderer>();
            so.ApplyModifiedProperties();
        }

        // Ensure folder exists
        if (!System.IO.Directory.Exists("Assets/_Project"))
            AssetDatabase.CreateFolder("Assets", "_Project");

        GameObject existingPrefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        Debug.Log("[Multiplayer] NetworkPlayer prefab created at " + prefabPath);

        // Assign to NetworkManager
        var nm = nmGo.GetComponent<NetworkManager>();
        if (nm != null && existingPrefab != null)
        {
            var nmSo = new SerializedObject(nm);
            var playerPrefabProp = nmSo.FindProperty("NetworkConfig.PlayerPrefab");
            if (playerPrefabProp != null)
            {
                playerPrefabProp.objectReferenceValue = existingPrefab;
                nmSo.ApplyModifiedProperties();
                Debug.Log("[Multiplayer] Player Prefab assigned to NetworkManager.");
            }
        }
    }

    static TMP_FontAsset GetFont() =>
        AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

    static GameObject MakeTMP(GameObject parent, string name, string text,
        Vector2 ancMin, Vector2 ancMax, Vector2 pivot,
        Vector2 ancPos, Vector2 sizeDelta, float size, Color color)
    {
        Transform t = parent.transform.Find(name);
        GameObject go = t != null ? t.gameObject : new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var tmp = go.GetComponent<TextMeshProUGUI>() ?? go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        var f = GetFont(); if (f) tmp.font = f;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax; rt.pivot = pivot;
        rt.anchoredPosition = ancPos; rt.sizeDelta = sizeDelta;
        return go;
    }

    static void MakeButton(GameObject parent, string name, string label, Vector2 anchoredPos)
    {
        Transform t = parent.transform.Find(name);
        GameObject go = t != null ? t.gameObject : new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = Color.white;
        var btn = go.GetComponent<Button>() ?? go.AddComponent<Button>();
        var cb = btn.colors;
        cb.normalColor      = new Color(0.15f, 0.47f, 0.85f);
        cb.highlightedColor = new Color(0.25f, 0.60f, 1f);
        cb.pressedColor     = new Color(0.08f, 0.32f, 0.65f);
        btn.colors = cb;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = anchoredPos; rt.sizeDelta = new Vector2(-16, 36);

        MakeTMP(go, "Label", label, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
            Vector2.zero, Vector2.zero, 13, Color.white);
    }

    static void WireBtn(GameObject panel, string btnName, NetworkLauncher launcher, string method)
    {
        Transform t = panel.transform.Find(btnName);
        if (t == null) return;
        var btn = t.GetComponent<Button>();
        if (btn == null) return;
        var so = new SerializedObject(btn);
        var calls = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
        if (calls != null) { calls.ClearArray(); so.ApplyModifiedProperties(); }
        btn.onClick.RemoveAllListeners();
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick,
            System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction),
                launcher, method) as UnityEngine.Events.UnityAction);
    }
}
