using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanoidAnimator : MonoBehaviour
{
    public enum IdleType
    {
        Standing = 0,
        CrossedArms = 1,
        HandsOnHips = 2,
        CheckWatch = 3,
        Dance = 4,
        Smoking = 5,
        Salute = 6,
        WipeMouth = 7,
        LearningAgainstWall = 8,
        SittingOnGround = 9,
    }
    
    public enum WeaponType
    {
        None = 0,
    }
    
    // Cached Properties
    private static readonly int AnimationDeathBool = Animator.StringToHash("Death_b");
    private static readonly int AnimationCrouchBool = Animator.StringToHash("Crouch_b");
    private static readonly int AnimationIdleInt = Animator.StringToHash("Animation_int");
    private static readonly int AnimationSpeedFloat = Animator.StringToHash("Speed_f");
    private static readonly int AnimationWeaponTypeInt = Animator.StringToHash("WeaponType_int");
    
    // Serialized Properties
    public IdleType defaultIdleType = IdleType.Standing;
    public WeaponType defaultWeaponType = WeaponType.None;

    // Private Properties
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        _animator.SetFloat(AnimationSpeedFloat, 0.0f);
        _animator.SetInteger(AnimationIdleInt, (int)defaultIdleType);
        _animator.SetInteger(AnimationWeaponTypeInt, (int)defaultWeaponType);
    }
    
    private void Update()
    {
        
    }

    public void Die()
    {
        _animator.SetBool(AnimationDeathBool, true);
    }
    
    public void SetCrouched(bool isCrouched)
    {
        _animator.SetBool(AnimationCrouchBool, isCrouched);
    }

    public void WakeUp()
    {

    }
    
    private IEnumerator _GetUp()
    {
        yield return new WaitForSeconds(2.0f);
        SetCrouched(true);
        _SetIdleAnimation(IdleType.Standing);
        yield return new WaitForSeconds(1.5f);
        SetCrouched(false);
    }

    private void _SetIdleAnimation(IdleType idleType)
    {
        _animator.SetInteger(AnimationIdleInt, (int)idleType);
    }
}
