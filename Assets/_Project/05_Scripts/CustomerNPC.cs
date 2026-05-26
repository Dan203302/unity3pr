using UnityEngine;
using UnityEngine.AI;

public class CustomerNPC : MonoBehaviour
{
    public enum State { WalkingToCashier, WaitingAtCashier, TakingBox, WalkingToExit, Done }

    [Header("Waypoints")]
    public Transform cashierPoint;
    public Transform exitPoint;
    [Tooltip("Точка спавна при возврате. Если не задана — используется стартовая позиция")]
    public Transform spawnPoint;

    [Header("Settings")]
    public float stoppingDistance = 1.5f;
    public float waitMessageInterval = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private State currentState = State.WalkingToCashier;
    private GameObject heldBox;
    private float nextMessageTime;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private float exitTimeout;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        if (cashierPoint != null)
        {
            agent.stoppingDistance = stoppingDistance;
            agent.SetDestination(cashierPoint.position);
            Debug.Log("[NPC] Идёт к кассе...");
        }
        else
        {
            Debug.LogWarning("[NPC] cashierPoint не назначен!");
        }
    }

    void Update()
    {
        UpdateAnimator();

        switch (currentState)
        {
            case State.WalkingToCashier:
                if (!agent.pathPending && agent.remainingDistance <= stoppingDistance + 0.1f)
                {
                    agent.isStopped = true;
                    currentState = State.WaitingAtCashier;
                    Debug.Log("[NPC] Ждёт у кассы. Нажмите E для выдачи коробки.");
                }
                break;

            case State.WaitingAtCashier:
                if (cashierPoint != null)
                    transform.LookAt(new Vector3(cashierPoint.position.x, transform.position.y, cashierPoint.position.z));

                if (Time.time >= nextMessageTime)
                {
                    Debug.Log("[NPC] Ожидает получения товара... [E] — выдать коробку");
                    nextMessageTime = Time.time + waitMessageInterval;
                }
                break;

            case State.TakingBox:
                // Handled in ReceiveBox()
                break;

            case State.WalkingToExit:
                exitTimeout += Time.deltaTime;
                if ((!agent.pathPending && agent.remainingDistance <= 0.5f) || exitTimeout > 8f)
                {
                    currentState = State.Done;
                    Debug.Log("[NPC] Покупатель ушёл. Вернётся через 5 сек.");
                    StartCoroutine(RespawnAfterDelay(5f));
                }
                break;
        }
    }

    public void ReceiveBox(GameObject box)
    {
        if (currentState != State.WaitingAtCashier) return;

        heldBox = box;
        currentState = State.TakingBox;
        Debug.Log("[NPC] Получил коробку!");

        // Attach box to NPC's right hand
        if (heldBox != null)
        {
            Rigidbody rb = heldBox.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Transform rightHand = FindBone("RightHand");
            if (rightHand != null)
            {
                heldBox.transform.SetParent(rightHand, false);
                heldBox.transform.localPosition = new Vector3(0, 0.1f, 0);
                heldBox.transform.localRotation = Quaternion.identity;
            }
            else
            {
                heldBox.transform.SetParent(transform, false);
                heldBox.transform.localPosition = new Vector3(0.4f, 1.2f, 0.3f);
            }
        }

        // Walk to exit
        if (exitPoint != null)
        {
            agent.isStopped = false;
            agent.SetDestination(exitPoint.position);
            currentState = State.WalkingToExit;
            Debug.Log("[NPC] Идёт к выходу...");
        }
        else
        {
            Debug.LogWarning("[NPC] exitPoint не назначен!");
            currentState = State.Done;
            gameObject.SetActive(false);
        }
    }

    public bool IsWaitingAtCashier => currentState == State.WaitingAtCashier;

    private System.Collections.IEnumerator RespawnAfterDelay(float delay)
    {
        Debug.Log($"[NPC] Таймаут {delay}с начался — ждём нового покупателя...");
        yield return new WaitForSeconds(delay);
        Debug.Log("[NPC] Таймаут закончился — создаём нового покупателя!");

        // Позиция спавна
        Vector3 pos = spawnPoint != null ? spawnPoint.position : spawnPosition;
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : spawnRotation;

        // Создаём свежую копию себя
        GameObject newNpcGo = Instantiate(gameObject, pos, rot);
        CustomerNPC newNpc = newNpcGo.GetComponent<CustomerNPC>();
        newNpc.cashierPoint = cashierPoint;
        newNpc.exitPoint    = exitPoint;
        newNpc.spawnPoint   = spawnPoint;

        Debug.Log("[NPC] Новый покупатель появился!");
        Destroy(gameObject);
    }

    Transform FindBone(string boneName)
    {
        foreach (Transform t in GetComponentsInChildren<Transform>())
            if (t.name == boneName) return t;
        return null;
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        float speed = agent.velocity.magnitude;

        // Try common parameter names from Character_Movement controller
        TrySetFloat("Speed", speed);
        TrySetFloat("Velocity", speed);
        TrySetBool("IsMoving", speed > 0.1f);
        TrySetBool("IsWalking", speed > 0.1f);
    }

    void TrySetFloat(string name, float value)
    {
        foreach (var p in animator.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Float)
            { animator.SetFloat(name, value); return; }
    }

    void TrySetBool(string name, bool value)
    {
        foreach (var p in animator.parameters)
            if (p.name == name && p.type == AnimatorControllerParameterType.Bool)
            { animator.SetBool(name, value); return; }
    }
}
