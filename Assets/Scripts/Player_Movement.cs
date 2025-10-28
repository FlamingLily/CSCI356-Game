using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
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
    public TextMeshProUGUI stahs_collected; //stars collected label
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
    private bool immune = false; //short immunity from ragdolling obstacles after recovring from ragdoll

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
    private List<GameObject> coins_picked = new List<GameObject>(); //priority queue of picked up coins, for respawning coins after collection
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
        //set initial player values
        stahs_collected.text = coins_held.ToString();
        default_health = Full_Health;
        Health = Full_Health;
        default_jump_height = jumpHeight;
        default_move_speed = move_speed;
        consecutive_jumps_allowed_default = consecutive_jumps_allowed;

        //set ghost elements as inactive
        ghost.SetActive(true); //ghost is initially set to active so that Awake() fires on it, setting variables
        death_screen.SetActive(false);
        Rigidbody rb = this.GetComponent<Rigidbody>();
        rb.isKinematic = true;

        //camera values from mouse input 
        float normalizedX = (Input.mousePosition.x / Screen.width) - 0.5f;
        float normalizedY = (Input.mousePosition.y / Screen.height) - 0.5f;
        horizontalRot = normalizedX * 359f; //horizontal rotation from mouse input
        verticalRot = -normalizedY * 90f; //vertical rotation from mouse input
        verticalRot = Mathf.Clamp(verticalRot, minimumVert, maximumVert);
        targetYawRotation = Quaternion.Euler(0f, horizontalRot, 0f);
        targetPitchRotation = Quaternion.Euler(verticalRot, 0f, 0f);

        //player and camera movements from player input
        player.transform.localRotation = targetYawRotation;
        first_person_cameraTransform.localRotation = targetPitchRotation;
        third_person_cameraTransform.localRotation = targetPitchRotation;
        originalCameraPosition = first_person_cameraTransform.localPosition;

        ghost.SetActive(false); //set ghost back to false

        respawners_parent_holder = GameObject.Find("Respawners");

    }


    public void Loose_Health_Points(int damage_taken, float hurt_tick) //called on Player taking Damage
    {
        if (Health <= 0)
        {
            hurt_overlay.SetActive(false);
            Die(); //Kill player is health is 0 or less
        }
        else
        {
            Health = Health - damage_taken;
            hurt_overlay.SetActive(true); //enable hurt overlay
            Invoke("Hide_Hurt_Overlay", hurt_tick / 2);
        }
        health_slider.value = Health; //represent current user health in health slider
    }

    public void Hide_Hurt_Overlay()
    {
        hurt_overlay.SetActive(false);
    }

    public void Die() //called when players health is 0 or less
    {
        death_screen.SetActive(true);
        Health = 0;

        if (currently_held_gun != null) //drop currently held gun, if one is held
        {
            currently_held_gun.transform.SetParent(null);
            currently_held_gun.transform.position = this.transform.position;
            currently_held_gun.transform.rotation = this.transform.rotation;
            currently_held_gun.GetComponent<Collider>().enabled = true;
            currently_held_gun = null;
        }

        isDead = true;

        //set player values back to default values (loose effects from bought items)
        health_slider.value = Health;
        consecutive_jumps_allowed = consecutive_jumps_allowed_default;
        Full_Health = default_health;
        health_slider.maxValue = Full_Health;
        jumpHeight = default_jump_height;
        move_speed = default_move_speed;

        Ragdoll();
    }



    public void Ragdoll() //called when the user Ragdolls (on death or on collision with certain obstacles)
    {
        ghost.transform.position = player.transform.position; //match ghost transforms with user transforms for seamless transition between states
        ghost.transform.rotation = player.transform.rotation;

        Ragdoll ragdollBehaviour = ghost.GetComponent<Ragdoll>();
        ragdollBehaviour.horizontalRot = this.horizontalRot; //match mouse input rotations with active player
        ragdollBehaviour.verticalRot = this.verticalRot;
        third_person_cam_ghost.SetActive(true);

        first_person_cam_ghost.transform.position = first_person_cam.transform.position; //match camera position with active player
        first_person_cam_ghost.transform.rotation = first_person_cam.transform.rotation;
        third_person_cam_ghost.transform.position = third_person_cam.transform.position;
        third_person_cam_ghost.transform.rotation = third_person_cam.transform.rotation;

        first_person_cam.SetActive(false); //disable active players cameras
        third_person_cam.SetActive(false);

        player_input_enabled = false; //disable user input

        this.gameObject.SetActive(false); //disable active model

        ghost.SetActive(true); //activate ghost
        //Activating ghost here calls On_Enable() in Ragdoll, the starting point of Ragdolling
    }
    private Quaternion return_to_floor_rotation; //gun transforms before being picked up
    private Vector3 return_to_floor_position;
    void Pick_Up_Gun(GameObject new_gun) //For picking up guns from floor
    {
        Quaternion new_gun_to_floor_rotation = new_gun.transform.rotation; //set currently pick up guns floor positions
        Vector3 new_gun_to_floor_position = new_gun.transform.position;

        if (currently_held_gun != null) //if player already has a gun (replace current gun with new gun)
        {
            currently_held_gun.transform.SetParent(null);
            currently_held_gun.transform.position = new_gun_to_floor_position; //move current gun to new guns floor position
            currently_held_gun.transform.rotation = new_gun_to_floor_rotation;
            currently_held_gun.GetComponent<Collider>().enabled = true;
            currently_held_gun = null; //remove current gun
        }

        player_audio_source.PlayOneShot(pick_up_gun, 1);

        return_to_floor_position = new_gun_to_floor_position; //reset floor position
        return_to_floor_rotation = new_gun_to_floor_rotation;

        guns_in_interactable_radius.RemoveAt(0); //remove from available gun queue  (as it has been picked up)

        currently_held_gun = new_gun; //set as current gun
        currently_held_gun.GetComponent<Collider>().enabled = false;

        Transform gun_grab_point = currently_held_gun.transform.Find("Grab_Point"); //move gun grab point to player grab point
        currently_held_gun.transform.SetParent(grab_point, true);
        currently_held_gun.transform.position = grab_point.position + grab_point.TransformDirection(gun_grab_point.localPosition * -1);
        currently_held_gun.transform.rotation = grab_point.rotation * Quaternion.Inverse(gun_grab_point.localRotation);

        guns_in_interactable_radius.Add(new_gun); //add gun to back of gun queue  (as it's still able to be put down /replaced)
    }

    private Quaternion return_item_to_floor_rotation;  //item transforms before being picked up
    private Vector3 return_item_to_floor_position;

    void Pick_Up_Shop_Item(GameObject item) //for picking up shop items
    {
        Quaternion new_gun_to_floor_rotation = item.transform.rotation; //set currently pick up items floor positions
        Vector3 new_gun_to_floor_position = item.transform.position;

        if (currently_held_item != null) //if player has item
        {
            currently_held_item.transform.SetParent(null);
            currently_held_item.transform.position = new_gun_to_floor_position; //move current item to new item floor position
            currently_held_item.transform.rotation = new_gun_to_floor_rotation;
            currently_held_item.GetComponent<Collider>().enabled = true;
            currently_held_item = null; //set as not currently held item
        }


        player_audio_source.PlayOneShot(pick_up_gun, 1);
        return_item_to_floor_position = new_gun_to_floor_position; //reset floor position
        return_item_to_floor_rotation = new_gun_to_floor_rotation;

        shop_items_in_interactable_radius.RemoveAt(0); //remove from available item queue (as it has been picked up)

        currently_held_item = item; //set as current item
        currently_held_item.GetComponent<Collider>().enabled = false;

        Transform gun_grab_point = currently_held_item.transform.Find("Grab_Point"); //move item grab point to player grab point
        currently_held_item.transform.SetParent(item_grab_point, true);
        currently_held_item.transform.position = item_grab_point.position + item_grab_point.TransformDirection(gun_grab_point.localPosition * -1);
        currently_held_item.transform.rotation = item_grab_point.rotation * Quaternion.Inverse(gun_grab_point.localRotation);

        shop_items_in_interactable_radius.Add(item); //add item to back of item queue (as it's still able to be put down /replaced)
    }

    public void Respawn_Player() //called to respawn user after Death
    {

        int respawners_in_scene = respawners_parent_holder.transform.childCount; //find all respawners 
        int random_respawn_index = Random.Range(0, respawners_in_scene); //pick random respawner
        Transform chosen_respawn_anchor = respawners_parent_holder.transform.GetChild(random_respawn_index);

        this.gameObject.transform.position = chosen_respawn_anchor.transform.position; //move player to respawner

        this.gameObject.SetActive(true); //set player elements as active and enabled
        this.first_person_cam.SetActive(false);
        this.third_person_cam.SetActive(true);
        this.player_input_enabled = true;
        Health = Full_Health;
        health_slider.value = Health;

        ghost.SetActive(false); //disable ghost elements
        death_screen.SetActive(false);

        isDead = false;
        player_audio_source.PlayOneShot(full_heal, 0.8f);
    }
    public void back_to_normal_scan() //called after ragdoll player immunity ends
    {
        immune = false;
    }

    public void scan_immunity() //called to enable player immunity after ragdolling
    {
        immune = true;
        Invoke("back_to_normal_scan", 3.0f);
    }


    private Quaternion return_to_floor_rotation_item; //original position of shop item
    private Vector3 return_to_floor_position_item;
    void Checkout_Held_Item() //called when user leave shop
    {
        if (currently_held_item != null)
        {
            if (coins_held >= item_price) //if player has enough money for item
            {
                player_audio_source.PlayOneShot(full_heal, 0.8f);
                if (currently_held_item.CompareTag("gun")) //if gun
                {
                    Quaternion new_gun_to_floor_rotation = currently_held_item.transform.rotation;
                    Vector3 new_gun_to_floor_position = currently_held_item.transform.position;

                    if (currently_held_gun != null) //replace old gun with new gun
                    {
                        currently_held_gun.transform.SetParent(null);
                        currently_held_gun.transform.position = return_to_floor_position_item;
                        currently_held_gun.transform.rotation = return_to_floor_rotation_item;
                        currently_held_gun.GetComponent<Collider>().enabled = true;
                        currently_held_gun = null;
                    }

                    currently_held_gun = currently_held_item; //set item as current gun
                    currently_held_item.transform.SetParent(null);
                    currently_held_item = null; //remove currently held item

                    Transform gun_grab_point = currently_held_gun.transform.Find("Grab_Point"); //move gun grab point to player grab point
                    currently_held_gun.transform.SetParent(grab_point, true);
                    currently_held_gun.transform.position = grab_point.position + grab_point.TransformDirection(gun_grab_point.localPosition * -1);
                    currently_held_gun.transform.rotation = grab_point.rotation * Quaternion.Inverse(gun_grab_point.localRotation);
                }
                else if (currently_held_item.CompareTag("Shop_Item")) //if shop item, change player variables depending on bought item
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

                coins_held = coins_held -= item_price; //pay for item
                stahs_collected.text = coins_held.ToString();
            }
            else //if not enough money for item
            {
                player_audio_source.PlayOneShot(ragdoll_audio, 0.8f);
            }
            currently_held_item.transform.SetParent(null); //remove currently held item

            currently_held_item.transform.position = return_to_floor_position_item; //move currently held item back to original shop position
            currently_held_item.transform.rotation = return_to_floor_rotation_item; ;
            currently_held_item.transform.localRotation = return_to_floor_rotation_item; ;
            currently_held_item = null;
        }

    }

    void Detect_in_Radius() //Fires frequetly, allowing player to interact and be effected by environment
    {
        bool found_shop = false; //if player in shop

        List<GameObject> current_guns = new List<GameObject>(); //list of guns
        List<GameObject> current_items = new List<GameObject>(); //list of items

        Collider[] interactable_colliders_in_radius = Physics.OverlapSphere(transform.position, interactables_radius);

        foreach (var interactable_collider in interactable_colliders_in_radius) //for all interactable elements
        {
            GameObject interactable_object_in_radius = interactable_collider.gameObject;
            if (walls_up == false) //if shop is inactive
            {
                if (interactable_object_in_radius.CompareTag("gun")) //if shop has gun as shop item
                {
                    current_guns.Add(interactable_object_in_radius);
                    if (!guns_in_interactable_radius.Contains(interactable_object_in_radius))
                    {
                        guns_in_interactable_radius.Add(interactable_object_in_radius); //add to list of all guns in interactable radius
                    }
                }
            }
            else //if shop is active
            {
                if (interactable_object_in_radius.CompareTag("gun") || interactable_object_in_radius.CompareTag("Shop_Item"))
                {
                    current_items.Add(interactable_object_in_radius);
                    if (!shop_items_in_interactable_radius.Contains(interactable_object_in_radius))
                    {
                        shop_items_in_interactable_radius.Add(interactable_object_in_radius); //add to list of all shop items in interactable radius
                    }
                }
            }
        }

        Collider[] environment_colliders_in_radius = Physics.OverlapSphere(transform.position, environment_radius);
        foreach (var environment_collider in environment_colliders_in_radius) // for all environmental obstacles / reactive objects in environment radius (smaller than interactable radius)
        {
            GameObject environment_object_in_radius = environment_collider.gameObject;

            Hurter hurter = environment_object_in_radius.GetComponent<Hurter>();
            if (hurter != null && hurter.isActiveAndEnabled) //if obstacle is an active hurter
            {
                Hurter hurterScript = environment_object_in_radius.GetComponent<Hurter>();
                hurterScript.Hurt(this); //inflict damage on player
            }
            else if (environment_object_in_radius.CompareTag("wake_on_player") && immune == false) //if obstacle is a ragdoller and player is not immune
            {
                Rigidbody rb = this.GetComponent<Rigidbody>();
                player_input_enabled = false;
                Ragdoll(); //ragdoll player
                Rigidbody ghostRigid = ghost.GetComponent<Rigidbody>();
                ghostRigid.AddExplosionForce(500 * 25.0f, environment_object_in_radius.transform.position, 25.0f); //propel ragdoll
            }
            else if (environment_object_in_radius.name == "Spring_Up") //if obstacle is spring
            {
                float spring_force = 70.0f;
                float spring_mag = Mathf.Sqrt(spring_force * -2.0f * gravityValue);

                Vector3 spring_direction = environment_object_in_radius.transform.forward.normalized;
                playerVelocity = spring_direction * spring_mag;
                player_audio_source.PlayOneShot(sping_up_audio, 0.8f);
            }
            else if (environment_object_in_radius.CompareTag("Heal")) //if reactive object is a partial heal
            {
                if (Health != Full_Health)
                {
                    if (Health + (Full_Health / 10) > Full_Health) //if health in nearly full, set to full health
                    {
                        Health = Full_Health;
                        player_audio_source.PlayOneShot(full_heal, 0.5f);
                    }
                    else
                    {
                        Health = Health + (Full_Health / 10); //else, heal some health points
                        player_audio_source.PlayOneShot(heal_audio, 0.5f);
                    }
                    health_slider.value = Health;
                }
            }
            else if (environment_object_in_radius.CompareTag("Full_Heal")) // if reactive object is full heal
            {
                if (Health < Full_Health)
                {
                    Health = Full_Health; //fully heal player
                    player_audio_source.PlayOneShot(full_heal, 0.75f);
                    health_slider.value = Health;
                }
            }
            else if (environment_object_in_radius.CompareTag("Coin")) // if reactive object is coin
            {
                coins_held += 1;
                stahs_collected.text = coins_held.ToString();

                coins_picked.Insert(0, environment_object_in_radius); //queue of coins, to be respawned after period of time
                environment_object_in_radius.SetActive(false); //hide coin

                AudioSource.PlayClipAtPoint(full_heal, environment_object_in_radius.transform.position, 1.0f);

                Invoke("reactive_coin", Random.Range(1.0f, 15.0f)); // respawn coin after random period
            }
            else if (environment_object_in_radius.CompareTag("Shop")) //if reactive object is shop
            {
                found_shop = true;
                if (current_shop != environment_object_in_radius) //set as current shop
                {
                    current_shop = environment_object_in_radius;
                    Wall_Up(environment_object_in_radius); //move shop walls up

                }
            }
        }
        // Outside of object detection

        guns_in_interactable_radius.RemoveAll(gun => !current_guns.Contains(gun)); //remove all guns not in radius
        shop_items_in_interactable_radius.RemoveAll(item => !current_items.Contains(item)); //remove all items not in radius

        if (!found_shop && current_shop != null) //if player leaves shop
        {
            Checkout_Held_Item(); //checkout item
            Wall_Down(current_shop); //move shop walls down
            current_shop = null;
        }
    }

    void reactive_coin() //respawn coin after random time
    {
        coins_picked[0].SetActive(true);
        coins_picked.RemoveAt(0); //remove from coin queue

    }
    void Wall_Up(GameObject shop_parent) //move walls of shop up
    {
        int walls_count = shop_parent.transform.childCount;
        if (walls_up == false)
        {
            foreach (Transform child_wall in shop_parent.transform)
            {

                if (child_wall.name == "Wall")
                {
                    Vector3 downpos = child_wall.transform.position;//move all shop walls up
                    downpos.y += 7.0f;
                    child_wall.transform.position = downpos;
                    walls_up = true;
                }
            }
        }

    }

    void Wall_Down(GameObject shop_parent) //move walls of shop down
    {
        int walls_count = shop_parent.transform.childCount;
        if (walls_up == true)
        {
            foreach (Transform child_wall in shop_parent.transform)
            {
                if (child_wall.name == "Wall")
                {
                    Vector3 downpos = child_wall.transform.position; //move all shop walls down
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
        if (ghost.activeSelf) //if player is in ghost state, do not take user input
        {
            return;
        }

        if (Time.time >= next_scan)
        {
            Detect_in_Radius(); //scan if last scan was more than scan_time 
            next_scan = Time.time + scan_wait_time;
        }

        float mouseX = Input.GetAxis("Mouse X") * sensitivityHor; //mouse inputs for camera and player rotation
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

        Cursor.lockState = CursorLockMode.Locked; //hide mouse cursor
        groundedPlayer = false;

        if (currently_held_gun != null) //if player is holding gun
        {
            //match gun rotation with camera pitch
            currently_held_gun.transform.localRotation = first_person_cameraTransform.localRotation;
            currently_held_gun.transform.localRotation = Quaternion.Slerp(
            currently_held_gun.transform.localRotation,
            targetPitchRotation,
            1f - Mathf.Exp(-lerp_speed * Time.deltaTime)
        );
        }
        Vector3 move = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.G) && player_input_enabled) //on g click (pick up interactable button)
        {
            if (walls_up == false) //if not in shop, pick up gun
            {
                if (guns_in_interactable_radius.Count > 0)
                {

                    Pick_Up_Gun(guns_in_interactable_radius[0]); //pick up first gun in interactable gun list
                }
            }
            else //if in shop, pick up shop item
            {
                if (shop_items_in_interactable_radius.Count > 0) //if shop has items
                {
                    return_to_floor_rotation_item = shop_items_in_interactable_radius[0].transform.rotation; //set item floor position
                    return_to_floor_position_item = shop_items_in_interactable_radius[0].transform.position;
                    Pick_Up_Shop_Item(shop_items_in_interactable_radius[0]);  //pick up first shop item in interactable items list
                }
            }
        }
        if (controller.isGrounded || Mathf.Abs(playerVelocity.y) <= 0.0001f) 
        {
            groundedPlayer = true; //if player is barely moving on y axis, or character controller is touching ground, player is grounded
        }
        if (groundedPlayer)
        {
            current_jumps = 0; //reset amount of jumps performed
            playerVelocity.y = -0.1f;
        }

        if (Input.GetKeyDown(KeyCode.Space) && player_input_enabled) //on jump key
        {
            if (current_jumps < consecutive_jumps_allowed) //if player can jump more
            {
                player_audio_source.PlayOneShot(jump_audio, 1);
                current_jumps++;
                playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue); //player jump
            }
        }

        if (player_input_enabled)
        {
            if (!groundedPlayer) 
            {
                playerVelocity.y += gravityValue * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.W)) //WASD inputs for moving forwards, left, back and right respectively
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

        if (Input.GetKeyUp(KeyCode.P)) //toggle between first and third person
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


        if (currently_held_gun != null && player_input_enabled) //if player is holding a gun
        {
            ICommon_Gun_Actions gun_interface = currently_held_gun.GetComponent<ICommon_Gun_Actions>();

            if (Input.GetKey(KeyCode.LeftShift) && first_person_cam.activeSelf == true) 
            {
                if (!isScoped) //scope in on left shift, is player is in first person and not already scoped in
                {
                    isScoped = true;
                    gun_interface.Scope_in();
                }

                Transform scopeTransform = gun_interface.Get_Scope();
                if (scopeTransform != null)
                {
                    first_person_cameraTransform.position = Vector3.Lerp( // move first person camera to scope
                        first_person_cameraTransform.position,
                        scopeTransform.position,
                        scopeTransitionSpeed * Time.deltaTime
                    );

                }
            }
            else
            {
                if (isScoped) //scope out on left shift release, is player is in first person and already scoped in
                {
                    isScoped = false;
                    gun_interface.Scope_out();
                }

                first_person_cameraTransform.localPosition = Vector3.Lerp( //move first person camera back to original position
                    first_person_cameraTransform.localPosition,
                    originalCameraPosition,
                    scopeTransitionSpeed * Time.deltaTime
                );
            }

            if (Input.GetKey(KeyCode.V) || Input.GetMouseButton(0)) //first on left click
            {
                gun_interface.Fire();
            }
            if (Input.GetKeyUp(KeyCode.V) || Input.GetMouseButtonUp(0)) //reload on left click
            {
                gun_interface.Reload();
            }
        }

        if (player_input_enabled) 
        {
            Vector3 finalMove = (move.normalized * move_speed) + new Vector3(0, playerVelocity.y, 0); //move player controller from player inputs
            controller.Move(finalMove * Time.deltaTime);
        }
        if (move != Vector3.zero)
        {

        }
    }

    public void TakeDamage(float damage) //Called when enemy hits player
    {
        int inted_damage = (int)damage;
        Loose_Health_Points(inted_damage, 0.2f);
    }

}
