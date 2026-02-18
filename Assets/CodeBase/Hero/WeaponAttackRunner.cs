using CodeBase.Combat;
using CodeBase.Infrastructure.Factory;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.Weapon;
using UnityEngine;

namespace CodeBase.Hero
{
    public class WeaponAttackRunner : MonoBehaviour,
        IWeaponStatsApplier,
        IWeaponIdReceiver,
        IProjectileFactoryReceiver,
        IWeaponConfigReceiver,
        IAttackAnimationDriver
    {
        private const float RetryDelay = 0.05f;
        private const float MaxMeleeAnimRate = 3f;
        private const float AnimSpeedCap = 50f;

        [Header("Slot")] [SerializeField] private bool _isPrimarySlot; // true тільки на WeaponPrimary_Attack
        public bool IsPrimarySlot => _isPrimarySlot;

        [Header("Refs")] [SerializeField] private CharacterController _characterController;
        [SerializeField] private HeroAnimator _heroAnimator;
        [SerializeField] private TargetSensor _sensor;
        [SerializeField] private WeaponFxPlayer _fx;

        private WeaponStats _weaponStats;
        private bool _hasStats;

        private ProjectileFactory _projectiles;
        private WeaponId _weaponId;

        private WeaponAttackPhysics _physics;
        private PersistentAuraFx _auraFx;

        private float _cooldownLeft;
        private float _meleeAnimCdLeft;
        private bool _attackInProgress;

        private bool _warnedNoProjectiles;

        // хто драйвить анімацію (окремо від слота)
        private bool _isAnimationDriver;

        private void Awake()
        {
            CacheRefs();
            InitSystems();

            _isAnimationDriver = _isPrimarySlot;
        }

        private void CacheRefs()
        {
            if (_fx == null)
                _fx = GetComponentInChildren<WeaponFxPlayer>(true) ?? GetComponentInParent<WeaponFxPlayer>(true);

            if (_characterController == null)
                _characterController = GetComponentInParent<CharacterController>(true);

            if (_heroAnimator == null)
                _heroAnimator = GetComponentInParent<HeroAnimator>(true);

            if (_sensor == null)
                _sensor = GetComponentInChildren<TargetSensor>(true) ?? GetComponentInParent<TargetSensor>(true);
        }

        private void InitSystems()
        {
            _physics = new WeaponAttackPhysics(overlapSize: 16, raySize: 32);
            _physics.DamageModifier = RollDamage;
            _auraFx = new PersistentAuraFx();
            _auraFx.SetParentGetter(() => transform.root);
        }

        private void OnEnable() => _auraFx.OnEnable();

        private void OnDisable()
        {
            _auraFx.OnDisable();

            // якщо герой не пересоздається між ранами — чистимо runtime-стан
            _hasStats = false;
            _weaponStats = default;
            _cooldownLeft = 0f;
            _meleeAnimCdLeft = 0f;
            _attackInProgress = false;
        }

        // ---------------- injections ----------------

        public void Construct(ProjectileFactory projectiles)
        {
            _projectiles = projectiles;
            _warnedNoProjectiles = false;

            if (_projectiles != null)
                _projectiles.DamageModifier = RollDamage;
        }


        public void SetWeaponId(WeaponId id) => _weaponId = id;

        public void SetConfig(WeaponConfig cfg)
        {
            _auraFx.SetConfig(cfg);
            _fx?.SetConfig(cfg);
        }

        public void ApplyStats(WeaponStats stats)
        {
            Debug.Log($"CRIT: chance={_weaponStats.CritChance:0.###} mult={_weaponStats.CritMultiplier:0.###}");

            _weaponStats = stats;
            _hasStats = true;

            // не чіпаємо _cooldownLeft (щоб не ламати таймер)
            _attackInProgress = false;

            if (_sensor != null)
                _sensor.SetRadius(_weaponStats.Range);

            _auraFx.ApplyStats(stats);
        }

        public void SetAsAnimationDriver(bool isDriver)
        {
            _isAnimationDriver = isDriver;
            if (!isDriver) _attackInProgress = false;
        }

        // ---------------- update ----------------

        private void Update()
        {
            if (!_hasStats || _weaponStats.Cooldown <= 0f)
                return;

            TickTimers();
            if (_cooldownLeft > 0f)
                return;

            TryAttack();
        }

        private void TickTimers()
        {
            if (_meleeAnimCdLeft > 0f)
                _meleeAnimCdLeft -= Time.deltaTime;

            if (_cooldownLeft > 0f)
                _cooldownLeft -= Time.deltaTime;
        }

        private void TryAttack()
        {
            // AURA: не залежить від gate/анімацій
            if (_weaponStats.Shape == WeaponStats.AttackShape.Aura)
            {
                _cooldownLeft = Mathf.Max(0.02f, _weaponStats.Cooldown);
                _physics.AttackAura(HeroCenter(), _weaponStats, transform.root);
                return;
            }

            // primary: чекаємо animation event
            if (_isPrimarySlot && _attackInProgress)
                return;

            // gate: немає цілі — трохи почекай
            if (!CanStartAttack())
            {
                _cooldownLeft = RetryDelay;
                return;
            }

            // MELEE hybrid
            if (_weaponStats.Shape == WeaponStats.AttackShape.Cone)
            {
                if (ShouldUseMeleeTick() || !CanAnimateAsPrimary())
                    DoMeleeTick();
                else
                    StartPrimaryAttack();

                return;
            }

            // RANGED
            if (CanAnimateAsPrimary())
                StartPrimaryAttack();
            else
                DoSecondaryAttackNow();
        }

        private bool CanAnimateAsPrimary() =>
            _isPrimarySlot && _isAnimationDriver && _heroAnimator != null;

        // ---------------- PRIMARY ----------------

        private void StartPrimaryAttack()
        {
            _attackInProgress = true;
            _cooldownLeft = Mathf.Max(0.02f, _weaponStats.Cooldown);

            if (_heroAnimator == null)
                return;

            float animSpeed = Mathf.Min(AnimSpeedCap, Mathf.Max(0.1f, _weaponStats.AttackSpeed));
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
            }
        }

