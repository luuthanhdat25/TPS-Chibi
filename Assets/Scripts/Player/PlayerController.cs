using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float baseMoveSpeed = 6f; //base speed for animation

    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerInputMap input;
    [SerializeField] private PlayerAnimationController animationController;

    [Header("Jump")]
    [SerializeField] private float maxJumpHeight = 2f;
    [SerializeField] private float maxJumpTime = 1f;
    [SerializeField] private float fallMultiplier = 2f;
    [SerializeField] private AnimationClip jumpAnimClip;
    [SerializeField] private float groundGravity = 0.5f;

    private float gravity;
    private float initialJumpVelocity;
    private bool isJumpPressed = false;
    private bool isJumping = false;
    private Vector3 currentMovement;

    private void Start()
    {
        UpdateMoveSpeed(moveSpeed);
        input.OnJumpStart += () => isJumpPressed = true;
        input.OnJumpCancle += () => isJumpPressed = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

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
        
        transform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        
        Vector3 xzMovement;
        xzMovement = transform.forward * moveInput.y + transform.right * moveInput.x;
        xzMovement = xzMovement.normalized * moveSpeed;
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
}
