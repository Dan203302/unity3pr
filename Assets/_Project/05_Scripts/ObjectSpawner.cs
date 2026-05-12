using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] prefabs;
    public float spawnRangeX = 8f;
    public float spawnY = 15f;

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) Spawn(0);
        else if (kb.digit2Key.wasPressedThisFrame) Spawn(1);
        else if (kb.digit3Key.wasPressedThisFrame) Spawn(2);
        else if (kb.digit4Key.wasPressedThisFrame) Spawn(3);
        else if (kb.spaceKey.wasPressedThisFrame) Spawn(Random.Range(0, prefabs.Length));
    }

    void Spawn(int index)
    {
        if (prefabs == null || index >= prefabs.Length || prefabs[index] == null) return;
        Vector3 pos = new Vector3(Random.Range(-spawnRangeX, spawnRangeX), spawnY, Random.Range(-3f, 3f));
        Instantiate(prefabs[index], pos, Random.rotation);
    }
}
