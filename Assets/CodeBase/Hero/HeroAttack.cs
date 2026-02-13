using CodeBase.Combat;
using CodeBase.Infrastructure.Factory;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.Weapon;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroAttack : MonoBehaviour,
        IWeaponStatsApplier,
        IWeaponIdReceiver,
        IProjectileFactoryReceiver,
        IWeaponConfigReceiver,
        IAttackAnimationDriver
    {
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private HeroAnimator _heroAnimator;
        [SerializeField] private TargetSensor _sensor;
        [SerializeField] private WeaponFxPlayer _fx;

        private WeaponConfig _cfg;
        private WeaponStats _weaponStats;

        private bool _hasStats;
        private float _cooldownLeft;
        private bool _attackInProgress;

        private ProjectileFactory _projectiles;
        private WeaponId _weaponId;
        private bool _isDriver = true;

        private WeaponAttackPhysics _physics;
        private PersistentAuraFx _auraFx;

        private const float AnimSpeedCap = 2.5f;

        private void Awake()
        {
            if (_heroAnimator == null)
                _heroAnimator = GetComponentInParent<HeroAnimator>(true) ?? GetComponentInChildren<HeroAnimator>(true);

            if (_sensor == null)
                _sensor = GetComponentInChildren<TargetSensor>(true);

            if (_fx == null)
                _fx = GetComponentInChildren<WeaponFxPlayer>(true) ?? GetComponentInParent<WeaponFxPlayer>(true);

            _physics = new WeaponAttackPhysics(overlapSize: 16, raySize: 32);

            _auraFx = new PersistentAuraFx();
            _auraFx.SetParentGetter(() => transform.root);



        }

        private void OnEnable() => _auraFx.OnEnable();


        public void Construct(ProjectileFactory projectiles) => _projectiles = projectiles;
        public void SetWeaponId(WeaponId id) => _weaponId = id;
        public void SetAsAnimationDriver(bool isDriver) => _isDriver = isDriver;

        public void SetConfig(WeaponConfig cfg)
        {
            _cfg = cfg;

            // aura persistent — як у Runner
            _auraFx.SetConfig(cfg);

            // one-shot FX config (сам PlayAttackFx відсікає aura)
            _fx?.SetConfig(cfg);
        }

        public void ApplyStats(WeaponStats stats)
        {
            _weaponStats = stats;
            _hasStats = true;

            if (_sensor != null)
                _sensor.SetRadius(_weaponStats.Range);
            // aura persistent — як у Runner
            _auraFx.ApplyStats(stats);

            _cooldownLeft = 0f;
            _attackInProgress = false;
        }

        private void Update()
        {
            if (!_hasStats) return;
            if (_weaponStats.Cooldown <= 0f) return;

            if (_cooldownLeft > 0f)
            {
                _cooldownLeft -= Time.deltaTime;
                return;
            }

            if (_attackInProgress) return;

            // ---------------------------
            // AURA: tick without animation (як у тебе)
            // ---------------------------
            if (_weaponStats.Shape == WeaponStats.AttackShape.Aura)
            {
                _cooldownLeft = _weaponStats.Cooldown;
                _physics.AttackAura(HeroCenter(), _weaponStats, transform.root);
                return;
            }

            // ---------------------------
            // Gate: do we have a target?
            // ---------------------------
            if (_weaponStats.Shape == WeaponStats.AttackShape.Line)
            {
                if (!_physics.HasEnemyOnLineForward(HeroCenter(), transform.root.forward, _weaponStats, transform.root))
                {
                    _cooldownLeft = 0.05f;
                    return;
                }
            }
            else if (_weaponStats.Shape == WeaponStats.AttackShape.Aim)
            {
                if (_physics.FindNearestEnemy(HeroCenter(), _weaponStats.Range) == null)
                {
                    _cooldownLeft = 0.05f;
                    return;
                }
            }
            else
            {
                if (_sensor == null || !_sensor.TryGetNearest(HeroCenter(), out _))
                {
                    _cooldownLeft = 0.05f;
                    return;
                }
            }

            StartAttack();
        }

        private void StartAttack()
        {
            _attackInProgress = true;
            _cooldownLeft = _weaponStats.Cooldown;

            if (_isDriver)
                SwitchWeaponAnimation();
        }

        private void SwitchWeaponAnimation()
        {
            float animSpeed = Mathf.Min(AnimSpeedCap,
                _weaponStats.AttackSpeed <= 0 ? 1f : _weaponStats.AttackSpeed);

            _heroAnimator.SetAttackSpeed(animSpeed);
            switch (_weaponStats.Shape)
            {
                case WeaponStats.AttackShape.Line:
                case WeaponStats.AttackShape.Aim:
                    _heroAnimator.PlayAttack(HeroAnimator.AttackType.Ranged);
                    break;

                case WeaponStats.AttackShape.Cone:
                    _heroAnimator.PlayAttack(HeroAnimator.AttackType.Melee);
                    break;

                // Aura тут не треба (вона без анімації у тебе)
            }
        }
     

        // Animation Event (кліп): OnAttack
        public void OnAttack()
        {
            if (!_attackInProgress) return;
            _attackInProgress = false;

            Vector3 origin = HeroCenter();

            bool attacked = false;

            switch (_weaponStats.Shape)
            {
                case WeaponStats.AttackShape.Line:
                    attacked = _physics.AttackLine(origin, transform.root.forward, _weaponStats, _projectiles, _weaponId);
                    break;

                case WeaponStats.AttackShape.Cone:
                    attacked = _physics.AttackCone(origin, transform.root.forward, _weaponStats, 180f, transform.root);
                    break;

                case WeaponStats.AttackShape.Aim:
                    attacked = _physics.AttackAim(origin, _weaponStats, _projectiles, _weaponId, transform.root);
                    break;

                case WeaponStats.AttackShape.Aura:
                    attacked = _physics.AttackAura(origin, _weaponStats, transform.root);
                    break;
            }

            if (attacked)
                _fx?.PlayAttackFx(origin); // aura всередині WeaponFxPlayer відсікається
        }

        public void OnAttackEnded() { _heroAnimator?.ResetAttackSpeed();}

        private Vector3 HeroCenter()
        {
            float y = _characterController != null ? _characterController.center.y : 0.5f;
            return transform.position + Vector3.up * y;
        }
    }
}
