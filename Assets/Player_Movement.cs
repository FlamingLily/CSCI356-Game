using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class Player_Movement : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXAndY = 0, // yaw + pitch
        MouseX = 1,     // yaw only
        MouseY = 2      // pitch only
    }

    //sensitivity 4
    //vert min -25, max 25 
    //lerp speed 2



    public float move_speed;
    public GameObject player;

    public int Health;

    private List<GameObject> guns_in_interactable_radius = new List<GameObject>();

    public GameObject first_person_cam;
    public Transform first_person_cameraTransform;
    public GameObject third_person_cam;
    public Transform third_person_cameraTransform;

    public CharacterController controller;

    public GameObject death_screen;

    public GameObject hurt_overlay;

    // public float jumpHeight = 20.0f;
    // public float gravityValue = -10.0f;
    // public float jumpHeight = 30f;
    // public float gravityValue = 20f;

    public float jumpHeight;
    public float gravityValue;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    private int current_jumps = 0;

    private GameObject currently_held_gun;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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


    private float verticalRot;
    private float horizontalRot;

    private bool player_input_enabled = true;


    private Quaternion targetYawRotation;
    private Quaternion targetPitchRotation;


    void Start()
    {

        death_screen.SetActive(false);
        Rigidbody rb = this.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        //if the cameraTransform doesnt exist
        // if (cameraTransform == null)
        //     cameraTransform = Camera.main.transform;

        //yaw rotation for rotating the player capsule
        targetYawRotation = transform.localRotation;

        //pitch for rotating only the camera
        targetPitchRotation = first_person_cameraTransform.localRotation;
    }


    public void Loose_Health_Points(int damage_taken)
    {
        if (Health <= 0)
        {
            Die();
        }
        else
        {
            Health = Health - damage_taken;
        }
        Debug.Log("TAKE DAMAGE" + damage_taken + " from " + Health);
    }

    public void Die()
    {
        death_screen.SetActive(true);
        Debug.Log("DIE");
        Rigidbody rb = this.GetComponent<Rigidbody>();
        player_input_enabled = false;
        rb.isKinematic = false;
    }

    private Quaternion return_to_floor_rotation;
    private Vector3 return_to_floor_position;
    void Pick_Up_Gun(GameObject new_gun)
    {
        if (currently_held_gun != null)
        {
            currently_held_gun.transform.SetParent(null);
            return_to_floor_position = new_gun.transform.position;
            return_to_floor_rotation = new_gun.transform.rotation;

            currently_held_gun.transform.position = return_to_floor_position;
            currently_held_gun.transform.rotation = return_to_floor_rotation;
            currently_held_gun.GetComponent<Collider>().enabled = true;
            currently_held_gun = null;
        }
        guns_in_interactable_radius.RemoveAt(0);

        currently_held_gun = new_gun;
        currently_held_gun.GetComponent<Collider>().enabled = false;

        currently_held_gun.transform.SetParent(grab_point, true);
        currently_held_gun.transform.position = grab_point.position;
        currently_held_gun.transform.rotation = grab_point.rotation;
        guns_in_interactable_radius.Add(new_gun);
    }

    void Drop_Gun() { }

    void Fire_Gun() { }

    // private void OnTriggerEnter(Collider other)
    // {
    //     // Detect_in_Radius();
    // }

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
            if (environment_object_in_radius.CompareTag("hurter"))
            {
                Hurter hurterScript = environment_object_in_radius.GetComponent<Hurter>();
                Debug.Log("PLAYER BEFORE HURT CALL");
                hurterScript.Hurt(this);
            }
            else if (environment_object_in_radius.CompareTag("wake_on_player"))
            {
                Rigidbody rb = this.GetComponent<Rigidbody>();
                CharacterController cc = GetComponent<CharacterController>();
                cc.enabled = false;
                rb.isKinematic = false;
                // rb.constraints = RigidbodyConstraints.None;
                player_input_enabled = false;
                // rb.AddExplosionForce(200 * 50.0f, environment_object_in_radius.transform.position, 20.0f);
                Collider[] colliders = Physics.OverlapSphere(environment_object_in_radius.transform.position, 10.0f);

                foreach (Collider nearby in colliders)
                {
                    Rigidbody new_rb = nearby.attachedRigidbody;
                    if (rb != null)
                    {
                        rb.AddExplosionForce(200 * 5.0f, environment_object_in_radius.transform.position, 25.0f);
                    }
                }
                // Vector3 hitDirection = (transform.position - environment_object_in_radius.transform.position).normalized;
                // rb.AddForce(hitDirection * 20f, ForceMode.Impulse);
                Debug.Log("yeet player");
            }
        }
        guns_in_interactable_radius.RemoveAll(gun => !current_guns.Contains(gun));
    }


    // Update is called once per frame
    void Update()
    {

        if (Time.time >= next_scan)
        {
            Detect_in_Radius();
            next_scan = Time.time + scan_wait_time;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivityHor;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityVert;

        horizontalRot += mouseX; //horizontal rotation is taken from mouses X
        verticalRot -= mouseY; // vertical rotation is taken from mouses Y

        // cannot rotate vertically more than limits
        verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);

        //yaw rotation for rotating the player capsule
        targetYawRotation = Quaternion.Euler(0f, horizontalRot, 0f);

        //pitch for rotating only the camera
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

            Pick_Up_Gun(guns_in_interactable_radius[0]);
            // this.transform.position = first_gun_cycle_enter;
        }

        if (controller.isGrounded || Mathf.Abs(playerVelocity.y) <= 0.01f)
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

                first_person_cam.SetActive(false);
                third_person_cam.SetActive(true);
            }
            else
            {
                first_person_cam.SetActive(true);
                third_person_cam.SetActive(false);
            }
        }

        if (Input.GetKey(KeyCode.M) && player_input_enabled)
        {
            ICommon_Gun_Actions gun_interface = currently_held_gun.GetComponent<ICommon_Gun_Actions>();
            gun_interface.Fire();
            // Debug.Log("FIRE");
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
