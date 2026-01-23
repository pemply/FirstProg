using CodeBase.Data;
using CodeBase.Enemy;
using CodeBase.Infrastructure.Services.RunTime;
using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroAttack : MonoBehaviour, IWeaponStatsApplier
    {
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private AttackRadiusZoneForHero _zone;

        private readonly Collider[] _hits = new Collider[8];
        private int _enemyMask;

        private WeaponStats _weaponStats;
        private bool _hasStats;
        private float _cooldownLeft;

        private void Awake()
        {
            if (_zone == null)
                _zone = GetComponentInChildren<AttackRadiusZoneForHero>(true);

            _enemyMask = LayerMask.GetMask("Enemy");
        }

        public void ApplyStats(WeaponStats stats)
        {
            Debug.Log(
                $"[HERO ATTACK] {gameObject.name} APPLY shape={stats.Shape} dmg={stats.Damage} range={stats.Range} width={stats.HitWidth} cd={stats.Cooldown}");

            _weaponStats = stats;
            _hasStats = true;

            if (_zone != null)
            {
                // зона має сенс тільки для "локальних" форм (Cone/Aura)
                bool showZone = _weaponStats.Shape != WeaponStats.AttackShape.Line;
                _zone.gameObject.SetActive(showZone);

                if (showZone)
                {
                    // Для Cone/Aura логічно показувати радіус = Range
                    _zone.SetRadius(_weaponStats.Range);
                }
            }

            Debug.Log(
                $"[HERO ATTACK] ApplyStats " +
                $"dmg={stats.Damage}, " +
                $"range={stats.Range}, " +
                $"width={stats.HitWidth}, " +
                $"cd={stats.BaseCooldown}"
            );
        }

        private void Update()
        {
            if (!_hasStats)
                return;

            if (_weaponStats.Cooldown <= 0f)
                return;

            if (_cooldownLeft > 0f)
            {
                _cooldownLeft -= Time.deltaTime;
                return;
            }

            AttackOnce();
            _cooldownLeft = _weaponStats.Cooldown;
        }

        private void AttackOnce()
        {
            Vector3 from = HeroCenter();
            Vector3 to = from + transform.forward * _weaponStats.Range;
            Debug.DrawLine(from, to, Color.red, 0.2f);

            switch (_weaponStats.Shape)
            {
                case WeaponStats.AttackShape.Line:
                    AttackLine();
                    break;

                case WeaponStats.AttackShape.Cone:
                    AttackCone(180f);
                    break;

                case WeaponStats.AttackShape.Aura:
                    AttackAura();
                    break;
            }
        }

        private void AttackCone(float angleDeg)
        {
            Vector3 origin = HeroCenter();
            float radius = _weaponStats.Range;

            int count = Physics.OverlapSphereNonAlloc(origin, radius, _hits, _enemyMask);

            float cos = Mathf.Cos((angleDeg * 0.5f) * Mathf.Deg2Rad); // для 180 => cos(90)=0

            for (int i = 0; i < count; i++)
            {
                Collider col = _hits[i];
                if (col == null) continue;
                if (col.transform.root == transform.root) continue;

                Vector3 to = col.transform.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude < 0.0001f) continue;

                if (Vector3.Dot(transform.forward, to.normalized) < cos)
                    continue;

                TryHit(col);
            }
        }

        private void AttackAura()
        {
            Vector3 origin = HeroCenter();
            float radius = _weaponStats.Range;

            int count = Physics.OverlapSphereNonAlloc(origin, radius, _hits, _enemyMask);

            for (int i = 0; i < count; i++)
            {
                Collider col = _hits[i];
                if (col == null) continue;
                if (col.transform.root == transform.root) continue;

                TryHit(col);
            }
        }


        private void AttackLine()
        {
            Vector3 origin = HeroCenter();
            Vector3 dir = transform.forward;

            Ray ray = new Ray(origin, dir);

            RaycastHit[] hits = Physics.SphereCastAll(
                ray,
                _weaponStats.HitWidth * 0.5f,
                _weaponStats.Range,
                _enemyMask
            );

            int maxTargets = Mathf.Max(1, 1 + _weaponStats.Pierce);
            int hitCount = 0;

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;

                // (опційно) відсікти бокових/позаду, якщо товщина велика
                Vector3 to = hit.collider.transform.position - transform.position;
                to.y = 0f;
                if (to.sqrMagnitude > Constant.Epsilone)
                {
                    if (Vector3.Dot(transform.forward, to.normalized) < 0.2f)
                        continue;
                }

                if (!TryHit(hit.collider))
                    continue;

                hitCount++;
                if (hitCount >= maxTargets)
                    break;
            }
        }


        private bool TryHit(Collider col)
        {
            if (col.transform.root == transform.root)
                return false;

            IHealth health = col.GetComponentInParent<IHealth>();
            if (health == null)
                return false;

            health.TakeDamage(_weaponStats.Damage);

            var kb = col.GetComponentInParent<EnemyKnockback>();
            if (kb != null)
            {
                Vector3 dir = col.transform.position - transform.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.0001f)
                    kb.Push(dir.normalized, _weaponStats.Knockback);
            }

            return true;
        }

        private Vector3 HeroCenter()
        {
            float y = _characterController != null ? _characterController.center.y : 0.5f;
            return transform.position + Vector3.up * y;
        }

#if UNITY_EDITOR
#endif
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;
            if (!_hasStats) return;

            Vector3 origin = HeroCenter();

            switch (_weaponStats.Shape)
            {
                case WeaponStats.AttackShape.Aura:
                    Gizmos.DrawWireSphere(origin, _weaponStats.Range);
                    break;

                case WeaponStats.AttackShape.Cone:
                    DrawConeGizmo(origin, _weaponStats.Range, 180f);
                    break;

                case WeaponStats.AttackShape.Line:
                    Vector3 to = origin + transform.forward * _weaponStats.Range;
                    Gizmos.DrawLine(origin, to);
                    Gizmos.DrawWireSphere(origin, _weaponStats.HitWidth * 0.5f);
                    break;
            }

#if UNITY_EDITOR
            // підпис над героєм (або над обʼєктом WeaponPrimary/WeaponSecondary)
            Handles.Label(origin + Vector3.up * 0.2f, $"{gameObject.name} | {_weaponStats.Shape}");
#endif
        }

        private void DrawConeGizmo(Vector3 origin, float radius, float angleDeg)
        {
            Vector3 fwd = transform.forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;
            fwd.Normalize();

            float half = angleDeg * 0.5f;

            Vector3 left = Quaternion.Euler(0f, -half, 0f) * fwd;
            Vector3 right = Quaternion.Euler(0f, half, 0f) * fwd;

            // межі конуса
            Gizmos.DrawLine(origin, origin + left * radius);
            Gizmos.DrawLine(origin, origin + right * radius);

#if UNITY_EDITOR
            // дуга
            Handles.DrawWireArc(origin, Vector3.up, left, angleDeg, radius);
#endif
        }
    }
}