        // Animation Event
        public void OnAttack()
        {
            if (!CanAnimateAsPrimary()) return;
            if (!_attackInProgress) return;

            // melee overcap: дамаг не через event
            if (_weaponStats.Shape == WeaponStats.AttackShape.Cone && ShouldUseMeleeTick())
                return;

            _attackInProgress = false;

            Vector3 origin = HeroCenter();
            if (!DoAttack(origin)) return;

            _fx?.PlayAttackFx(origin);
        }

        public void OnAttackEnded()
        {
            if (!CanAnimateAsPrimary()) return;
            _heroAnimator?.ResetAttackSpeed();
        }

        // ---------------- SECONDARY ----------------

        private void DoSecondaryAttackNow()
        {
            Vector3 origin = HeroCenter();

            bool attacked = DoAttack(origin);
            _cooldownLeft = attacked ? Mathf.Max(0.02f, _weaponStats.Cooldown) : RetryDelay;

            if (!attacked) return;
            _fx?.PlayAttackFx(origin);
        }

        // ---------------- MELEE TICK ----------------

        private void DoMeleeTick()
        {
            Vector3 origin = HeroCenter();

            bool attacked = DoAttack(origin);
            _cooldownLeft = attacked ? Mathf.Max(0.02f, _weaponStats.Cooldown) : RetryDelay;

            if (!attacked) return;

            _fx?.PlayAttackFx(origin);

            // візуал махання обмежуємо
            if (_isPrimarySlot && _isAnimationDriver && _heroAnimator != null && _meleeAnimCdLeft <= 0f)
            {
                _meleeAnimCdLeft = 1f / MaxMeleeAnimRate;
                _heroAnimator.PlayAttack(HeroAnimator.AttackType.Melee);
            }
        }

        private bool ShouldUseMeleeTick()
        {
            float cd = Mathf.Max(0.0001f, _weaponStats.Cooldown);
            float logicRate = 1f / cd;
            return logicRate > MaxMeleeAnimRate;
        }

        // ---------------- SHARED ----------------

        private bool CanStartAttack()
        {
            Vector3 origin = HeroCenter();

            switch (_weaponStats.Shape)
            {
                case WeaponStats.AttackShape.Line:
                    return _physics.HasEnemyOnLineForward(origin, transform.root.forward, _weaponStats, transform.root);

                case WeaponStats.AttackShape.Aim:
                    return _physics.FindNearestEnemy(origin, _weaponStats.Range) != null;

                case WeaponStats.AttackShape.Cone:
                {
                    bool ok = _sensor != null &&
                              _sensor.TryGetNearestInFront(origin, transform.root.forward, 120f, out var t);
                    return ok;
                }

                default:
                    return true;
            }
        }

        private bool DoAttack(Vector3 origin)
        {
            // ranged потребує фабрику + валідний weaponId
            if (_weaponStats.Shape == WeaponStats.AttackShape.Line || _weaponStats.Shape == WeaponStats.AttackShape.Aim)
            {
                if (_projectiles == null)
                {
                    if (!_warnedNoProjectiles)
                    {
                        _warnedNoProjectiles = true;
                        Debug.LogError(
                            $"[WeaponAttackRunner] ProjectileFactory is NULL (weapon={_weaponId}, runner={name}). " +
                            $"Fix: caller must call Construct(projectiles) on this runner.");
                    }

                    return false;
                }
            }

            Vector3 fwd = transform.root.forward;

            switch (_weaponStats.Shape)
            {
                case WeaponStats.AttackShape.Line:
                    return _physics.AttackLine(origin, fwd, _weaponStats, _projectiles, _weaponId);

                case WeaponStats.AttackShape.Cone:
                    return _physics.AttackCone(origin, fwd, _weaponStats, 180f, transform.root);

                case WeaponStats.AttackShape.Aim:
                    return _physics.AttackAim(origin, _weaponStats, _projectiles, _weaponId, transform.root);

                default:
                    return false;
            }
        }

        private DamageRoll RollDamage(float baseDamage)
        {
            float chancePercent = _weaponStats.CritChance; // 10 = 10%
            float mult = _weaponStats.CritMultiplier; // 2 = x2

            float chance01 = Mathf.Clamp01(chancePercent * 0.01f);

            if (chance01 > 0f && Random.value < chance01)
            {
                float safeMult = Mathf.Max(1f, mult);
                return new DamageRoll(baseDamage * safeMult, true);
            }

            return new DamageRoll(baseDamage, false);
        }


        private Vector3 HeroCenter()
        {
            Transform root = transform.root;
            Vector3 basePos = root != null ? root.position : transform.position;

            float y = _characterController != null ? _characterController.center.y : 0.5f;
            return basePos + Vector3.up * y;
        }
    }
}