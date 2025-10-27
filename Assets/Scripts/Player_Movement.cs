using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

// using Unity.VisualScripting;
// using NUnit.Framework.Constraints;
// using System.Linq.Expressions;
// using NUnit.Framework;
// using UnityEngine.InputSystem;

public class Player_Movement : MonoBehaviour, I_TakeDamage //extends Damage Taking interface
{
    public enum RotationAxes //rotation axes for camera control
    {
        MouseXAndY = 0, // yaw + pitch
        MouseX = 1,     // yaw only
        MouseY = 2      // pitch only
    }

    //Camera Variables
    public float sensitivityHor; //mouse X sensitivity
    public float sensitivityVert; //mouse Y sensitivity
    public float minimumVert; //minimum camera rotation
    public float maximumVert; //maximum camera rotation
    public float verticalRot; //vertical rotation
    public float horizontalRot; //horizontal rotation
    private Quaternion targetYawRotation; //target camera yaw
    private Quaternion targetPitchRotation; //target camera pitch
    public bool is_first_person; //is player in first person mode?

    //Shop
    private GameObject current_shop = null; //shop player is currently in, if any
    private bool walls_up = false; //if walls of current shop, if any, are up
    public int item_price; //standard item price

    // Player Audio 
    public AudioSource player_audio_source; //player audio source
    public AudioClip jump_audio; //jumping audio
    public AudioClip heal_audio; //partial heal audio
    public AudioClip full_heal; //full heal audio
    public AudioClip hurter_audio; //on hurt audio
    public AudioClip pick_up_gun; //pick up gun audio
    public AudioClip hit_audio; //player hit audio
    public AudioClip bullet_hit_auido; //player hit by bullet audio
    public AudioClip ragdoll_audio; //ragdoll start audio
    public AudioClip ragdoll_recover_audio; //ragdoll recover audio
    public AudioClip dead_audio; //on death audio
    public AudioClip sping_up_audio; //jump on spring audio

    //UI
    public GameObject healthLabel; //enemies remaining label
    public GameObject death_screen; //death screen
    public GameObject hurt_overlay; //on hurt overlay
    public TextMeshProUGUI stahs_collected; //starts collected label
    public Slider health_slider; //player current health slider

    //Player States - Ragdoll
    public GameObject ghost; //dead state
    public GameObject first_person_cam_ghost; //ragdoll first person camera
    public Transform first_person_cameraTransform_ghost; //ragdoll first person camera transform
    public GameObject third_person_cam_ghost; //ragdoll third person camera
    public Transform third_person_cameraTransform_ghost; //ragdoll third person camera transform

    // Player States - Active
    public GameObject player; //active player
    public CharacterController controller; //active player character controller
    public GameObject first_person_cam; //active player first person camera
    public Transform first_person_cameraTransform; //active player first person camera transform
    public GameObject third_person_cam; //active player third person camera
    public Transform third_person_cameraTransform; //active player third person camera transform

    private bool immune = false;

    //Player Variables
    public float move_speed; //move speed now
    private float default_move_speed; //move speed on Start()
    public int Health; //player health now
    public int Full_Health; //maximum health / full health
    private int default_health; //full health value on Start()
    public float jumpHeight; //player jump height
    private float default_jump_height; //jump height on Start()
    public int consecutive_jumps_allowed; //consective jumps allowed
    private int consecutive_jumps_allowed_default; //consective jumps allowed on Start()
    public float gravityValue; //world gravity value
    public Vector3 playerVelocity; //current player velocity
    public int coins_held = 0; //currently held coins
    public int current_jumps = 0; //current consecutive jumps by player
    private bool groundedPlayer; //is player grounded?
    private GameObject currently_held_gun; //currently held gun, if any
    private GameObject currently_held_item;//currently held item, if any
    public float lerp_speed; //animation / lerp speed of player
    public Transform grab_point; //player's gun hold point
    public Transform item_grab_point; //player's item hold point
    public bool player_input_enabled = true; //is player currently controllable by user?
    public bool isDead = false; //is player alive?


