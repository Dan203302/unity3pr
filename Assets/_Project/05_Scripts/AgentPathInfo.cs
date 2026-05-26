using UnityEngine;
using UnityEngine.AI;

public class AgentPathInfo : MonoBehaviour
{
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (agent.pathPending)
        {
            Debug.Log("Путь еще рассчитывается.");
            return;
        }

        Debug.Log("Оставшееся расстояние: " + agent.remainingDistance);

        if (agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Debug.Log("Путь построен полностью.");
        }
        else if (agent.pathStatus == NavMeshPathStatus.PathPartial)
        {
            Debug.Log("Путь построен частично.");
        }
        else if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            Debug.Log("Путь недействителен.");
        }
    }
}
