using System.Linq.Expressions;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
//  Called when user ragdolls (Hits an obstacle, hit by an explosion, or killed)
public class Ragdoll : MonoBehaviour
{
    public GameObject first_person_cam_ghost; //ragdolls first person camera
    public GameObject third_person_cam_ghost;//ragdolls third person camera

    public float orbitDistance = 3.0f; //camera orbit distance

    private float sensitivityHor; //mouse sensitivity on horizontal plane (for camera control)
    private float sensitivityVert; //mouse sensitivity on vertical plane (for camera control)
    private float minimumVert; //minimum look-up angle of camera
    private float maximumVert; //maximum look-up angle of camera
    public float lerp_speed; //Lerp move animation speed

    public float max_sleep_time = 4.0f; //max ragdoll time before forcing recovery

    private bool isRecovering = false; //has ragdoll recovered? boolean
    private float recoveryStartTime; //timestamp of starting recovery
    public float recoveryDuration = 1.0f; //recovery speed

    public GameObject playerGameObject; //player game object

    public Transform default_player_stance; //default player stance (upright)
    public Transform default_first_person_stance; //defualt first person player camera position
    public Transform default_third_person_stance;//defualt third person player camera position



    private bool has_respawned; //has player respawned? boolean
    public float respawn_time; //time player remains dead before respawning

    private float time_of_death; //time user died

    public float verticalRot; //vertical rotation of camera
    public float horizontalRot; //horizontal rotation of camera
    private Rigidbody ragdoll_rigid; //Ragdolls Rigid Body
    private Player_Movement playerMovement; //Reference to player MonoBehaviour

    public AudioSource ragdoll_audio_source; //ragdolls audio source

    public AudioClip ragdoll_audio; //ragdoll started audio
    public AudioClip ragdoll_recover_audio; //ragdoll ended / recovered audio
    public AudioClip dead_audio; //ragdoll death audio

    private bool isPlayerAlive = true; //is player alive? boolean
    private bool hasDeathGongPlayed = false; //has death sound effect played? boolean

    private float ragdollStartTime; //start time of ragdoll

    void Awake() //called when Ghost (Players Ragdoll model and behaviour) is set to active
    {
        playerMovement = playerGameObject.GetComponent<Player_Movement>(); //get reference to active players behaviour

        sensitivityHor = playerMovement.sensitivityHor; //set ghost camera variables to match active players camera
        sensitivityVert = playerMovement.sensitivityVert; //set ghost camera variables to match active players camera
        minimumVert = playerMovement.minimumVert; //set ghost camera variables to match active players camera
        maximumVert = playerMovement.maximumVert; //set ghost camera variables to match active players camera
        ragdoll_rigid = this.GetComponent<Rigidbody>(); //ghost Rigid body
    }

    void Update()
    {

        if (!isPlayerAlive) //if player is not alive
        {
            if (!hasDeathGongPlayed) //if player is not alive, and death noise hasn't played
            {
                ragdoll_audio_source.PlayOneShot(dead_audio, 1); //play death noise
                hasDeathGongPlayed = true; //set true
                time_of_death = Time.time; //call time of death
            }

            if (Time.time - time_of_death >= respawn_time) //if player has been dead long enough to respawn
            {
                playerMovement.Respawn_Player(); //Respawn player through active player behaviour
            }
            Ghost_Camera_Operator(); //camera continues to be operated until player has recovered
        }
        else
        {
            if (isRecovering) //if ragdoll is in process of recovering
            {
                Ragdoll_Recover();

                if (Time.time - recoveryStartTime >= recoveryDuration) //if up-animation time has completed
                {
                    //Ragdoll is in upright / recovery position and can be disabled.
                    Return_to_alive(); // called to disable ragdoll, and enable active player
                }
            }
            else
            {
                Ghost_Camera_Operator(); //camera continues to be operated until player has recovered

                if (Time.time - ragdollStartTime > max_sleep_time && !has_respawned) //if player has been ragdolling longer than max, and is not dead
                {
                    has_respawned = true; //set true
                    Debug.Log("Ragdoll time limit exceeded - forcing recovery");
                    StartRecovery(); //force recovery
                }
            }
        }
    }


    void OnEnable()
    {
        if (playerMovement == null) //if player behaviour is not empty
        {
            playerMovement = playerGameObject.GetComponent<Player_Movement>();
        }
        ragdollStartTime = Time.time; //ragdoll start time is set
        default_player_stance = playerGameObject.transform; //active players position + rotation before ragdolling
        default_first_person_stance = playerMovement.first_person_cameraTransform; //active players 1st person camera transform before ragdolling
        default_third_person_stance = playerMovement.third_person_cameraTransform;//active players 3rd person camera transform before ragdolling
        isPlayerAlive = (playerMovement.Health > 0); //if player health is more than 0, player is alive
        hasDeathGongPlayed = false;
        has_respawned = false;

        if (isPlayerAlive)
        {
            Play_Ragdoll_Sound(); //if player is alive, play ragdoll start noise
        }
        else
        {
            ragdoll_rigid.AddForce(Vector3.up * 20f, ForceMode.Impulse); //if player is dead, add physics force to knock over ragdoll
        }


    }