    //Players Interactable Radius
    private List<GameObject> guns_in_interactable_radius = new List<GameObject>(); //guns in players interactable radius
    private List<GameObject> coins_picked = new List<GameObject>();
    private List<GameObject> shop_items_in_interactable_radius = new List<GameObject>(); //items in players interactable radius
    public float interactables_radius; //radius around player to detect interactables
    public float environment_radius;  //radius around player to detect enviromental factors (obstacles etc)
    public float scan_wait_time; //how often the player detects
    private float next_scan = 0f; //next detect time

    //World Variables
    private GameObject respawners_parent_holder; //parent GameObject of Respawn Beacons

    //Currently Held Gun Variables
    private bool isScoped = false; //is gun scoped in?
    private Vector3 originalCameraPosition; //un-scoped camera position
    public float scopeTransitionSpeed = 10f; // How fast camera moves to scope

    void Start()
    {
        stahs_collected.text = coins_held.ToString();
        default_health = Full_Health;
        Health = Full_Health;
        default_jump_height = jumpHeight;
        default_move_speed = move_speed;
        consecutive_jumps_allowed_default = consecutive_jumps_allowed;
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

        ghost.SetActive(false);

        respawners_parent_holder = GameObject.Find("Respawners");

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
        health_slider.value = Health;
    }

    public void Hide_Hurt_Overlay()
    {
        hurt_overlay.SetActive(false);
    }

    public void Die()
    {
        death_screen.SetActive(true);
        Health = 0;
        health_slider.value = Health;
        if (currently_held_gun != null)
        {
            currently_held_gun.transform.SetParent(null);
            currently_held_gun.transform.position = this.transform.position;
            currently_held_gun.transform.rotation = this.transform.rotation;
            currently_held_gun.GetComponent<Collider>().enabled = true;
            currently_held_gun = null;
        }
        consecutive_jumps_allowed = consecutive_jumps_allowed_default;

        isDead = true;
        Full_Health = default_health;
        health_slider.maxValue = Full_Health;
        jumpHeight = default_jump_height;
        move_speed = default_move_speed;

        Ragdoll();
    }



