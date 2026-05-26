using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class SetupNPC
{
    [MenuItem("Tools/Setup Customer NPC")]
    public static void Run()
    {
        // CashierPoint
        GameObject cashier = GameObject.Find("CashierPoint");
        if (cashier == null) cashier = new GameObject("CashierPoint");
        cashier.transform.position = new Vector3(-0.5f, 0.1f, -1.0f);

        // ExitPoint — far outside shop
        GameObject exit = GameObject.Find("ExitPoint");
        if (exit == null) exit = new GameObject("ExitPoint");
        exit.transform.position = new Vector3(10f, 0.1f, 10f);

        // CustomerNPC
        GameObject npc = GameObject.Find("CustomerNPC");
        if (npc == null) npc = new GameObject("CustomerNPC");
        npc.transform.position = new Vector3(5f, 0.1f, 5f);

        // NavMeshAgent
        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
        if (agent == null) agent = npc.AddComponent<NavMeshAgent>();
        agent.speed = 2.5f;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 1.5f;
        agent.radius = 0.4f;
        agent.height = 1.8f;

        // CustomerNPC script
        CustomerNPC script = npc.GetComponent<CustomerNPC>();
        if (script == null) script = npc.AddComponent<CustomerNPC>();
        script.cashierPoint = cashier.transform;
        script.exitPoint = exit.transform;

        EditorUtility.SetDirty(npc);
        EditorUtility.SetDirty(cashier);
        EditorUtility.SetDirty(exit);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        // Attach Base_Mesh if it's at root level (not inside Player)
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (var go in allObjects)
        {
            if (go.name == "Base_Mesh" && go.transform.parent == null)
            {
                go.transform.SetParent(npc.transform, false);
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
                EditorUtility.SetDirty(go);
                Debug.Log("[NPC] Base_Mesh attached to CustomerNPC.");
                break;
            }
        }

        Debug.Log("[NPC] CustomerNPC setup complete. CashierPoint: " +
                  cashier.transform.position + " ExitPoint: " + exit.transform.position);
    }
}
