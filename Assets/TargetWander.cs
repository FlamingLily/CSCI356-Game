using UnityEngine;
using UnityEngine.AI;

public class TargetWander : MonoBehaviour
{

    public NavMeshAgent navAgent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                float angle = Random.Range(-110f, 110f);
                float distance = Random.Range(10f, 20f);

                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                Vector3 targetPosition = transform.position + direction.normalized * distance;

                navAgent.SetDestination(targetPosition);
            }
    }
}