    void Play_Ragdoll_Sound()
    {
        ragdoll_audio_source.PlayOneShot(ragdoll_audio, 0.8f); //play ragdoll start noise
    }

    void FixedUpdate() //FixedUpdate is called for Math updates / recalculations (?)
    {

        if (isPlayerAlive && ragdoll_rigid != null) //if player is alive
        {

            if (ragdoll_rigid.IsSleeping()) //if player is alive and rigid body is not moving
            {
                StartRecovery(); //start recovery

            }
            else
            {

            }
        }
    }


    void StartRecovery()
    {
        if (playerMovement.Health > 0) //if player is alive
        {
            isRecovering = true; //player is recovering
            recoveryStartTime = Time.time; //set recovery start time
            ragdoll_audio_source.PlayOneShot(ragdoll_recover_audio, 0.7f); //play recovery sound
        }
    }

    void Ragdoll_Recover() //called when ragdoll is recovering
    {
        playerGameObject.transform.position = this.gameObject.transform.position; //teleport active player model (disabled) to ragdolls current location

        transform.rotation = Quaternion.Slerp( //Lerp change ragdoll rotation to match active players rotation
            transform.rotation,
            default_player_stance.rotation,
            1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
        );

        first_person_cam_ghost.transform.rotation = Quaternion.Slerp( //Lerp change ragdoll 1st person camera rotation to match active players 3rd person camera rotation
        playerMovement.third_person_cameraTransform.transform.rotation,
        playerMovement.third_person_cameraTransform.rotation,
        1f - Mathf.Exp(-lerp_speed * Time.deltaTime));


        first_person_cam_ghost.transform.position = Vector3.Lerp( //Lerp change ragdoll 1st person camera position to match active players 3rd person camera position
        third_person_cam_ghost.transform.position,
        default_third_person_stance.position,
        1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
);


        third_person_cam_ghost.transform.rotation = Quaternion.Slerp( //Lerp change ragdoll 3rd person camera rotation to match active players 3rd person camera rotation
                    playerMovement.third_person_cameraTransform.transform.rotation,
                    playerMovement.third_person_cameraTransform.rotation,
                    1f - Mathf.Exp(-lerp_speed * Time.deltaTime));


        third_person_cam_ghost.transform.position = Vector3.Lerp( //Lerp change ragdoll 3rd person camera position to match active players 3rd person camera position
        third_person_cam_ghost.transform.position,
        default_third_person_stance.position,
        1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
);
    }

    void Return_to_alive() //called when ragdoll has recovered and ragdoll model is matching active player model
    {
        playerGameObject.SetActive(true); //re-enable active player model
        
        playerMovement.first_person_cam.SetActive(false); //disable active player model 1st person cam
        playerMovement.third_person_cam.SetActive(true); //enable active player model 3rd person cam
        playerMovement.player_input_enabled = true; //enable user input
        this.gameObject.SetActive(false); //disable ragdoll
        playerMovement.scan_immunity();
        isRecovering = false;
    }


    void Ghost_Camera_Operator() //controls camera 
    {
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityVert; //camera y orbit around player from vertical mouse input

        verticalRot -= mouseY;
        verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert); //vertical rotation of camera normalised

        Vector3 ragdollCenter = transform.position + Vector3.up * 1.0f; //center of ragdoll object

        Quaternion rotation = Quaternion.Euler(verticalRot, horizontalRot, 0f);
        Vector3 orbitOffset = rotation * Vector3.back * orbitDistance;


        if (first_person_cam_ghost.activeSelf) //if first person camera
        {

            first_person_cam_ghost.transform.position = Vector3.Lerp( //orbit around ragdoll model 
                first_person_cam_ghost.transform.position,
                ragdollCenter + orbitOffset,
                1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
            );


            first_person_cam_ghost.transform.rotation = Quaternion.Slerp( //orbit around ragdoll model 
                first_person_cam_ghost.transform.rotation,
                Quaternion.LookRotation(ragdollCenter - first_person_cam_ghost.transform.position),
                1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
            );
        }

        if (third_person_cam_ghost.activeSelf) //if third person camera 
        {
            third_person_cam_ghost.transform.position = Vector3.Lerp( //orbit around ragdoll model 
                third_person_cam_ghost.transform.position,
                ragdollCenter + orbitOffset,
                1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
            );

            third_person_cam_ghost.transform.rotation = Quaternion.Slerp( //orbit around ragdoll model 
                third_person_cam_ghost.transform.rotation,
                Quaternion.LookRotation(ragdollCenter - third_person_cam_ghost.transform.position),
                1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
            );
        }

        Cursor.lockState = CursorLockMode.Locked;
    }
}
