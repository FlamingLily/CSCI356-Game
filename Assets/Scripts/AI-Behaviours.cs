using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class AIBehaviour : MonoBehaviour
{


    [Header("Settings")]
    [SerializeField] private string moveState = "Wander";
    [SerializeField] private string aiType = "Demo";
    [SerializeField] private float health = 100f;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float approachRange = 3f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float attackCooldown = 0f;

    [Header("References")]
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private GameObject targetPlayer;

    NavMeshAgent navAgent;
    Renderer rdr;
    float nextAttack = 0f;
    float specialTimer = 0f;
    string cloaked = "false";


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        rdr = GetComponent<Renderer>();
        rdr.material.color = new Color(1f, 1f, 1f, 1f);
        nextAttack = Time.time;
        specialTimer = Time.time + 10f; // Matters for warpers, not really for cloakers unless you're camping hard
    }

    public void SetNavMesh(NavMeshSurface navMesh)
    {
        navMeshSurface = navMesh;
        navMesh.BuildNavMesh();
    }
    public void SetTarget(GameObject target)
    {
        targetPlayer = target;
    }
    public void SetType(string type)
    {
        aiType = type;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        switch(aiType)
        {
            case "Demo":
                moveSpeed = 3.5f;
                approachRange = 1f;
                DemoUpdate();
                break;
            case "Swarmer":
                moveSpeed = 5f;
                damage = 5f;
                attackCooldown = 1f;
                health = 25f;
                approachRange = 3f;
                attackRange = 3f;
                BasicUpdate();
                break;
            case "Melee":
                moveSpeed = 3.5f;
                damage = 10f;
                attackCooldown = 3f;
                health = 50f;
                approachRange = 3f;
                attackRange = 3f;
                BasicUpdate();
                break;
            case "Soldier":
                moveSpeed = 3.5f;
                damage = 10f;
                attackCooldown = 3f;
                health = 50f;
                attackRange = 20f;
                approachRange = 10f;
                BasicUpdate();
                break;
            case "Cloaker":
                moveSpeed = 3.5f;
                damage = 20f;
                attackCooldown = 3f;
                health = 100f;
                attackRange = 20f;
                approachRange = 10f;
                CloakerUpdate();
                break;
            case "Warper":
                moveSpeed = 2f;
                damage = 20f;
                attackCooldown = 3f;
                health = 50f;
                attackRange = 10f;
                approachRange = 10f;
                WarperUpdate();
                break;
            case "Tank":
                moveSpeed = 0.5f;
                damage = 30f;
                attackCooldown = 10f;
                health = 2000f;
                approachRange = 10f;
                attackRange = 30f;
                BasicUpdate();
                break;
            default:
                Debug.Log("bad aiType! default to demo");
                DemoUpdate();
                break;
        }
    }

    void DemoUpdate()
    {
        navAgent.speed = moveSpeed;
        if (Time.time > nextAttack)
        {
            if (moveState == "Wander")
            {
                moveState = "Approach";
            }
            else
            {
                moveState = "Wander";
            }
            nextAttack = Time.time + 30f;
        }
        switch (moveState)
        {
            case "Wander":
                Wander();
                break;
            case "Approach":
                Approach(approachRange);
                break;
            default:
                Debug.Log("bad movestate! default to wander");
                Wander();
                break;
        }      
    }

    void BasicUpdate()
    {
        navAgent.speed = moveSpeed;
        Approach(approachRange);
        HandleAttack();
    }

    void CloakerUpdate()
    {
        if (cloaked == "false" || cloaked == "used")
        {
            navAgent.speed = moveSpeed;
            Approach(approachRange);
            HandleAttack();
        }
        if (cloaked == "true")
        {
            Approach(approachRange);
        }
        if (health < 50f)
            {
                rdr.material.color = new Color(1f, 1f, 1f, 0.05f);
                cloaked = "true";
                navAgent.speed = 30f;
                specialTimer = Time.time + 10f;
            }
        if (cloaked == "true" && Time.time > specialTimer)
        {
            rdr.material.color = new Color(1f, 1f, 1f, 1f);
            cloaked = "used";
            health = 100f;
        }
    }

    void WarperUpdate()
    {
        navAgent.speed = moveSpeed;
        Approach(approachRange);
        HandleAttack();

        if(Vector3.Distance(transform.position, targetPlayer.transform.position) > 10f && Time.time > specialTimer)
        {
            Vector3 warpPosition = targetPlayer.transform.position + new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
            NavMeshHit hit;
            if (NavMesh.SamplePosition(warpPosition, out hit, 5.0f, NavMesh.AllAreas))
            {
                navAgent.Warp(hit.position);
                specialTimer = Time.time + 15f;
            }
        }
    }

    void Wander()
    {

        if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
            {
                float angle = Random.Range(-110f, 110f);
                float distance = Random.Range(2f, 5f);

                Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
                Vector3 targetPosition = transform.position + direction.normalized * distance;

                navAgent.SetDestination(targetPosition);
            }
    }

    void Approach(float range)
    {
        if (targetPlayer == null)
        {
            Debug.Log("no approach target! returning to wander");
            moveState = "Wander";
            return;
        }
        navAgent.stoppingDistance = range;
        navAgent.SetDestination(targetPlayer.transform.position);
    }

    void HandleAttack()
    {
        if (targetPlayer == null) return;

        float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.transform.position);
        if (distanceToTarget <= attackRange && Time.time >= nextAttack)
        {
            // Attack logic here
            Debug.Log($"{aiType} attacks for {damage} damage!");
            nextAttack = Time.time + attackCooldown;
        }
    }
}