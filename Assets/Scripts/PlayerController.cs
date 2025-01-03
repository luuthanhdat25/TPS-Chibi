using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float smoothRotateTimeMove = 0.1f;
    [SerializeField] private float smoothRotateTimeShoot = 0.05f;

    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerGunController playerGunController;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerInputMap input;
    [SerializeField] private PlayerAnimationController animationController;
    
    private float turnSmoothVelocity;

    private void Update()
    {
        Vector2 moveInput = input.GetRawInputNormalized();
        bool isMove = moveInput.magnitude >= 0.1f;

        if (isMove || playerGunController.IsShooting)
        {
            float targetAngle, smoothRotateTime;
            Vector3 moveDir;

            if (playerGunController.IsShooting)
            {
                targetAngle = cameraTransform.eulerAngles.y;
                smoothRotateTime = smoothRotateTimeShoot;
                moveDir = cameraTransform.forward * moveInput.y + cameraTransform.right * moveInput.x;
            }
            else
            {
                targetAngle = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
                smoothRotateTime = smoothRotateTimeMove;
                moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            }

            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, smoothRotateTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            moveDir.y = 0;
            controller.Move(moveDir.normalized * moveSpeed * Time.deltaTime);
        }

        animationController.SetIsRunning(isMove);
        animationController.SetRunBlendXY(moveInput.x, moveInput.y);
    }

}
