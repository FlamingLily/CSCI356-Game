using UnityEngine;
using UnityEngine.InputSystem;

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

    public GameObject first_person_cam;
    public Transform first_person_cameraTransform;
    public GameObject third_person_cam;
    public Transform third_person_cameraTransform;

    public CharacterController controller;

    // public float jumpHeight = 20.0f;
    // public float gravityValue = -10.0f;
    // public float jumpHeight = 30f;
    // public float gravityValue = 20f;

    public float jumpHeight;
    public float gravityValue;
    private Vector3 playerVelocity;
    private bool groundedPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float sensitivityHor;
    public float sensitivityVert;

    public float minimumVert;
    public float maximumVert;

    public float lerp_speed;

    private float verticalRot;
    private float horizontalRot;


    private Quaternion targetYawRotation;
    private Quaternion targetPitchRotation;


    void Start()
    {
        //if the cameraTransform doesnt exist
        // if (cameraTransform == null)
        //     cameraTransform = Camera.main.transform;

        //yaw rotation for rotating the player capsule
        targetYawRotation = transform.localRotation;

        //pitch for rotating only the camera
        targetPitchRotation = first_person_cameraTransform.localRotation;
    }
    // Update is called once per frame
    void Update()
    {
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


        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        // gun rotation matches pitch of camera

        Vector3 move = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.Space) && groundedPlayer)
        {
            Debug.Log("JUMP");
            // playerVelocity.y = jumpHeight;
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
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

        else if (Input.GetKeyUp(KeyCode.P))
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

        playerVelocity.y += gravityValue * Time.deltaTime;

        Vector3 finalMove = (move.normalized * move_speed) + new Vector3(0, playerVelocity.y, 0);
        controller.Move(finalMove * Time.deltaTime);
        if (move != Vector3.zero)
        {
            // controller.Move to move Characte Controller instead of Transform, so character can collide with walls
            // controller.Move(move.normalized * move_speed * Time.deltaTime);
            // playerVelocity.y += gravityValue * Time.deltaTime;

            // Vector3 finalMove = (move.normalized * move_speed) + (playerVelocity.y * Vector3.up);
            // controller.Move(finalMove * Time.deltaTime);
        }
    }

}
