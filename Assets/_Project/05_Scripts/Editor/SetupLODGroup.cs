using UnityEngine;
using UnityEditor;

public class SetupLODGroup : MonoBehaviour
{
    [MenuItem("Tools/Setup LOD Group on Shelf")]
    static void AddLODToShelf()
    {
        // Find the first Cafe_Shelf_1 in scene
        GameObject shelf = GameObject.Find("Cafe_Shelf_1");
        if (shelf == null)
        {
            Debug.LogError("Cafe_Shelf_1 not found in scene!");
            return;
        }

        // Add LOD Group if not present
        LODGroup lodGroup = shelf.GetComponent<LODGroup>();
        if (lodGroup == null)
            lodGroup = shelf.AddComponent<LODGroup>();

        // Get all renderers in the shelf
        Renderer[] renderers = shelf.GetComponentsInChildren<Renderer>();

        // Setup 2 LOD levels
        LOD[] lods = new LOD[2];

        // LOD0 - full detail (visible from 0% to 60% of screen height)
        lods[0] = new LOD(0.6f, renderers);

        // LOD1 - same renderers but lower screen percentage threshold
        lods[1] = new LOD(0.1f, renderers);

        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();

        EditorUtility.SetDirty(shelf);
        Debug.Log("LOD Group successfully added to " + shelf.name);
    }
}
