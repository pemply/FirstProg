using CodeBase.Combat;
using CodeBase.GameLogic;
using CodeBase.Infrastructure.Factory;
using CodeBase.Infrastructure.Services;
using CodeBase.Infrastructure.Services.Pool;
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

        [Header("Slot")]
        [SerializeField] private bool _isPrimarySlot; // true тільки на WeaponPrimary_Attack
        public bool IsPrimarySlot => _isPrimarySlot;

        [Header("Refs")]
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private HeroAnimator _heroAnimator;
        [SerializeField] private TargetSensor _sensor;

        private WeaponFxPlayer _fx;
        private IDamagePopupService _popups;

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

        // juice (камера/флеш/хітстоп) — окремий компонент
        private CombatJuice _juice;

        private void Awake()
        {
            CacheRefs();
            InitSystems();

            _fx?.Construct(AllServices.Container.Single<IPoolService>());

            _isAnimationDriver = _isPrimarySlot;
            _juice = Camera.main != null ? Camera.main.GetComponent<CombatJuice>() : null;
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

            if (_popups != null)
                _physics.SetPopups(_popups);

            _auraFx = new PersistentAuraFx();
            _auraFx.SetParentGetter(() => transform.root);
        }

        private void OnEnable() => _auraFx.OnEnable();

        private void OnDisable()
        {
            _auraFx.OnDisable();

            _hasStats = false;
            _weaponStats = default;
            _cooldownLeft = 0f;
            _meleeAnimCdLeft = 0f;
            _attackInProgress = false;
        }

        // ---------------- injections ----------------

        public void Construct(IDamagePopupService popups)
        {
            _popups = popups;
            _physics?.SetPopups(_popups);
        }

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
            _weaponStats = stats;
            _hasStats = true;

            Debug.Log($"[WeaponAttackRunner] weapon={_weaponId} knockback={_weaponStats.Knockback} chance={_weaponStats.KnockbackChance}");

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

            if (_weaponStats.Shape == WeaponStats.AttackShape.Cone && ShouldUseMeleeTick())
                return;

            _attackInProgress = false;

            Vector3 origin =
                (_weaponStats.Shape == WeaponStats.AttackShape.Line || _weaponStats.Shape == WeaponStats.AttackShape.Aim)
                    ? RangedMuzzleOrigin()
                    : HeroCenter();

            if (!DoAttack(origin)) return;

            if (_weaponStats.Shape == WeaponStats.AttackShape.Cone)
                PlayMeleeFx(origin);
        }

        public void OnAttackEnded()
        {
            if (!CanAnimateAsPrimary()) return;
            _heroAnimator?.ResetAttackSpeed();
        }

        // ---------------- SECONDARY ----------------

        private void DoSecondaryAttackNow()
        {
            Vector3 origin =
                (_weaponStats.Shape == WeaponStats.AttackShape.Line || _weaponStats.Shape == WeaponStats.AttackShape.Aim)
                    ? RangedMuzzleOrigin()
                    : HeroCenter();

            bool attacked = DoAttack(origin);
            _cooldownLeft = attacked ? Mathf.Max(0.02f, _weaponStats.Cooldown) : RetryDelay;

            if (!attacked) return;

            if (_weaponStats.Shape == WeaponStats.AttackShape.Cone)
                PlayMeleeFx(origin);
        }

        // ---------------- MELEE TICK ----------------

        private void DoMeleeTick()
        {
            Vector3 origin = HeroCenter();

            bool attacked = DoAttack(origin);
            _cooldownLeft = attacked ? Mathf.Max(0.02f, _weaponStats.Cooldown) : RetryDelay;

            if (!attacked) return;

            PlayMeleeFx(origin);

            if (_isPrimarySlot && _isAnimationDriver && _heroAnimator != null && _meleeAnimCdLeft <= 0f)
            {
                _meleeAnimCdLeft = 1f / MaxMeleeAnimRate;
                _heroAnimator.PlayAttack(HeroAnimator.AttackType.Melee);
            }
        }

        private void PlayMeleeFx(Vector3 origin)
        {
            if (_fx == null) return;
            if (_weaponStats.Shape != WeaponStats.AttackShape.Cone) return;

            Vector3 fwd = transform.root.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;

            Vector3 fxOrigin = origin + fwd.normalized * 0.8f + Vector3.up * 0.15f;
            _fx.PlayAttackFx(fxOrigin, fwd, _weaponStats.Range, _weaponStats.HitWidth);
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
                              _sensor.TryGetNearestInFront(origin, transform.root.forward, 120f, out _);
                    return ok;
                }

                default:
                    return true;
            }
        }

        private bool DoAttack(Vector3 origin)
        {
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

            Vector3 fwd = (_weaponStats.Shape == WeaponStats.AttackShape.Line || _weaponStats.Shape == WeaponStats.AttackShape.Aim)
                ? RangedForward()
                : transform.root.forward;

            switch (_weaponStats.Shape)
            {
                case WeaponStats.AttackShape.Line:
                    return _physics.AttackLine(origin, fwd, _weaponStats, _projectiles, _weaponId, transform.root);

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
            float chance01 = Mathf.Clamp01(_weaponStats.CritChance * 0.01f);

            if (chance01 > 0f && Random.value < chance01)
            {
                _juice?.OnCrit();

                float safeMult = Mathf.Max(1f, _weaponStats.CritMultiplier);
                return new DamageRoll(baseDamage * safeMult, true);
            }

            return new DamageRoll(baseDamage, false);
        }

        private Vector3 RangedMuzzleOrigin()
        {
            var m = GetMuzzle();
            return m != null ? m.position : HeroCenter();
        }

        private Vector3 RangedForward()
        {
            var m = GetMuzzle();
            Vector3 fwd = m != null ? m.forward : transform.root.forward;

            fwd.y = 0f; // top-down
            return fwd.sqrMagnitude < 0.0001f ? Vector3.forward : fwd.normalized;
        }

        private Transform GetMuzzle()
        {
            var all = transform.root.GetComponentsInChildren<MonoBehaviour>(true);
            IWeaponPresentation pres = null;

            for (int i = 0; i < all.Length; i++)
                if (all[i] is IWeaponPresentation p) { pres = p; break; }

            return pres?.Muzzle;
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