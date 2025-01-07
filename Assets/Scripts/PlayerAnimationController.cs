using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Rigging")]
    [SerializeField] private Rig shootingRig;

    [SerializeField] private Transform targetLeftHandRig;
    [SerializeField] private Transform hintLeftHandRig;

    [SerializeField] private Vector3 twoHandWeapon_Target;
    [SerializeField] private Vector3 twoHandWeapon_Hint;

    [SerializeField] private Vector3 oneHandWeapon_Target;
    [SerializeField] private Vector3 oneHandWeapon_Hint;

    public void SetAttackSpeedMuiltiplier(float value)
    {
        animator.SetFloat("AttackSpeedMultiplier", value);
    }

    public void SetMoveSpeedMuiltiplier(float value)
    {
        animator.SetFloat("MoveSpeedMultiplier", value);
    }

    public void SetIsShooting(bool value)
    {
        animator.SetBool("IsShooting", value);
        shootingRig.weight = value ? 1f : 0f;
    }

    public void SetIsTwoHand(bool value)
    {
        animator.SetBool("IsTwoHand", value);
        targetLeftHandRig.localPosition = value ? twoHandWeapon_Target : oneHandWeapon_Target;
        hintLeftHandRig.localPosition = value ? twoHandWeapon_Hint : oneHandWeapon_Hint;
    }

    public void SetIsRunning(bool value)
    {
        animator.SetBool("IsRunning", value);
    }

    public void SetRunBlendXY(float x, float y)
    {
        animator.SetFloat("RunBlendX", x);
        animator.SetFloat("RunBlendY", y);
    }

    public void SetJumpTrigger(float jumpSpeedMultiplier)
    {
        animator.SetFloat("JumpSpeedMultiplier", jumpSpeedMultiplier);
        animator.SetTrigger("JumpTrigger");
    }
}
