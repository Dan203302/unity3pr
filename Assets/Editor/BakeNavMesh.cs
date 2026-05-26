using UnityEngine;
using UnityEditor;
using Unity.AI.Navigation;

public class BakeNavMesh
{
    [MenuItem("Tools/Bake NavMesh")]
    public static void Bake()
    {
        var surface = Object.FindFirstObjectByType<NavMeshSurface>();
        if (surface == null) { UnityEngine.Debug.LogError("NavMeshSurface not found!"); return; }
        surface.BuildNavMesh();
        EditorUtility.SetDirty(surface);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEngine.Debug.Log("[NavMesh] Baked successfully!");
    }
}