    public void Ragdoll()
    {
        ghost.transform.position = player.transform.position;
        ghost.transform.rotation = player.transform.rotation;

        Ragdoll ragdollBehaviour = ghost.GetComponent<Ragdoll>();
        ragdollBehaviour.horizontalRot = this.horizontalRot;
        ragdollBehaviour.verticalRot = this.verticalRot;
        third_person_cam_ghost.SetActive(true);

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
    private Quaternion return_to_floor_rotation;
    private Vector3 return_to_floor_position;
    void Pick_Up_Gun(GameObject new_gun)
    {
        Quaternion new_gun_to_floor_rotation = new_gun.transform.rotation;
        Vector3 new_gun_to_floor_position = new_gun.transform.position;

        if (currently_held_gun != null)
        {
            currently_held_gun.transform.SetParent(null);
            currently_held_gun.transform.position = new_gun_to_floor_position;
            currently_held_gun.transform.rotation = new_gun_to_floor_rotation;
            currently_held_gun.GetComponent<Collider>().enabled = true;
            currently_held_gun = null;
        }

        player_audio_source.PlayOneShot(pick_up_gun, 1);
        return_to_floor_position = new_gun_to_floor_position;
        return_to_floor_rotation = new_gun_to_floor_rotation;

        guns_in_interactable_radius.RemoveAt(0);

        currently_held_gun = new_gun;
        currently_held_gun.GetComponent<Collider>().enabled = false;

        Transform gun_grab_point = currently_held_gun.transform.Find("Grab_Point");

        currently_held_gun.transform.SetParent(grab_point, true);

        currently_held_gun.transform.position = grab_point.position + grab_point.TransformDirection(gun_grab_point.localPosition * -1);
        currently_held_gun.transform.rotation = grab_point.rotation * Quaternion.Inverse(gun_grab_point.localRotation);

        guns_in_interactable_radius.Add(new_gun);
    }

    private Quaternion return_item_to_floor_rotation;
    private Vector3 return_item_to_floor_position;

    void Pick_Up_Shop_Item(GameObject item)
    {
        Quaternion new_gun_to_floor_rotation = item.transform.rotation;
        Vector3 new_gun_to_floor_position = item.transform.position;

        if (currently_held_item != null)
        {
            currently_held_item.transform.SetParent(null);
            currently_held_item.transform.position = new_gun_to_floor_position;
            currently_held_item.transform.rotation = new_gun_to_floor_rotation;
            currently_held_item.GetComponent<Collider>().enabled = true;
            currently_held_item = null;
        }


        player_audio_source.PlayOneShot(pick_up_gun, 1);
        return_item_to_floor_position = new_gun_to_floor_position;
        return_item_to_floor_rotation = new_gun_to_floor_rotation;

        shop_items_in_interactable_radius.RemoveAt(0);

        currently_held_item = item;
        currently_held_item.GetComponent<Collider>().enabled = false;

        Transform gun_grab_point = currently_held_item.transform.Find("Grab_Point");

        currently_held_item.transform.SetParent(item_grab_point, true);

        currently_held_item.transform.position = item_grab_point.position + item_grab_point.TransformDirection(gun_grab_point.localPosition * -1);
        currently_held_item.transform.rotation = item_grab_point.rotation * Quaternion.Inverse(gun_grab_point.localRotation);

        shop_items_in_interactable_radius.Add(item);
    }


    void Drop_Gun() { }

    void Fire_Gun() { }


    void Heal() { }

    public void Respawn_Player()
    {

        int respawners_in_scene = respawners_parent_holder.transform.childCount;
        int random_respawn_index = Random.Range(0, respawners_in_scene);
        Transform chosen_respawn_anchor = respawners_parent_holder.transform.GetChild(random_respawn_index);

        this.gameObject.transform.position = chosen_respawn_anchor.transform.position;
        this.gameObject.SetActive(true);
        this.first_person_cam.SetActive(false);
        this.third_person_cam.SetActive(true);

        this.player_input_enabled = true;
        ghost.SetActive(false);

        death_screen.SetActive(false);
        Health = Full_Health;
        health_slider.value = Health;

        isDead = false;
        player_audio_source.PlayOneShot(full_heal, 0.8f);
    }
    public void back_to_normal_scan()
    {
        // scan_wait_time = 0.2f;
        immune = false;
    }

    public void scan_immunity()
    {
        // scan_wait_time = 6.0f;
        immune = true;
        Invoke("back_to_normal_scan", 3.0f);
    }

   
    // void OnEnable()
    // {
    //     Debug.Log("Immunity On Enable in Player");
    //     scan_immunity();
    //     Invoke("back_to_normal_scan()", 5.0f);
    // }



    private Quaternion return_to_floor_rotation_item;
    private Vector3 return_to_floor_position_item;
    void Checkout_Held_Item()
    {

        if (currently_held_item != null)
        {
            if (coins_held >= item_price)
            {
                player_audio_source.PlayOneShot(full_heal, 0.8f);
                if (currently_held_item.CompareTag("gun"))
                {
                    Quaternion new_gun_to_floor_rotation = currently_held_item.transform.rotation;
                    Vector3 new_gun_to_floor_position = currently_held_item.transform.position;

                    if (currently_held_gun != null)
                    {
                        currently_held_gun.transform.SetParent(null);
                        currently_held_gun.transform.position = return_to_floor_position_item;
                        currently_held_gun.transform.rotation = return_to_floor_rotation_item;
                        currently_held_gun.GetComponent<Collider>().enabled = true;
                        currently_held_gun = null;
                    }

                    currently_held_gun = currently_held_item;
                    currently_held_item.transform.SetParent(null);
                    currently_held_item = null;


                    Transform gun_grab_point = currently_held_gun.transform.Find("Grab_Point");
                    currently_held_gun.transform.SetParent(grab_point, true);
                    currently_held_gun.transform.position = grab_point.position + grab_point.TransformDirection(gun_grab_point.localPosition * -1);
                    currently_held_gun.transform.rotation = grab_point.rotation * Quaternion.Inverse(gun_grab_point.localRotation);
                }
                else if (currently_held_item.CompareTag("Shop_Item"))
                {
                    if (currently_held_item.name == "speed_boost")
                    {
                        move_speed += 0.5f;
                    }
                    else if (currently_held_item.name == "jump_boost")
                    {
                        jumpHeight += 0.5f;
                        consecutive_jumps_allowed += 1;
                    }
                    else if (currently_held_item.name == "health_boost")
                    {
                        Full_Health += 10;
                        Health = Full_Health;
                        health_slider.maxValue = Health;
                        health_slider.value = Health;
                    }
                }
                player_audio_source.PlayOneShot(full_heal, 0.8f);
                coins_held = coins_held -= item_price;
                stahs_collected.text = coins_held.ToString();

            }
            else
            {
                player_audio_source.PlayOneShot(ragdoll_audio, 0.8f);
            }
            currently_held_item.transform.SetParent(null);
            currently_held_item.transform.position = return_to_floor_position_item;
            currently_held_item.transform.rotation = return_to_floor_rotation_item;;
            currently_held_item.transform.localRotation = return_to_floor_rotation_item;;
            currently_held_item = null;
        }

    }

    void Detect_in_Radius()
    {
        bool found_shop = false;
        List<GameObject> current_guns = new List<GameObject>();
        List<GameObject> current_items = new List<GameObject>();
        Collider[] interactable_colliders_in_radius = Physics.OverlapSphere(transform.position, interactables_radius);
        foreach (var interactable_collider in interactable_colliders_in_radius)
        {
            GameObject interactable_object_in_radius = interactable_collider.gameObject;
            if (walls_up == false)
            {
                if (interactable_object_in_radius.CompareTag("gun"))
                {
                    current_guns.Add(interactable_object_in_radius);
                    if (!guns_in_interactable_radius.Contains(interactable_object_in_radius))
                    {
                        guns_in_interactable_radius.Add(interactable_object_in_radius);
                    }
                }
            }
            else
            {
                if (interactable_object_in_radius.CompareTag("gun") || interactable_object_in_radius.CompareTag("Shop_Item"))
                {
                    current_items.Add(interactable_object_in_radius);
                    if (!shop_items_in_interactable_radius.Contains(interactable_object_in_radius))
                    {
                        shop_items_in_interactable_radius.Add(interactable_object_in_radius);
                    }
                }
            }
        }

        Collider[] environment_colliders_in_radius = Physics.OverlapSphere(transform.position, environment_radius);
        foreach (var environment_collider in environment_colliders_in_radius)
        {
            GameObject environment_object_in_radius = environment_collider.gameObject;
            Hurter hurter = environment_object_in_radius.GetComponent<Hurter>();
            if (hurter != null && hurter.isActiveAndEnabled)
            {
                Hurter hurterScript = environment_object_in_radius.GetComponent<Hurter>();
                hurterScript.Hurt(this);
            }
            else if (environment_object_in_radius.CompareTag("wake_on_player") && immune == false)
            {
                Rigidbody rb = this.GetComponent<Rigidbody>();
                player_input_enabled = false;
                Ragdoll();
                Rigidbody ghostRigid = ghost.GetComponent<Rigidbody>();
                ghostRigid.AddExplosionForce(500 * 25.0f, environment_object_in_radius.transform.position, 25.0f);
                // ghostRigid.AddExplosionForce(350 * 35.0f, environment_object_in_radius.transform.position, 45.0f);
            }
            else if (environment_object_in_radius.name == "Spring_Up")
            {
                float spring_force = 70.0f;
                float spring_mag = Mathf.Sqrt(spring_force * -2.0f * gravityValue);

                Vector3 spring_direction = environment_object_in_radius.transform.forward.normalized;
                playerVelocity = spring_direction * spring_mag;
                player_audio_source.PlayOneShot(sping_up_audio, 0.8f);
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
                    health_slider.value = Health;
                }
            }
            else if (environment_object_in_radius.CompareTag("Full_Heal"))
            {
                if (Health < Full_Health)
                {
                    Health = Full_Health;
                    player_audio_source.PlayOneShot(full_heal, 0.75f);
                    health_slider.value = Health;
                }
            }
            else if (environment_object_in_radius.CompareTag("Coin"))
            {
                coins_held += 1;
                coins_picked.Insert(0, environment_object_in_radius);
                environment_object_in_radius.SetActive(false);
                
                AudioSource.PlayClipAtPoint(full_heal, environment_object_in_radius.transform.position, 1.0f);
                stahs_collected.text = coins_held.ToString();
                Invoke("reactive_coin", Random.Range(1.0f, 15.0f));
            }
            else if (environment_object_in_radius.CompareTag("Shop"))
            {
                found_shop = true;


                if (current_shop != environment_object_in_radius)
                {
                    current_shop = environment_object_in_radius;
                    Wall_Up(environment_object_in_radius);

                }
            }
        }
        guns_in_interactable_radius.RemoveAll(gun => !current_guns.Contains(gun));
        shop_items_in_interactable_radius.RemoveAll(item => !current_items.Contains(item));
        if (!found_shop && current_shop != null)
        {
            Checkout_Held_Item();
            Wall_Down(current_shop);
            current_shop = null;
        }
    }

