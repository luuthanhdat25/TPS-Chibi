using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float baseMoveSpeed = 6f; //base speed for animation
    [SerializeField] private float smoothRotateTimeMove = 0.1f;
    [SerializeField] private float smoothRotateTimeShoot = 0.05f;

    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerGunController playerGunController;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerInputMap input;
    [SerializeField] private PlayerAnimationController animationController;

    [Header("Jump")]
    [SerializeField] private float maxJumpHeight = 2f;
    [SerializeField] private float maxJumpTime = 1f;
    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private AnimationClip jumpAnimClip;
    [SerializeField] private float groundGravity = 0.5f;
    [SerializeField] private float threshHoldRotationAllowShoot = 10f;

    private float gravity;
    private float initialJumpVelocity;
    private bool isJumpPressed = false;
    private bool isJumping = false;
    private Vector3 currentMovement;
    private float turnSmoothVelocity;

    private void Start()
    {
        UpdateMoveSpeed(moveSpeed);
        input.OnJumpStart += () => isJumpPressed = true;
        input.OnJumpCancle += () => isJumpPressed = false;

        CalculateJumpAndGravity();
    }

    private void CalculateJumpAndGravity()
    {
        float timeToApex = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
    }

    private void TriggerJumpAnimation()
    {
        animationController.SetJumpTrigger(jumpAnimClip.length / maxJumpTime);
    }

    public void UpdateMoveSpeed(float value)
    {
        moveSpeed = value;
        float moveSpeedMultiplier = value / baseMoveSpeed;
        animationController.SetMoveSpeedMuiltiplier(moveSpeedMultiplier);
    }

    private void Update()
    {
        Vector2 moveInput = input.GetRawInputNormalized();
        bool isMove = moveInput.magnitude >= 0.1f;
        Vector3 xzMovement = Vector3.zero;

        if (isMove || playerGunController.IsShootPressed)
        {
            float targetAngle, smoothRotateTime;

            if (playerGunController.IsShootPressed)
            {
                targetAngle = cameraTransform.eulerAngles.y;
                smoothRotateTime = smoothRotateTimeShoot;
                xzMovement = cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x;
            }
            else
            {
                targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                smoothRotateTime = smoothRotateTimeMove;
                xzMovement = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            }

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothRotateTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            xzMovement = xzMovement * moveSpeed;
        }

        currentMovement.x = xzMovement.x;
        currentMovement.z = xzMovement.z;

        if (IsGround())
        {
            currentMovement.y = -groundGravity;

            if (!isJumping && isJumpPressed)
            {
                isJumping = true;
                TriggerJumpAnimation();
                currentMovement.y = initialJumpVelocity;
            }
            else if (!isJumpPressed && isJumping)
            {
                isJumping = false;
            }
        }
        else
        {
            bool isFalling = currentMovement.y <= 0 || !isJumpPressed;
            float multiplier = isFalling ? fallMultiplier : 1f;
            float previousYVelocity = currentMovement.y;
            float newYVelocity = currentMovement.y + (gravity * multiplier * Time.deltaTime);
            float nextYVelocity = (previousYVelocity + newYVelocity) * .5f;
            currentMovement.y = nextYVelocity;
        }

        controller.Move(currentMovement * Time.deltaTime);
        animationController.SetIsRunning(isMove && controller.isGrounded);
        animationController.SetRunBlendXY(moveInput.x, moveInput.y);
    }

    private bool IsGround() => controller.isGrounded;

    public bool CanShoot()
    {
        float rotationDelta = transform.eulerAngles.y - cameraTransform.eulerAngles.y;
        return Mathf.Abs(rotationDelta) <= threshHoldRotationAllowShoot;
    }
}
