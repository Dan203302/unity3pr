using UnityEngine;
using System.Collections.Generic;

public class MobileOptimizer : MonoBehaviour
{
    [System.Serializable]
    public class OptimizationPreset
    {
        public string name              = "Авто";
        public int    targetFPS         = 30;
        public bool   disableShadows    = true;
        public bool   combineMeshes     = true;
        public bool   useOcclusion      = true;
        public bool   reduceParticles   = true;
        public int    textureSizeLimit  = 512;
        public int    maxParticles      = 50;
    }

    [SerializeField] private List<OptimizationPreset> presets = new List<OptimizationPreset>();
    [SerializeField] private OptimizationPreset currentPreset;
#pragma warning disable 0414
    [SerializeField] private bool showFPSCounter  = true;
#pragma warning restore 0414

    private float fpsTimer;
    private float currentFPS;
    private int   frameCount;

    void Start()
    {
#if UNITY_ANDROID || UNITY_IOS
        InitDefaultPresets();
        AutoDetect();
        ApplyOptimizations();
        Application.lowMemory += OnLowMemory;
#endif
    }

    void Update()
    {
#if UNITY_ANDROID || UNITY_IOS
        frameCount++;
        fpsTimer += Time.unscaledDeltaTime;
        if (fpsTimer >= 1f)
        {
            currentFPS = frameCount / fpsTimer;
            frameCount = 0; fpsTimer = 0f;

            // Динамическое снижение качества если FPS низкий
            if (currentPreset != null && currentFPS < currentPreset.targetFPS * 0.7f)
            {
                int lvl = QualitySettings.GetQualityLevel();
                if (lvl > 0) { QualitySettings.SetQualityLevel(lvl - 1, true); Debug.Log($"[MobileOpt] FPS низкий ({currentFPS:F0}), снижаем качество до {lvl-1}"); }
            }
        }
#endif
    }

    void InitDefaultPresets()
    {
        if (presets.Count > 0) return;
        presets.Add(new OptimizationPreset { name = "Низкий",   targetFPS = 30, disableShadows = true,  textureSizeLimit = 256, maxParticles = 20  });
        presets.Add(new OptimizationPreset { name = "Средний",  targetFPS = 30, disableShadows = true,  textureSizeLimit = 512, maxParticles = 50  });
        presets.Add(new OptimizationPreset { name = "Высокий",  targetFPS = 60, disableShadows = false, textureSizeLimit = 1024,maxParticles = 100 });
    }

    public void AutoDetect()
    {
        int mem = SystemInfo.systemMemorySize, cores = SystemInfo.processorCount;
        Debug.Log($"[MobileOpt] Устройство: {SystemInfo.deviceModel} | RAM: {mem}MB | CPU: {cores} ядер");

        if      (mem >= 4000 && cores >= 4) currentPreset = presets.Find(p => p.name == "Высокий");
        else if (mem >= 2000 && cores >= 2) currentPreset = presets.Find(p => p.name == "Средний");
        else                                currentPreset = presets.Find(p => p.name == "Низкий");

        currentPreset ??= new OptimizationPreset();
        Debug.Log($"[MobileOpt] Выбран пресет: {currentPreset.name}");
    }

    public void ApplyOptimizations()
    {
        if (currentPreset == null) return;

        Application.targetFrameRate = currentPreset.targetFPS;

        if (currentPreset.disableShadows)
            QualitySettings.shadows = ShadowQuality.Disable;

        QualitySettings.globalTextureMipmapLimit = currentPreset.textureSizeLimit >= 1024 ? 0 :
                                             currentPreset.textureSizeLimit >= 512  ? 1 :
                                             currentPreset.textureSizeLimit >= 256  ? 2 : 3;

        if (currentPreset.useOcclusion && Camera.main != null)
            Camera.main.useOcclusionCulling = true;

        if (currentPreset.reduceParticles)
            OptimizeParticles(currentPreset.maxParticles);

        if (currentPreset.combineMeshes)
            Debug.Log("[MobileOpt] Объединение мешей рекомендуется для сцены.");

        Debug.Log($"[MobileOpt] Оптимизации применены: {currentPreset.name}");
    }

    void OptimizeParticles(int maxCount)
    {
        foreach (var ps in FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None))
        {
            var main = ps.main;
            if (main.maxParticles > maxCount)
            {
                main.maxParticles = maxCount;
                Debug.Log($"[MobileOpt] Частицы ограничены: {ps.name}");
            }
        }
    }

    void OnLowMemory()
    {
        Debug.LogWarning("[MobileOpt] Низкая память! Очистка ресурсов...");
        Resources.UnloadUnusedAssets();
        QualitySettings.SetQualityLevel(0, true);
        System.GC.Collect();
    }

    void OnGUI()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!showFPSCounter) return;
        GUI.Label(new Rect(10, 10, 100, 30), $"FPS: {currentFPS:F0}");
#endif
    }

    void OnDestroy()
    {
#if UNITY_ANDROID || UNITY_IOS
        Application.lowMemory -= OnLowMemory;
#endif
    }

    public float GetCurrentFPS() => currentFPS;
}
