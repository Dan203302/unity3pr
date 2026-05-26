using UnityEngine;
using System.Collections.Generic;

public class PCGraphicsSettings : MonoBehaviour
{
    public enum TextureQuality { FullRes, HalfRes, QuarterRes, EighthRes }

    [System.Serializable]
    public class GraphicsPreset
    {
        public string  name             = "Высокое";
        public int     qualityLevel     = 2;
        public int     width            = 1920;
        public int     height           = 1080;
        public int     refreshRate      = 60;
        public bool    fullscreen       = true;
        public int     antiAliasing     = 4;
        public bool    shadows          = true;
        public TextureQuality textureQuality = TextureQuality.FullRes;
        public float   shadowDistance   = 100f;
        public bool    softParticles    = true;
        public bool    realtimeReflectionProbes = true;
    }

    [SerializeField] private List<GraphicsPreset> presets = new List<GraphicsPreset>();
    [SerializeField] private GraphicsPreset currentPreset = new GraphicsPreset();

    private List<Resolution> filteredResolutions = new List<Resolution>();

    void Start()
    {
        InitDefaultPresets();
        InitResolutions();
        LoadSettings();
        ApplySettings();
    }

    void InitDefaultPresets()
    {
        if (presets.Count > 0) return;
        presets.Add(new GraphicsPreset { name = "Низкое",    qualityLevel = 0, antiAliasing = 0, shadows = false, textureQuality = TextureQuality.QuarterRes, shadowDistance = 20f  });
        presets.Add(new GraphicsPreset { name = "Среднее",   qualityLevel = 1, antiAliasing = 2, shadows = true,  textureQuality = TextureQuality.HalfRes,    shadowDistance = 50f  });
        presets.Add(new GraphicsPreset { name = "Высокое",   qualityLevel = 2, antiAliasing = 4, shadows = true,  textureQuality = TextureQuality.FullRes,    shadowDistance = 100f });
        presets.Add(new GraphicsPreset { name = "Ультра",    qualityLevel = 5, antiAliasing = 8, shadows = true,  textureQuality = TextureQuality.FullRes,    shadowDistance = 200f, softParticles = true, realtimeReflectionProbes = true });
    }

    void InitResolutions()
    {
        var seen = new System.Collections.Generic.HashSet<string>();
        foreach (var r in Screen.resolutions)
        {
            string key = $"{r.width}x{r.height}";
            if (seen.Add(key)) filteredResolutions.Add(r);
        }
        filteredResolutions.Sort((a, b) => (b.width * b.height).CompareTo(a.width * a.height));
    }

    public void LoadSettings()
    {
        int idx = PlayerPrefs.GetInt("GraphicsPreset", 2);
        if (idx >= 0 && idx < presets.Count)
            currentPreset = presets[idx];
        currentPreset.width       = PlayerPrefs.GetInt("ResW",  Screen.currentResolution.width);
        currentPreset.height      = PlayerPrefs.GetInt("ResH",  Screen.currentResolution.height);
        currentPreset.refreshRate = PlayerPrefs.GetInt("ResHz", 60);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("GraphicsPreset", presets.IndexOf(currentPreset));
        PlayerPrefs.SetInt("ResW",  currentPreset.width);
        PlayerPrefs.SetInt("ResH",  currentPreset.height);
        PlayerPrefs.SetInt("ResHz", currentPreset.refreshRate);
        PlayerPrefs.Save();
    }

    public void ApplySettings()
    {
        Screen.SetResolution(currentPreset.width, currentPreset.height,
            currentPreset.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);

        QualitySettings.SetQualityLevel(currentPreset.qualityLevel);
        QualitySettings.antiAliasing      = currentPreset.antiAliasing;
        QualitySettings.shadows           = currentPreset.shadows ? ShadowQuality.All : ShadowQuality.Disable;
        QualitySettings.shadowDistance    = currentPreset.shadowDistance;
        QualitySettings.softParticles     = currentPreset.softParticles;
        QualitySettings.realtimeReflectionProbes = currentPreset.realtimeReflectionProbes;
        QualitySettings.globalTextureMipmapLimit = (int)currentPreset.textureQuality;

        Debug.Log($"[PCGraphics] Применён пресет: {currentPreset.name} ({currentPreset.width}x{currentPreset.height})");
    }

    public void SetPreset(int idx)
    {
        if (idx < 0 || idx >= presets.Count) return;
        currentPreset = presets[idx];
        ApplySettings(); SaveSettings();
    }

    public void SetFullscreen(bool fs)
    {
        currentPreset.fullscreen = fs;
        ApplySettings(); SaveSettings();
    }

    public void SetResolution(int w, int h, int hz = 60)
    {
        currentPreset.width = w; currentPreset.height = h; currentPreset.refreshRate = hz;
        ApplySettings(); SaveSettings();
    }

    public void AutoDetect()
    {
        int vram  = SystemInfo.graphicsMemorySize;
        int cores = SystemInfo.processorCount;
        Debug.Log($"[PCGraphics] GPU: {SystemInfo.graphicsDeviceName}, VRAM: {vram}MB, Cores: {cores}");
        int preset = vram >= 8192 && cores >= 8 ? 3 : vram >= 4096 && cores >= 4 ? 2 : 1;
        SetPreset(preset);
    }

    public List<Resolution>    GetResolutions() => filteredResolutions;
    public List<GraphicsPreset> GetPresets()    => presets;
    public GraphicsPreset       GetCurrent()    => currentPreset;
}