    void reactive_coin()
    {
        coins_picked[0].SetActive(true);
        coins_picked.RemoveAt(0);
        
    }
    void Wall_Up(GameObject shop_parent)
    {
        int walls_count = shop_parent.transform.childCount;
        if (walls_up == false)
        {
            foreach (Transform child_wall in shop_parent.transform)
            {

                if (child_wall.name == "Wall")
                {
                    Vector3 downpos = child_wall.transform.position;
                    downpos.y += 7.0f;
                    child_wall.transform.position = downpos;
                    walls_up = true;
                }
            }
        }

    }

    void Wall_Down(GameObject shop_parent)
    {
        int walls_count = shop_parent.transform.childCount;
        if (walls_up == true)
        {
            foreach (Transform child_wall in shop_parent.transform)
            {
                if (child_wall.name == "Wall")
                {
                    Vector3 downpos = child_wall.transform.position;
                    downpos.y -= 7.0f;
                    child_wall.transform.position = downpos;
                    walls_up = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (ghost.activeSelf)
        {
            return;
        }

        if (Time.time >= next_scan)
        {
            Detect_in_Radius();
            next_scan = Time.time + scan_wait_time;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivityHor;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivityVert;

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
        if (Input.GetKeyDown(KeyCode.G) && player_input_enabled)
        {
            if (walls_up == false)
            {
                if (guns_in_interactable_radius.Count > 0)
                {

                    Pick_Up_Gun(guns_in_interactable_radius[0]);
                }
            }
            else
            {
                if (shop_items_in_interactable_radius.Count > 0)
                {
                    return_to_floor_rotation_item = shop_items_in_interactable_radius[0].transform.rotation;
                    return_to_floor_position_item = shop_items_in_interactable_radius[0].transform.position;
                    Pick_Up_Shop_Item(shop_items_in_interactable_radius[0]);
                }
            }
        }
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
            }
            else if (Input.GetKey(KeyCode.A))
            {
                move += -player.transform.right;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                move += -player.transform.forward;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                move += player.transform.right;
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

            if (Input.GetKey(KeyCode.V) || Input.GetMouseButton(0))
            {
                gun_interface.Fire();
            }
            if (Input.GetKeyUp(KeyCode.V) || Input.GetMouseButtonUp(0))
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

    public void TakeDamage(float damage)
    {
        int inted_damage = (int)damage;
        Loose_Health_Points(inted_damage, 0.2f);
    }

}
