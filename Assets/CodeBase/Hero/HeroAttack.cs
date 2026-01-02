using CodeBase.Data;
using CodeBase.Infrastructure.Services.PersistentProgress;
using CodeBase.Logic;
using CodeBase.StaticData;
using UnityEngine;

namespace CodeBase.Hero
{
    public class HeroAttack : MonoBehaviour, ISavedProgressReader
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
            _weaponStats = stats;
            _hasStats = true;

            if (_zone != null)
                _zone.SetRadius(_weaponStats.DamageRadius);
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

        public void LoadProgress(PlayerProgress progress)
        {
            progress.RunProgressData ??= new RunProgressData();

            _weaponStats = progress.RunProgressData.WeaponStats;
            _hasStats = true;

            if (_zone != null)
                _zone.SetRadius(_weaponStats.DamageRadius);
        }

        private void AttackOnce()
        {
            int count = Physics.OverlapSphereNonAlloc(
                HeroCenter(),
                _weaponStats.DamageRadius,
                _hits,
                _enemyMask
            );

            for (int i = 0; i < count; i++)
            {
                Collider col = _hits[i];
                if (col == null)
                    continue;

                if (col.transform.root == transform.root)
                    continue;

                IHealth health = col.GetComponentInParent<IHealth>();
                if (health == null)
                    continue;

                health.TakeDamage(_weaponStats.Damage);
            }
        }

        private Vector3 HeroCenter()
        {
            float y = _characterController != null ? _characterController.center.y : 0.5f;
            return transform.position + Vector3.up * y;
        }
    }
}
