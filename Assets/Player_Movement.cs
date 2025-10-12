using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using NUnit.Framework;

using UnityEngine.UI;
public class Player_Movement : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXAndY = 0, // yaw + pitch
        MouseX = 1,     // yaw only
        MouseY = 2      // pitch only
    }




    public AudioSource player_audio_source;

    public AudioClip jump_audio;
    public AudioClip heal_audio;
    public AudioClip full_heal;
    public AudioClip hurter_audio;
    public AudioClip pick_up_gun;
    public AudioClip hit_audio;

    public AudioClip bullet_hit_auido;

    public AudioClip ragdoll_audio;
    public AudioClip ragdoll_recover_audio;
    public AudioClip dead_audio;
    public AudioClip sping_up_audio;


    public GameObject corpse;
    public GameObject alive;
    public GameObject ghost;

    public GameObject first_person_cam_ghost;
    public Transform first_person_cameraTransform_ghost;
    public GameObject third_person_cam_ghost;
    public Transform third_person_cameraTransform_ghost;
    public float move_speed;
    public GameObject player;

    public int Health;
    public int Full_Health;


    private List<GameObject> guns_in_interactable_radius = new List<GameObject>();

    public GameObject first_person_cam;
    public Transform first_person_cameraTransform;
    public GameObject third_person_cam;
    public Transform third_person_cameraTransform;

    public CharacterController controller;

    public GameObject death_screen;

    public GameObject hurt_overlay;

    public float jumpHeight;
    public float gravityValue;
    public Vector3 playerVelocity;

    public Slider health_slider;
    private bool groundedPlayer;

    public int current_jumps = 0;

    private GameObject currently_held_gun;


    public float sensitivityHor;
    public float sensitivityVert;

    public float minimumVert;
    public float maximumVert;

    public int consecutive_jumps_allowed;

    public float lerp_speed;

    public Transform grab_point;

    public float interactables_radius;
    public float environment_radius;

    public float scan_wait_time;

    private float next_scan = 0f;


    public float verticalRot;
    public float horizontalRot;

    public bool player_input_enabled = true;

    public bool isDead = false;


    private Quaternion targetYawRotation;
    private Quaternion targetPitchRotation;

    private bool isScoped = false;
    private Vector3 originalCameraPosition;
    public float scopeTransitionSpeed = 10f; // How fast camera moves to scope

    public bool is_first_person;


    void Start()
    {
        ghost.SetActive(true);
        death_screen.SetActive(false);
        Rigidbody rb = this.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        float normalizedX = (Input.mousePosition.x / Screen.width) - 0.5f;
        float normalizedY = (Input.mousePosition.y / Screen.height) - 0.5f;

        horizontalRot = normalizedX * 359f;
        verticalRot = -normalizedY * 90f;
        verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);

        targetYawRotation = Quaternion.Euler(0f, horizontalRot, 0f);
        targetPitchRotation = Quaternion.Euler(verticalRot, 0f, 0f);

        player.transform.localRotation = targetYawRotation;
        first_person_cameraTransform.localRotation = targetPitchRotation;
        third_person_cameraTransform.localRotation = targetPitchRotation;

        originalCameraPosition = first_person_cameraTransform.localPosition;

        corpse.SetActive(false);
        ghost.SetActive(false);

        // Cursor.lockState = CursorLockMode.Locked;
    }


    public void Loose_Health_Points(int damage_taken, float hurt_tick)
    {
        if (Health <= 0)
        {
            hurt_overlay.SetActive(false);
            Die();
        }
        else
        {
            Health = Health - damage_taken;
            hurt_overlay.SetActive(true);
            Invoke("Hide_Hurt_Overlay", hurt_tick / 2);
        }
        Debug.Log("TAKE DAMAGE" + damage_taken + " from " + Health);
        health_slider.value = Health;
    }

    public void Hide_Hurt_Overlay()
    {
        hurt_overlay.SetActive(false);
    }

    public void Die()
    {
        // corpse.SetActive(true);
        // player_audio_source.PlayOneShot(dead_audio, 1);
        death_screen.SetActive(true);
        isDead = true;
        Debug.Log("DIE");
        Ragdoll();
        // Rigidbody rb = this.GetComponent<Rigidbody>();
        // // this.gameObject.SetActive(false);
        // this.GetComponent<Renderer>().enabled = false;
        // player_input_enabled = false;
        // rb.isKinematic = false;
    }



    public void Ragdoll()
    {
        // player_audio_source.PlayOneShot(ragdoll_audio, 1);
        Debug.Log("Ragdoll started");
        Debug.Log(is_first_person);

        ghost.transform.position = player.transform.position;
        ghost.transform.rotation = player.transform.rotation;

        Ragdoll ragdollBehaviour = ghost.GetComponent<Ragdoll>();
        ragdollBehaviour.horizontalRot = this.horizontalRot;
        ragdollBehaviour.verticalRot = this.verticalRot;

        first_person_cam_ghost.SetActive(first_person_cam.activeSelf);
        third_person_cam_ghost.SetActive(third_person_cam.activeSelf);

        first_person_cam_ghost.transform.position = first_person_cam.transform.position;
        first_person_cam_ghost.transform.rotation = first_person_cam.transform.rotation;
        third_person_cam_ghost.transform.position = third_person_cam.transform.position;
        third_person_cam_ghost.transform.rotation = third_person_cam.transform.rotation;

        first_person_cam.SetActive(false);
        third_person_cam.SetActive(false);
        player_input_enabled = false;

        this.gameObject.SetActive(false);

        ghost.SetActive(true);
    }

    // public void Ragdoll_Recover()
    // {
    // }

    private Quaternion return_to_floor_rotation;
    private Vector3 return_to_floor_position;
    void Pick_Up_Gun(GameObject new_gun)
    {
        Quaternion new_gun_to_floor_rotation = new_gun.transform.rotation;
        Vector3 new_gun_to_floor_position = new_gun.transform.position;

        if (currently_held_gun != null)
        {
            currently_held_gun.transform.SetParent(null);
            // currently_held_gun.transform.position = return_to_floor_position;
            // currently_held_gun.transform.rotation = return_to_floor_rotation;
            currently_held_gun.transform.position = new_gun_to_floor_position;
            currently_held_gun.transform.rotation = new_gun_to_floor_rotation;
            currently_held_gun.GetComponent<Collider>().enabled = true;
            currently_held_gun = null;
        }


        player_audio_source.PlayOneShot(pick_up_gun, 1);
        // return_to_floor_position = new_gun.transform.position;
        // return_to_floor_rotation = new_gun.transform.rotation;

        return_to_floor_position = new_gun_to_floor_position;
        return_to_floor_rotation = new_gun_to_floor_rotation;


        guns_in_interactable_radius.RemoveAt(0);

        currently_held_gun = new_gun;
        currently_held_gun.GetComponent<Collider>().enabled = false;

        Transform gun_grab_point = currently_held_gun.transform.Find("Grab_Point");


        // Vector3 offset_world = currently_held_gun.transform.position - gun_grab_point.position;
        // Quaternion rotation_offset = Quaternion.Inverse(gun_grab_point.rotation) * currently_held_gun.transform.rotation;


        currently_held_gun.transform.SetParent(grab_point, true);


        currently_held_gun.transform.position = grab_point.position + grab_point.TransformDirection(gun_grab_point.localPosition * -1);
        currently_held_gun.transform.rotation = grab_point.rotation * Quaternion.Inverse(gun_grab_point.localRotation);

        guns_in_interactable_radius.Add(new_gun);
    }


    void Drop_Gun() { }

    void Fire_Gun() { }


    void Heal() { }


    void Detect_in_Radius()
    {
        List<GameObject> current_guns = new List<GameObject>();
        Collider[] interactable_colliders_in_radius = Physics.OverlapSphere(transform.position, interactables_radius);
        foreach (var interactable_collider in interactable_colliders_in_radius)
        {
            GameObject interactable_object_in_radius = interactable_collider.gameObject;
            if (interactable_object_in_radius.CompareTag("gun"))
            {
                // guns_in_interactable_radius.Add(guns_in_interactable_radius);
                current_guns.Add(interactable_object_in_radius);
                Debug.Log("gun detected within radius!" + interactable_object_in_radius.name);
                if (!guns_in_interactable_radius.Contains(interactable_object_in_radius))
                {
                    guns_in_interactable_radius.Add(interactable_object_in_radius);
                }
            }
        }

        Collider[] environment_colliders_in_radius = Physics.OverlapSphere(transform.position, environment_radius);
        foreach (var environment_collider in environment_colliders_in_radius)
        {
            GameObject environment_object_in_radius = environment_collider.gameObject;
            // if (environment_object_in_radius.CompareTag("hurter"))
            Hurter hurter = environment_object_in_radius.GetComponent<Hurter>();
            if (hurter != null && hurter.isActiveAndEnabled)
            {
                Hurter hurterScript = environment_object_in_radius.GetComponent<Hurter>();
                Debug.Log("PLAYER BEFORE HURT CALL");
                hurterScript.Hurt(this);
            }
            else if (environment_object_in_radius.CompareTag("wake_on_player"))
            {
                Rigidbody rb = this.GetComponent<Rigidbody>();
                // CharacterController cc = GetComponent<CharacterController>();
                // cc.enabled = false;
                // rb.isKinematic = false;
                // rb.constraints = RigidbodyConstraints.None;
                player_input_enabled = false;
                // rb.AddExplosionForce(200 * 50.0f, environment_object_in_radius.transform.position, 20.0f);
                Ragdoll();
                Rigidbody ghostRigid = ghost.GetComponent<Rigidbody>();
                ghostRigid.AddExplosionForce(350 * 35.0f, environment_object_in_radius.transform.position, 45.0f);

                Collider[] colliders = Physics.OverlapSphere(environment_object_in_radius.transform.position, 10.0f);

                foreach (Collider nearby in colliders)
                {
                    Rigidbody new_rb = nearby.attachedRigidbody;
                    if (rb != null)
                    {
                        // rb.AddExplosionForce(200 * 20.0f, environment_object_in_radius.transform.position, 25.0f);
                    }
                }
                // Vector3 hitDirection = (transform.position - environment_object_in_radius.transform.position).normalized;
                // rb.AddForce(hitDirection * 20f, ForceMode.Impulse);
                Debug.Log("yeet player");
            }
            else if (environment_object_in_radius.name == "Spring_Up")
            {
                Debug.Log("SPRING UP");
                float spring_force = 70.0f;
                float spring_mag = Mathf.Sqrt(spring_force * -2.0f * gravityValue);

                Vector3 spring_direction = environment_object_in_radius.transform.forward.normalized;
                playerVelocity = spring_direction * spring_mag;
                player_audio_source.PlayOneShot(sping_up_audio,0.8f);

            }
            else if (environment_object_in_radius.CompareTag("Heal"))
            {
                if (Health != Full_Health)
                {
                    if (Health + (Full_Health / 10) > Full_Health)
                    {
                        Health = Full_Health;
                        player_audio_source.PlayOneShot(full_heal, 0.5f);
                    }
                    else
                    {
                        Health = Health + (Full_Health / 10);
                        player_audio_source.PlayOneShot(heal_audio, 0.5f);
                    }

                    Debug.Log("10% point heal");

                    health_slider.value = Health;
                }



            }
            else if (environment_object_in_radius.CompareTag("Full_Heal"))
            {
                if (Health < Full_Health)
                {
                    Health = Full_Health;
                    Debug.Log("Full heal");
                    player_audio_source.PlayOneShot(full_heal, 0.75f);
                    health_slider.value = Health;
                }
            }
        }
        guns_in_interactable_radius.RemoveAll(gun => !current_guns.Contains(gun));
    }

    // Update is called once per frame
    void Update()
    {

        if (ghost.activeSelf)
        {

            // Ragdoll_State();
            return;

        }

        if (Time.time >= next_scan)
        {
            Detect_in_Radius();
            next_scan = Time.time + scan_wait_time;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivityHor;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityVert;

        if (((Input.mousePosition.x / Screen.width) - 0.5f) > 0.45f)
        {
            Debug.Log("Full RIGHT");
            // horizontalRot += Mathf.Sign(normalizedX) * sensitivityHor * Time.deltaTime * 100f;
        }

        if (((Input.mousePosition.x / Screen.width) - 0.5f) < -0.45f)
        {
            Debug.Log("Full left");

            // verticalRot -= Mathf.Sign(normalizedY) * sensitivityVert * Time.deltaTime * 100f;
        }

        horizontalRot += mouseX;
        verticalRot -= mouseY;
        verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);

        // Update target rotations
        targetYawRotation = Quaternion.Euler(0f, horizontalRot, 0f);
        targetPitchRotation = Quaternion.Euler(verticalRot, 0f, 0f);

        // smooth Yaw rotate player based on mouse input
        player.transform.localRotation = Quaternion.Slerp(
            player.transform.localRotation,
            targetYawRotation,
            1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
        );

        // smooth pitch rotate camera based on mouse input
        // When player is Yaw rotated, camera rotates with player
        // When camera is Pitch Rotated, only camera rotates


        first_person_cameraTransform.localRotation = Quaternion.Slerp(
            first_person_cameraTransform.localRotation,
            targetPitchRotation,
            1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
        );

        third_person_cameraTransform.localRotation = Quaternion.Slerp(
          third_person_cameraTransform.localRotation,
          targetPitchRotation,
          1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
      );

        Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;


        groundedPlayer = false;

        if (currently_held_gun != null)
        {
            currently_held_gun.transform.localRotation = first_person_cameraTransform.localRotation;
            currently_held_gun.transform.localRotation = Quaternion.Slerp(
            currently_held_gun.transform.localRotation,
            targetPitchRotation,
            1f - Mathf.Exp(-lerp_speed * Time.deltaTime)

        );
        }

        // gun rotation matches pitch of camera

        Vector3 move = Vector3.zero;
        // Vector3 first_gun_cycle_enter = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.G) && player_input_enabled)
        {

            // first_gun_cycle_enter = this.transform.position;
            if (guns_in_interactable_radius.Count > 0)
            {

                Pick_Up_Gun(guns_in_interactable_radius[0]);
            }
            // this.transform.position = first_gun_cycle_enter;
        }

        // if (controller.isGrounded || Mathf.Abs(playerVelocity.y) <= 0.01f)
        if (controller.isGrounded || Mathf.Abs(playerVelocity.y) <= 0.0001f)
        {
            groundedPlayer = true;
        }
        if (groundedPlayer)
        {
            current_jumps = 0;
            playerVelocity.y = -0.1f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && player_input_enabled)
        {
            if (current_jumps < consecutive_jumps_allowed)
            {
                player_audio_source.PlayOneShot(jump_audio, 1);
                current_jumps++;
                playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
            }
        }

        if (player_input_enabled)
        {
            if (!groundedPlayer)
            {
                playerVelocity.y += gravityValue * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.W))
            {
                move += player.transform.forward;
                Debug.Log("FORWARD");
            }
            else if (Input.GetKey(KeyCode.A))
            {
                move += -player.transform.right;
                Debug.Log("LEFT");
            }
            else if (Input.GetKey(KeyCode.S))
            {
                move += -player.transform.forward;
                Debug.Log("BACK");
            }
            else if (Input.GetKey(KeyCode.D))
            {
                move += player.transform.right;
                Debug.Log("RIGHT");
            }
        }

        if (Input.GetKeyUp(KeyCode.P))
        {
            if (first_person_cam.activeSelf == true)
            {

                is_first_person = true;
                first_person_cam.SetActive(false);
                third_person_cam.SetActive(true);
            }
            else
            {
                is_first_person = false;
                first_person_cam.SetActive(true);
                third_person_cam.SetActive(false);
            }
        }


        if (currently_held_gun != null && player_input_enabled)
        {
            ICommon_Gun_Actions gun_interface = currently_held_gun.GetComponent<ICommon_Gun_Actions>();

            if (Input.GetKey(KeyCode.LeftShift) && first_person_cam.activeSelf == true)
            {
                if (!isScoped)
                {
                    isScoped = true;
                    gun_interface.Scope_in();
                }

                Transform scopeTransform = gun_interface.Get_Scope();
                if (scopeTransform != null)
                {
                    first_person_cameraTransform.position = Vector3.Lerp(
                        first_person_cameraTransform.position,
                        scopeTransform.position,
                        scopeTransitionSpeed * Time.deltaTime
                    );

                }
            }
            else
            {
                if (isScoped)
                {
                    isScoped = false;
                    gun_interface.Scope_out();
                }

                first_person_cameraTransform.localPosition = Vector3.Lerp(
                    first_person_cameraTransform.localPosition,
                    originalCameraPosition,
                    scopeTransitionSpeed * Time.deltaTime
                );
            }

            if (Input.GetKey(KeyCode.V))
            {
                gun_interface.Fire();
            }
            if (Input.GetKeyUp(KeyCode.V))
            {
                gun_interface.Reload();
            }
        }



        if (player_input_enabled)
        {
            Vector3 finalMove = (move.normalized * move_speed) + new Vector3(0, playerVelocity.y, 0);
            controller.Move(finalMove * Time.deltaTime);
        }
        if (move != Vector3.zero)
        {

        }
    }


}
