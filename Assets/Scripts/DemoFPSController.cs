using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("Base Speeds")]
    public float walkSpeed = 12f;        // 通常移動
    public float sprintSpeed = 35f;      // Shiftで走る
    public float gravity = -16f;         // 重力（-12〜-18で好み）
    public float jumpHeight = 1.8f;      // ジャンプ高さ(m)

    [Header("Jump Forward Boost")]
    public float jumpForwardBoost = 30f; // ジャンプ直後の“前”加速量
    public float boostDuration = 0.25f;  // ブーストを維持する秒数
    [Range(0f, 1f)]
    public float airControl = 0.6f;      // 空中で入力をどれだけ効かせるか

    [Header("View")]
    public Transform cameraTransform;    // Main Camera を割当て
    public float mouseSensitivity = 120f;
    public int baseFov = 85;             // 初期FOV

    // 実行時チューニング
    float speedScale = 1f;               // -/= で ±10%
    float camPitch = 0f;
    float boostTime = 0f;
    Vector3 cachedForward = Vector3.zero;
    Vector3 velocity;                    // 縦方向のみ
    CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        // 目線/コントローラの推奨値（念のため設定）
        controller.height = 1.8f;
        controller.center = new Vector3(0f, 0.9f, 0f);
        controller.stepOffset = 0.45f;
        controller.minMoveDistance = 0.001f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Camera.main) Camera.main.fieldOfView = baseFov;
    }

    void Update()
    {
        // ===== 視点（マウス） =====
        float mx = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float my = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mx);
        camPitch = Mathf.Clamp(camPitch - my, -80f, 80f);
        if (cameraTransform) cameraTransform.localEulerAngles = new Vector3(camPitch, 0, 0);

        // ===== 入力 =====
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 再生中チューニング：FOV（, / .）、速度プリセット（1〜4）、全体スケール（- / =）
        if (Input.GetKeyDown(KeyCode.Comma) && Camera.main) Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - 5, 60, 110);
        if (Input.GetKeyDown(KeyCode.Period) && Camera.main) Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView + 5, 60, 110);

        if (Input.GetKeyDown(KeyCode.Alpha1)) { walkSpeed = 8; sprintSpeed = 16; baseFov = 75; ApplyFov(); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { walkSpeed = 12; sprintSpeed = 35; baseFov = 85; ApplyFov(); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { walkSpeed = 20; sprintSpeed = 50; baseFov = 90; ApplyFov(); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { walkSpeed = 35; sprintSpeed = 90; baseFov = 95; ApplyFov(); }

        if (Input.GetKeyDown(KeyCode.Minus)) speedScale = Mathf.Max(0.1f, speedScale * 0.9f);
        if (Input.GetKeyDown(KeyCode.Equals)) speedScale = Mathf.Min(10f, speedScale * 1.1f);

        // ESCでマウスロック切替
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked) { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
            else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
        }

        // ===== 地面判定 =====
        bool grounded = controller.isGrounded;
        if (grounded && velocity.y < 0f) velocity.y = -2f;

        // ===== 移動方向の決定（カメラ基準）=====
        Vector3 camF = cameraTransform ? cameraTransform.forward : transform.forward;
        Vector3 camR = cameraTransform ? cameraTransform.right : transform.right;
        camF = Vector3.Scale(camF, new Vector3(1, 0, 1)).normalized;
        camR = Vector3.Scale(camR, new Vector3(1, 0, 1)).normalized;

        Vector3 moveDirInput = (camF * v + camR * h);
        if (moveDirInput.sqrMagnitude > 1f) moveDirInput.Normalize();

        // 速度（Shiftで走り、Ctrlでターボ）
        float baseSpeed = (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed);
        if (Input.GetKey(KeyCode.LeftControl)) baseSpeed *= 1.8f;
        baseSpeed *= speedScale;

        // ジャンプ直後の前方ブースト（一定時間、前方向を合成＆速度加算）
        Vector3 moveDir = moveDirInput;
        if (boostTime > 0f)
        {
            Vector3 f = (cachedForward.sqrMagnitude > 0.0001f) ? cachedForward : transform.forward;
            moveDir = (moveDir + f).normalized;
            baseSpeed += jumpForwardBoost;
            boostTime -= Time.deltaTime;
        }

        // 空中制御（慣性寄りにする）
        if (!grounded && cachedForward != Vector3.zero)
            moveDir = Vector3.Lerp(cachedForward, moveDir, airControl).normalized;

        // 水平移動
        controller.Move(moveDir * baseSpeed * Time.deltaTime);

        // ===== ジャンプ & 重力（垂直だけ別管理）=====
        if (Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            cachedForward = (moveDirInput.sqrMagnitude > 0.0001f) ? moveDirInput : transform.forward;
            boostTime = boostDuration;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void OnGUI()
    {
        // 左上にHUD（デモ用）
        GUI.Box(new Rect(10, 10, 300, 80),
            $"Walk:{walkSpeed:F1}  Sprint:{sprintSpeed:F1}  Scale:{speedScale:F2}\n" +
            $"Boost:{jumpForwardBoost:F1}/{boostDuration:F2}s  AirCtrl:{airControl:F2}\n" +
            $"FOV:{(Camera.main ? Camera.main.fieldOfView : 0):F0}  Preset:[1-4]  Speed[-/=]  FOV[,/.]\n" +
            $"Run:Shift  Turbo:Ctrl  Jump:Space  MouseLock:Esc");
    }

    void ApplyFov()
    {
        if (Camera.main) Camera.main.fieldOfView = baseFov;
    }
}
