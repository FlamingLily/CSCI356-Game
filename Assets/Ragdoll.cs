using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class Ragdoll : MonoBehaviour
{

    public GameObject first_person_cam_ghost;
    public GameObject third_person_cam_ghost;


    public float orbitDistance = 3.0f;
    public float orbitHeightOffset = 1.0f;
    private float sensitivityHor;
    private float sensitivityVert;
    private float minimumVert;
    private float maximumVert;
    public float lerp_speed;

    public float max_sleep_time = 10.0f;

    public float velocityThreshold = 0.1f;
    public float angularVelocityThreshold = 0.1f;
    public float recoveryHeight = 1.0f;

    private bool isRecovering = false;
    private float recoveryStartTime;
    public float recoveryDuration = 1.0f;

    public GameObject playerGameObject;
    private GameObject first_person_cam_player;
    private GameObject third_person_cam_player;
    private Transform first_person_cam_transform_player;
    private Transform third_person_cam_transform_player;


    public Transform default_player_stance;
    public Transform default_first_person_stance;
    public Transform default_third_person_stance;

    public float verticalRot;
    public float horizontalRot;
    private Rigidbody ragdoll_rigid;
    private Player_Movement playerMovement;

    public AudioSource ragdoll_audio_source;

    public AudioClip ragdoll_audio;
    public AudioClip ragdoll_recover_audio;
    public AudioClip dead_audio;

    private bool isPlayerAlive = true;
    private bool hasDeathGongPlayed = false;


    private float ragdollStartTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {



        Play_Ragdoll_Sound();
        playerMovement = playerGameObject.GetComponent<Player_Movement>();

        sensitivityHor = playerMovement.sensitivityHor;
        sensitivityVert = playerMovement.sensitivityVert;
        minimumVert = playerMovement.minimumVert;
        maximumVert = playerMovement.maximumVert;
        // lerp_speed = playerMovement.lerp_speed;

        ragdoll_rigid = this.GetComponent<Rigidbody>();


        // first_person_cam_player = playerMovement.first_person_cam;
        // first_person_cam_transform_player = playerMovement.first_person_cameraTransform;
        // third_person_cam_player = playerMovement.third_person_cam;
        // third_person_cam_transform_player = playerMovement.third_person_cameraTransform;


        // horizontalRot = playerMovement.horizontalRot;
        // verticalRot = playerMovement.verticalRot;

        // default_player_stance = playerMovement.transform;
        // default_first_person_stance = playerMovement.first_person_cameraTransform;
        // default_third_person_stance = playerMovement.third_person_cameraTransform;
    }

    void Update()
    {
        // Ghost_Camera_Operator();

        // if (Time.time - ragdollStartTime > max_sleep_time)
        // {
        //     Debug.Log("Ragdoll time limit exceeded - forcing recovery");
        //     Ragdoll_Recover();
        // }
        if (!isPlayerAlive && !hasDeathGongPlayed)
        {
            ragdoll_audio_source.PlayOneShot(dead_audio, 1);
            hasDeathGongPlayed = true;
        }
        if (isRecovering)
        {
            Ragdoll_Recover();

            if (Time.time - recoveryStartTime >= recoveryDuration)
            {
                Return_to_alive();
            }
        }
        else
        {
            Ghost_Camera_Operator();

            if (Time.time - ragdollStartTime > max_sleep_time)
            {
                Debug.Log("Ragdoll time limit exceeded - forcing recovery");
                StartRecovery();
            }
        }

    }

    void OnEnable()
    {
        ragdollStartTime = Time.time;
        default_player_stance = playerGameObject.transform;
        default_first_person_stance = playerMovement.first_person_cameraTransform;
        default_third_person_stance = playerMovement.third_person_cameraTransform;
        isPlayerAlive = playerMovement.isDead;
        Play_Ragdoll_Sound();


    }

    void Play_Ragdoll_Sound()
    {
        ragdoll_audio_source.PlayOneShot(ragdoll_audio, 0.8f);
    }

    void FixedUpdate()
    {

        if (ragdoll_rigid != null)
        {

            if (ragdoll_rigid.IsSleeping())
            {
                Debug.Log("Rigidbody is not moving.");
                StartRecovery();
                // Ragdoll_Recover();
            }
            else
            {
                Debug.Log("Rigidbody is moving.");

            }
        }
    }


    void StartRecovery()
    {
        if (playerMovement.Health > 0)
        {
            isRecovering = true;
            recoveryStartTime = Time.time;
            ragdoll_audio_source.PlayOneShot(ragdoll_recover_audio, 0.7f);


        }
        else
        {
            // ragdoll_audio_source.PlayOneShot(dead_audio, 1);
        }
    }

    void Ragdoll_Recover()
    {
        // ragdoll_audio_source.PlayOneShot(ragdoll_recover_audio, 0.3f);

        playerGameObject.transform.position = this.gameObject.transform.position;


        // transform.position = Vector3.Lerp(
        //     transform.position,
        //     default_player_stance.position,
        //     1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
        // );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            default_player_stance.rotation,
            1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
        );
        // if (playerMovement.is_first_person == true)
        // {
        first_person_cam_ghost.transform.rotation = Quaternion.Slerp(
        playerMovement.third_person_cameraTransform.transform.rotation,
        playerMovement.third_person_cameraTransform.rotation,
        1f - Mathf.Exp(-lerp_speed * Time.deltaTime));


        first_person_cam_ghost.transform.position = Vector3.Lerp(
        third_person_cam_ghost.transform.position,
        default_third_person_stance.position,
        1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
);


        third_person_cam_ghost.transform.rotation = Quaternion.Slerp(
                    playerMovement.third_person_cameraTransform.transform.rotation,
                    playerMovement.third_person_cameraTransform.rotation,
                    1f - Mathf.Exp(-lerp_speed * Time.deltaTime));


        third_person_cam_ghost.transform.position = Vector3.Lerp(
        third_person_cam_ghost.transform.position,
        default_third_person_stance.position,
        1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
);
        // }

        // first_person_cam_ghost.transform.position = Vector3.Lerp(
        //     first_person_cam_ghost.transform.position,
        //     default_first_person_stance.position,
        //     1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
        // );



        // third_person_cam_ghost.transform.position = Vector3.Lerp(
        //     third_person_cam_ghost.transform.position,
        //     default_third_person_stance.position,
        //     1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
        // );




        Debug.Log("Ragdoll Ended");
        // Return_to_alive();
    }

    void Return_to_alive()
    {
        Debug.Log(playerMovement.is_first_person);

        // playerGameObject.transform.position = this.gameObject.transform.position;
        playerGameObject.SetActive(true);
        // if (playerMovement.is_first_person == true)
        // {
        //     playerMovement.first_person_cam.SetActive(true);
        //     playerMovement.third_person_cam.SetActive(false);
        // }
        // else
        // {
        playerMovement.first_person_cam.SetActive(false);
        playerMovement.third_person_cam.SetActive(true);
        // }


        playerMovement.player_input_enabled = true;
        this.gameObject.SetActive(false);
        Debug.Log(playerMovement.is_first_person);
        isRecovering = false;
    }


    void Ghost_Camera_Operator()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivityHor;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityVert;



        verticalRot -= mouseY;
        verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);


        Vector3 ragdollCenter = transform.position + Vector3.up * 1.0f;



        Quaternion rotation = Quaternion.Euler(verticalRot, horizontalRot, 0f);
        Vector3 orbitOffset = rotation * Vector3.back * orbitDistance;


        if (first_person_cam_ghost.activeSelf)
        {

            first_person_cam_ghost.transform.position = Vector3.Lerp(
                first_person_cam_ghost.transform.position,
                ragdollCenter + orbitOffset,
                1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
            );


            first_person_cam_ghost.transform.rotation = Quaternion.Slerp(
                first_person_cam_ghost.transform.rotation,
                Quaternion.LookRotation(ragdollCenter - first_person_cam_ghost.transform.position),
                1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
            );
        }

        if (third_person_cam_ghost.activeSelf)
        {
            third_person_cam_ghost.transform.position = Vector3.Lerp(
                third_person_cam_ghost.transform.position,
                ragdollCenter + orbitOffset,
                1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
            );

            third_person_cam_ghost.transform.rotation = Quaternion.Slerp(
                third_person_cam_ghost.transform.rotation,
                Quaternion.LookRotation(ragdollCenter - third_person_cam_ghost.transform.position),
                1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
            );
        }

        Cursor.lockState = CursorLockMode.Locked;
    }
}
