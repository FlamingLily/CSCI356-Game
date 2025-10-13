using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using static UnityEngine.GameObject;

public class AIBehaviour : MonoBehaviour, I_TakeDamage
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
    [SerializeField] private GameObject targetPlayer;
    [SerializeField] private GameObject waveController;
    [SerializeField] private GameObject gunPrefab;
    [SerializeField] private GameObject stickPrefab;
    GameObject spawnedGun;
    GameObject spawnedStick;
    Transform grab_point;
    Transform hold_point;

    NavMeshAgent navAgent;
    Renderer rdr;
    float nextAttack = 0f;
    float specialTimer = 0f;
    string cloaked = "false";
    bool aiSetup = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetPlayer = FindGameObjectWithTag("Player");
        navAgent = GetComponent<NavMeshAgent>();
        rdr = GetComponent<Renderer>();
        rdr.material.color = new Color(0.55f, 0f, 0f, 1f);
        nextAttack = Time.time;
        specialTimer = Time.time + 10f; // Matters for warpers, not really for cloakers unless you're camping hard

    }

    void SpawnGun()
    {
        spawnedGun = Instantiate(gunPrefab, transform.position + new Vector3(0f, 1f, 0f), gunPrefab.transform.rotation);
        grab_point = spawnedGun.transform.Find("Grab_Point");
        hold_point = transform.Find("Hold_Point");
        spawnedGun.GetComponent<Collider>().enabled = false;
        spawnedGun.transform.SetParent(hold_point, true);
        spawnedGun.transform.position = hold_point.position + hold_point.TransformDirection(grab_point.localPosition * -1);
        spawnedGun.transform.rotation = hold_point.rotation * Quaternion.Inverse(grab_point.localRotation);

        spawnedGun.GetComponent<Enemy_revolver>().playerController = GetComponent<CharacterController>();
        spawnedGun.GetComponent<Enemy_revolver>().bullet_damage = damage;
        spawnedGun.GetComponent<Enemy_revolver>().gun_grab_point = grab_point;

    }

    void SpawnStick()
    {
        spawnedStick = Instantiate(stickPrefab, transform.position + new Vector3(0f, 1f, 0f), stickPrefab.transform.rotation);
        grab_point = spawnedStick.transform.Find("Grab_Point");
        hold_point = transform.Find("Hold_Point");
        spawnedStick.GetComponent<Collider>().enabled = false;
        spawnedStick.transform.SetParent(hold_point, true);
        spawnedStick.transform.position = hold_point.position + hold_point.TransformDirection(grab_point.localPosition * -1);
        spawnedStick.transform.rotation = hold_point.rotation * Quaternion.Inverse(grab_point.localRotation);
    }

    public void SetType(string type)
    {
        aiType = type;
    }
    public void SetWaveController(GameObject wc)
    {
        waveController = wc;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0f)
        {
            Destroy(gameObject);
            waveController.GetComponent<WaveController>().EnemyKilled();
            return;
        }
        if (!aiSetup)
        {
            SetupAI();
            aiSetup = true;
        }
        switch (aiType)
        {
            case "Cloaker":
                CloakerUpdate();
                break;
            case "Warper":
                WarperUpdate();
                break;
            default:
                BasicUpdate();
                break;
        }
    }

    void SetupAI()
    {
        switch (aiType)
        {
            case "Swarmer":
                moveSpeed = 5f;
                damage = 1f;
                attackCooldown = 1f;
                health = 25f;
                approachRange = 3f;
                attackRange = 3f;
                SpawnStick();
                break;
            case "Melee":
                moveSpeed = 3.5f;
                damage = 5f;
                attackCooldown = 3f;
                health = 50f;
                approachRange = 3f;
                attackRange = 3f;
                SpawnStick();
                break;
            case "Soldier":
                moveSpeed = 3.5f;
                damage = 10f;
                attackCooldown = 3f;
                health = 50f;
                attackRange = 20f;
                approachRange = 10f;
                SpawnGun();
                break;
            case "Cloaker":
                moveSpeed = 3.5f;
                damage = 20f;
                attackCooldown = 3f;
                health = 100f;
                attackRange = 20f;
                approachRange = 10f;
                SpawnGun();
                break;
            case "Warper":
                moveSpeed = 2f;
                damage = 20f;
                attackCooldown = 3f;
                health = 50f;
                attackRange = 10f;
                approachRange = 10f;
                SpawnGun();
                break;
            case "Tank":
                moveSpeed = 0.5f;
                damage = 30f;
                attackCooldown = 10f;
                health = 2000f;
                approachRange = 10f;
                attackRange = 30f;
                SpawnGun();
                break;
            default:
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
        if (health < 50f && cloaked == "false")
        {
            rdr.material.color = new Color(0.55f, 0f, 0f, 0.05f);
            spawnedGun.GetComponent<Enemy_revolver>().Cloak();
            cloaked = "true";
            navAgent.speed = 30f;
            specialTimer = Time.time + 10f;
        }
        if (cloaked == "true" && Time.time >= specialTimer)
        {
            rdr.material.color = new Color(0.55f, 0f, 0f, 1f);
            spawnedGun.GetComponent<Enemy_revolver>().UnCloak();
            cloaked = "used";
            health = 100f;
        }
        if (cloaked == "true")
        {
            if(Vector3.Distance(transform.position, targetPlayer.transform.position) > approachRange * 2)
            {
                Approach(approachRange);
            }
            else
            {
                Wander();
            }
        }
       
    }

    void WarperUpdate()
    {
        navAgent.speed = moveSpeed;
        Approach(approachRange);
        HandleAttack();

        if (Vector3.Distance(transform.position, targetPlayer.transform.position) > 10f && Time.time > specialTimer)
        {
            Vector3 warpPosition = targetPlayer.transform.position + new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));
            NavMeshHit hit;
            if (NavMesh.SamplePosition(warpPosition, out hit, 5.0f, NavMesh.AllAreas))
            {
                navAgent.Warp(hit.position);
                specialTimer = Time.time + 15f;
                nextAttack = Time.time + 5f;
            }
        }
    }

    void Wander()
    {
        navAgent.stoppingDistance = 0f;

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
            Wander();
            return;
        }
        navAgent.stoppingDistance = range;
        navAgent.SetDestination(targetPlayer.transform.position);
        if (Vector3.Distance(transform.position, targetPlayer.transform.position) <= range)
        {
            Vector3 directionToPlayer = (targetPlayer.transform.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToPlayer.x, 0, directionToPlayer.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);           
        }
    }

    void HandleAttack()
    {
        if (targetPlayer == null) return;
        if (cloaked == "True") return;

        
        if(spawnedGun == null && Vector3.Distance(transform.position, targetPlayer.transform.position) <= attackRange) // Melee
        {
            targetPlayer.GetComponent<I_TakeDamage>().TakeDamage(damage);
            Debug.Log($"{aiType} attacks for {damage} damage!");
            nextAttack = Time.time + attackCooldown;
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetPlayer.transform.position);
        if (distanceToTarget <= attackRange && Time.time >= nextAttack)
        {
            spawnedGun.GetComponent<Enemy_revolver>().Fire();
            Debug.Log($"{aiType} attacks for {damage} damage!");
            nextAttack = Time.time + attackCooldown;
        }
    }

    public void TakeDamage(float amount)
    {
        Debug.Log($"{aiType} takes {amount} damage!");
        health -= amount;
        Debug.Log("Health now: " + health);
        if (health <= 0f)
        {
            Destroy(gameObject);
            waveController.GetComponent<WaveController>().EnemyKilled();
        }
    }
}