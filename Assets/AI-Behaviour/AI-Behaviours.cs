using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{


    [Header("Settings")]
    [SerializeField] private string moveState = "Wander";

    [Header("References")]
    [SerializeField] private Camera demoCamera;
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private GameObject approachTarget;

    NavMeshAgent navAgent;
    float demoTypeChangeTime = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        demoTypeChangeTime = Time.time + UnityEngine.Random.Range(15f, 30f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > demoTypeChangeTime)
        {
            if (moveState == "Wander")
            {
                moveState = "Approach";               
            }
            else
            {
                moveState = "Wander";
            }
            demoTypeChangeTime = Time.time + 30f;
        }
        switch (moveState)
        {
            case "Wander":
                Wander();
                break;
            case "Approach":
                Approach();
                break;
            default:
                Debug.Log("bad movestate! default to wander");
                Wander();
                break;
        }
    }

    void Wander()
    {
        navAgent.speed = 3.5f;

        if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                float angle = UnityEngine.Random.Range(-110f, 110f);
                float distance = UnityEngine.Random.Range(2f, 5f);

                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                Vector3 targetPosition = transform.position + direction.normalized * distance;

                navAgent.SetDestination(targetPosition);
            }
    }

    void Approach()
    {
        navAgent.speed = 5f;
        if (approachTarget == null)
        {
            Debug.Log("no approach target! returning to wander");
            moveState = "Wander";
            return;
        }
        navAgent.SetDestination(approachTarget.transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == approachTarget)
        {
            moveState = "Wander";
        }
    }
}