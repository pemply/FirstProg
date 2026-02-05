using System;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsSittingHash = Animator.StringToHash("IsSitting");
        private static readonly int AttackSpeedHash = Animator.StringToHash("AttackSpeed");

        [SerializeField] private Animator _animator;
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int AttackTypeHash = Animator.StringToHash("AttackType");

        private float _idleTimer;
        private const float SitDelay = 3f;
        private const float SpeedEps = 0.1f;
        

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>(true);
            
            Debug.Log($"[HeroAnimator] Awake animGO={_animator.gameObject.name} root={transform.root.name} ctrl={_animator.runtimeAnimatorController?.name}");
        }

        public void SetSpeed(float speed)
        {
            _animator.SetFloat(SpeedHash, speed);

            if (speed > SpeedEps)
            {
                _idleTimer = 0f;
                _animator.SetBool(IsSittingHash, false);
                return;
            }

            _idleTimer += Time.deltaTime;
            if (_idleTimer >= SitDelay)
                _animator.SetBool(IsSittingHash, true);
        }

        // ✅ NEW: швидкість анімації атаки (простий варіант через animator.speed)
        public void SetAttackSpeed(float mult)
        {
            if (_animator == null) return;
            _animator.SetFloat(AttackSpeedHash, Mathf.Max(0.01f, mult));
        }

        public void ResetAttackSpeed()
        {
            if (_animator == null) return;
            _animator.SetFloat(AttackSpeedHash, 1f);
        }


        public enum AttackType
        {
            Melee = 0,
            Ranged = 1,
            Cast = 2
        }
        

        public void PlayAttack(AttackType type)
        {
            _animator.SetInteger(AttackTypeHash, (int)type);
            _animator.SetTrigger(AttackHash);
        }
    }
}
