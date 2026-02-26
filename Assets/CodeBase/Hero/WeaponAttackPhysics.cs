using System;
using CodeBase.GameLogic;
using CodeBase.Hero;
using CodeBase.Logic;
using CodeBase.StaticData;
using CodeBase.Infrastructure.Factory;
using UnityEngine;

namespace CodeBase.Combat
{
    public sealed class WeaponAttackPhysics
    {
        private readonly Collider[] _overlapHits;
        private readonly RaycastHit[] _rayHits;

        private readonly int _enemyMask;

        // 👇 нове
        private IDamagePopupService _popups;

        public Func<float, DamageRoll> DamageModifier;

        private readonly int[] _hitHealthIds;
        private int _hitHealthIdsCount;

        public WeaponAttackPhysics(int overlapSize = 16, int raySize = 32)
        {
            _overlapHits = new Collider[overlapSize];
            _rayHits = new RaycastHit[raySize];
            _enemyMask = LayerMask.GetMask("Enemy");
            _hitHealthIds = new int[Mathf.Max(8, overlapSize)];
        }
        public void SetPopups(IDamagePopupService popups) => _popups = popups;
        private void BeginAttackHitCache() => _hitHealthIdsCount = 0;

        private bool RegisterHit(IHealth health)
        {
            int id = ((Component)health).GetInstanceID();

            for (int i = 0; i < _hitHealthIdsCount; i++)
                if (_hitHealthIds[i] == id)
                    return false;

            if (_hitHealthIdsCount < _hitHealthIds.Length)
                _hitHealthIds[_hitHealthIdsCount++] = id;

            return true;
        }

        public bool HasEnemyOnLineForward(Vector3 origin, Vector3 forward, WeaponStats stats, Transform selfRoot)
        {
            Vector3 fwd = forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) return false;
            fwd.Normalize();

            Ray ray = new Ray(origin, fwd);

            int count = Physics.SphereCastNonAlloc(
                ray,
                stats.HitWidth * 0.5f,
                _rayHits,
                stats.Range,
                _enemyMask
            );

            for (int i = 0; i < count; i++)
            {
                var hit = _rayHits[i];
                if (hit.collider == null) continue;

                Transform enemy = hit.collider.transform.root;
                if (enemy == selfRoot) continue;

                Vector3 to = enemy.position - origin;
                to.y = 0f;
                if (to.sqrMagnitude < 0.0001f) continue;

                float dot = Vector3.Dot(fwd, to.normalized);
                if (dot < 0.3f) continue;

                return true;
            }

            return false;
        }

        public Transform FindNearestEnemy(Vector3 origin, float radius)
        {
            int count = Physics.OverlapSphereNonAlloc(origin, radius, _overlapHits, _enemyMask);
            if (count <= 0) return null;

            Transform best = null;
            float bestSqr = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var col = _overlapHits[i];
                if (col == null) continue;

                Transform t = col.transform.root;
                float sqr = (t.position - origin).sqrMagnitude;

                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = t;
                }
            }

            return best;
        }

        public bool AttackAura(Vector3 origin, WeaponStats stats, Transform selfRoot)
        {
            int count = Physics.OverlapSphereNonAlloc(origin, stats.Range, _overlapHits, _enemyMask);
            if (count <= 0) return false;

            BeginAttackHitCache();

            bool hitAny = false;

            for (int i = 0; i < count; i++)
            {
                var col = _overlapHits[i];
                if (col == null) continue;
                if (col.transform.root == selfRoot) continue;

                if (TryHit(col, stats.Damage, DamageModifier))
                    hitAny = true;
            }

            return hitAny;
        }

        public bool AttackCone(Vector3 origin, Vector3 forward, WeaponStats stats, float angleDeg, Transform selfRoot)
        {
            int count = Physics.OverlapSphereNonAlloc(origin, stats.Range, _overlapHits, _enemyMask);
            if (count <= 0) return false;

            BeginAttackHitCache();

            Vector3 fwd = forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) return false;
            fwd.Normalize();

            float cos = Mathf.Cos((angleDeg * 0.5f) * Mathf.Deg2Rad);

            int maxTargets = Mathf.Max(1, 1 + stats.Pierce);
            int hitCount = 0;

            for (int i = 0; i < count; i++)
            {
                Collider col = _overlapHits[i];
                if (col == null) continue;
                if (col.transform.root == selfRoot) continue;

                Vector3 to = col.transform.position - origin;
                to.y = 0f;
                if (to.sqrMagnitude < Constant.Epsilone) continue;

                if (Vector3.Dot(fwd, to.normalized) < cos)
                    continue;

                if (!TryHit(col, stats.Damage, DamageModifier))
                    continue;

                hitCount++;
                if (hitCount >= maxTargets)
                    break;
            }

            return hitCount > 0;
        }

        public bool AttackAim(Vector3 origin, WeaponStats stats, ProjectileFactory projectiles, WeaponId weaponId, Transform selfRoot)
        {
            if (projectiles == null) return false;
            if (weaponId == WeaponId.None) return false;

            Transform target = FindNearestEnemy(origin, stats.Range);
            if (target == null) return false;

            Vector3 dir = target.position - origin;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
                dir = selfRoot != null ? selfRoot.forward : Vector3.forward;

            projectiles.Spawn(weaponId, origin, dir.normalized, stats);
            return true;
        }

        public bool AttackLine(Vector3 origin, Vector3 forward, WeaponStats stats, ProjectileFactory projectiles, WeaponId weaponId)
        {
            if (projectiles == null) return false;
            if (weaponId == WeaponId.None) return false;

            Vector3 fwd = forward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.0001f) return false;
            fwd.Normalize();

            projectiles.Spawn(weaponId, origin, fwd, stats);
            return true;
        }

        private bool TryHit(Collider col, float baseDamage, Func<float, DamageRoll> damageModifier)
        {
            IHealth health = col.GetComponentInParent<IHealth>();
            if (health == null) return false;

            if (!RegisterHit(health))
                return false;

            float dmg = baseDamage;
            bool isCrit = false;

            if (damageModifier != null)
            {
                var roll = damageModifier(baseDamage);
                dmg = roll.Damage;
                isCrit = roll.IsCrit;
            }

            health.TakeDamage(dmg);

            // ✅ замість static Instance
            _popups?.Spawn(col.bounds.center, Mathf.RoundToInt(dmg), isCrit);

            return true;
        }
    }
}