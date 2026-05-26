using UnityEngine;
using UnityEditor;

public class AddGrassTexture
{
    [MenuItem("Tools/Add Grass Details To Terrain")]
    public static void AddGrassDetails()
    {
        Terrain terrain = Object.FindFirstObjectByType<Terrain>();
        if (terrain == null) { Debug.LogError("Terrain not found!"); return; }

        Texture2D grass1 = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grass01.tga");
        if (grass1 == null) { Debug.LogError("grass01.tga not found!"); return; }

        TerrainData td = terrain.terrainData;

        // Force clear existing details
        td.detailPrototypes = new DetailPrototype[0];

        td.detailPrototypes = new DetailPrototype[]
        {
            new DetailPrototype {
                prototypeTexture = grass1,
                renderMode = DetailRenderMode.Grass,
                minHeight = 0.5f, maxHeight = 1.0f,
                minWidth = 0.5f,  maxWidth = 1.0f,
                noiseSpread = 0.1f,
                healthyColor = new Color(0.26f, 0.75f, 0.1f),
                dryColor    = new Color(0.80f, 0.70f, 0.15f),
                useInstancing = false
            }
        };

        // Make sure detail resolution is adequate
        if (td.detailResolution < 256)
            td.SetDetailResolution(512, 8);

        int dw = td.detailWidth;
        int dh = td.detailHeight;
        int[,] layer = new int[dh, dw];
        for (int y = 0; y < dh; y++)
            for (int x = 0; x < dw; x++)
                layer[y, x] = 8; // density per cell

        td.SetDetailLayer(0, 0, 0, layer);

        terrain.detailObjectDensity = 1f;
        terrain.detailObjectDistance = 100f;
        terrain.drawTreesAndFoliage = true;

        EditorUtility.SetDirty(td);
        EditorUtility.SetDirty(terrain);
        AssetDatabase.SaveAssets();

        Debug.Log($"[Grass] Done! Resolution: {dw}x{dh}, density=8 per cell.");
    }

    [MenuItem("Tools/Add Grass To Terrain")]
    public static void Run()
    {
        Terrain terrain = Object.FindFirstObjectByType<Terrain>();
        if (terrain == null) { Debug.LogError("Terrain not found!"); return; }

        Texture2D diff = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/TerrainTexturesPackFree/TerrainTextures/GrassUV01.png");
        Texture2D norm = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/TerrainTexturesPackFree/TerrainTextures/GrassUV01_N.png");

        if (diff == null) { Debug.LogError("GrassUV01.png not found!"); return; }

        string layerPath = "Assets/_Project/Terrain/GrassLayer_New.terrainlayer";
        TerrainLayer layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(layerPath);
        if (layer == null)
        {
            layer = new TerrainLayer();
            layer.diffuseTexture = diff;
            layer.normalMapTexture = norm;
            layer.tileSize = new Vector2(8, 8);
            AssetDatabase.CreateAsset(layer, layerPath);
        }

        TerrainData td = terrain.terrainData;
        TerrainLayer[] existing = td.terrainLayers;

        // Check if already added
        foreach (var l in existing)
            if (l != null && l.diffuseTexture == diff) { Debug.Log("Grass layer already exists."); return; }

        TerrainLayer[] newLayers = new TerrainLayer[existing.Length + 1];
        existing.CopyTo(newLayers, 0);
        newLayers[existing.Length] = layer;
        td.terrainLayers = newLayers;

        // Paint grass over entire terrain
        int w = td.alphamapWidth;
        int h = td.alphamapHeight;
        int count = newLayers.Length;
        float[,,] maps = td.GetAlphamaps(0, 0, w, h);
        float[,,] newMaps = new float[h, w, count];

        int grassIdx = count - 1;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float grassWeight = 0.6f;
                float remaining = 1f - grassWeight;
                newMaps[y, x, grassIdx] = grassWeight;
                float oldTotal = 0f;
                for (int i = 0; i < count - 1; i++) oldTotal += maps[y, x, i];
                for (int i = 0; i < count - 1; i++)
                    newMaps[y, x, i] = oldTotal > 0 ? (maps[y, x, i] / oldTotal) * remaining : remaining / (count - 1);
            }

        td.SetAlphamaps(0, 0, newMaps);
        EditorUtility.SetDirty(terrain);
        AssetDatabase.SaveAssets();
        Debug.Log("[Grass] Done! Grass layer added to terrain.");
    }
}
