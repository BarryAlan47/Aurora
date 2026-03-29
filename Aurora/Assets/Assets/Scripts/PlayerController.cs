using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 玩家控制器：基于虚拟摇杆的角色移动与朝向控制。
/// 移动方向相对摄像机在地面上的投影（与 FastFoodRush 中 Quaternion.Euler(0,45,0)*input 同理，但适配任意机位）。
/// </summary>
public class PlayerController : MonoBehaviour
{
    [LabelText("移动摇杆")]
    public Joystick joystick;

    [LabelText("角色控制器")]
    public CharacterController controller;

    [LabelText("角色动画控制器")]
    public Animator anim;

    [LabelText("用于计算前后左右的摄像机（留空则用 MainCamera）")]
    [SerializeField]
    private Camera movementCamera;

    [LabelText("移动速度")]
    public float speed;

    [LabelText("重力系数")]
    public float gravity;

    [LabelText("当前移动方向")]
    Vector3 moveDirection;

    void Awake()
    {
        if (movementCamera == null)
            movementCamera = Camera.main;
    }

    /// <summary>
    /// 将屏幕/摇杆输入 (x=左右, y=上下) 转为世界 XZ 上的移动方向，与当前摄像机视角一致。
    /// </summary>
    Vector3 GetCameraRelativeMoveOnGround(Vector2 input)
    {
        if (input.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        if (movementCamera == null)
            movementCamera = Camera.main;
        if (movementCamera == null)
            return new Vector3(input.x, 0f, input.y).normalized;

        Vector3 camForward = movementCamera.transform.forward;
        camForward.y = 0f;
        if (camForward.sqrMagnitude < 1e-6f)
            camForward = Vector3.forward;
        else
            camForward.Normalize();

        Vector3 camRight = movementCamera.transform.right;
        camRight.y = 0f;
        if (camRight.sqrMagnitude < 1e-6f)
            camRight = Vector3.right;
        else
            camRight.Normalize();

        // 与 Unity 标准第三人称一致：Vertical → 摄像机朝前在地面上的方向，Horizontal → 右
        Vector3 move = camRight * input.x + camForward * input.y;
        return move.sqrMagnitude > 1e-6f ? move.normalized : Vector3.zero;
    }

    /// <summary>
    /// 获取移动方向：优先键盘（WASD/方向键），否则使用摇杆。
    /// </summary>
    Vector2 GetMoveDirection()
    {
        float x = 0f, y = 0f;

        // WASD 或 方向键
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;

        Vector2 keyDirection = new Vector2(x, y);
        if (keyDirection.sqrMagnitude > 0.01f)
            return keyDirection.normalized;

        // 无键盘输入时使用摇杆
        return joystick != null ? joystick.direction : Vector2.zero;
    }

    /// <summary>
    /// 每帧更新移动与旋转状态。
    /// </summary>
    void Update()
    {
        Vector2 direction = GetMoveDirection();
        Vector3 moveOnGround = GetCameraRelativeMoveOnGround(direction);

        if (controller.isGrounded)
            moveDirection = new Vector3(moveOnGround.x, 0, moveOnGround.z);

        moveDirection.y += gravity * Time.deltaTime;
        controller.Move(moveDirection * speed * Time.deltaTime);

        Quaternion targetRotation = moveOnGround != Vector3.zero
            ? Quaternion.LookRotation(moveOnGround)
            : transform.rotation;
        transform.rotation = targetRotation;

        if (direction != Vector2.zero)
            anim.SetBool("Run", true);
        else
            anim.SetBool("Run", false);
    }

    /// <summary>
    /// 向右侧小幅移动（用于解锁点动画偏移）。
    /// </summary>
    public void SidePos()
    {
        controller.Move(Vector3.right * 2.5f);
    }
}
