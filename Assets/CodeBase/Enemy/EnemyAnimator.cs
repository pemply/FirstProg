using System.Collections.Generic;
using CodeBase.Infrastructure.Services.Pool;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class EnemyAnimator : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent _agent;

        [Header("Explosion (optional)")]
        [SerializeField] private GameObject _explosionVfxPrefab;
        [SerializeField] private float _explosionFallbackLife = 2f;
        
        [SerializeField] private float _explosionVfxBaseRadius = 1f;

        private IPoolService _pool;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int DieHash      = Animator.StringToHash("Die");
        private static readonly int ExplodeHash  = Animator.StringToHash("Explode");
        private static readonly int AttackHash   = Animator.StringToHash("Attack");
        private static readonly int ThrowHash = Animator.StringToHash("Throw");
        private static readonly int ThrowSpeedHash = Animator.StringToHash("ThrowSpeed");
        private bool _finished;
        private HashSet<int> _paramHashes;

        public void Construct(IPoolService pool) => _pool = pool;

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            if (_agent == null)
                _agent = GetComponent<NavMeshAgent>();

            CacheAnimatorParams();
        }

        private void CacheAnimatorParams()
        {
            if (_animator == null)
            {
                _paramHashes = null;
                return;
            }

            var ps = _animator.parameters;
            _paramHashes = new HashSet<int>(ps != null ? ps.Length : 0);

            if (ps != null)
                for (int i = 0; i < ps.Length; i++)
                    _paramHashes.Add(ps[i].nameHash);
        }

        private bool HasParam(int hash) =>
            _animator != null && _paramHashes != null && _paramHashes.Contains(hash);

        private void SetBoolSafe(int hash, bool value)
        {
            if (HasParam(hash))
                _animator.SetBool(hash, value);
        }

        private void SetTriggerSafe(int hash)
        {
            if (HasParam(hash))
                _animator.SetTrigger(hash);
        }

        private void Update()
        {
            if (_finished)
                return;

            if (!IsAgentValid())
            {
                SetBoolSafe(IsMovingHash, false);
                return;
            }

            bool moving =
                _agent.remainingDistance > _agent.stoppingDistance + 0.05f &&
                _agent.velocity.sqrMagnitude > 0.01f;

            SetBoolSafe(IsMovingHash, moving);
        }

        private bool IsAgentValid()
        {
            return _agent != null
                   && _agent.isActiveAndEnabled
                   && _agent.gameObject.activeInHierarchy
                   && _agent.isOnNavMesh;
        }

        public void PlayAttack()
        {
            if (_finished) return;
            SetTriggerSafe(AttackHash);
        }

        public void PlayDeath()
        {
            if (_finished) return;
            _finished = true;

            SetBoolSafe(IsMovingHash, false);
            if (_animator != null) _animator.applyRootMotion = false;

            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _agent.isStopped = true;
                _agent.enabled = false;
            }

            SetTriggerSafe(DieHash);
        }
     
        public void PlayThrow(float speed)
        {
            if (_finished || _animator == null) return;
            _animator.SetFloat(ThrowSpeedHash, Mathf.Max(0.05f, speed));
            _animator.SetTrigger(ThrowHash);
        }
        
        // ✅ НОВЕ: вибух з радіусом
        public void PlayExplode(float radius)
        {
            if (_finished) return;
            _finished = true;

            if (_animator != null)
                _animator.applyRootMotion = false;

            SetBoolSafe(IsMovingHash, false);

            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _agent.isStopped = true;
                _agent.enabled = false;
            }

            SetTriggerSafe(ExplodeHash);
            SpawnExplosionVfx(radius);
        }

        // ✅ Backward-compatible (якщо десь ще викликаєш без параметра)
        public void PlayExplode()
        {
            PlayExplode(0f);
        }

        public void ResetForReuse()
        {
            _finished = false;

            if (_animator != null)
            {
                _animator.applyRootMotion = false;
                _animator.Rebind();
                _animator.Update(0f);
            }

            if (_agent != null)
            {
                if (!_agent.enabled)
                    _agent.enabled = true;

                if (_agent.isOnNavMesh)
                    _agent.isStopped = false;
            }

            SetBoolSafe(IsMovingHash, false);
        }

        private void SpawnExplosionVfx(float radius)
        {
            if (_explosionVfxPrefab == null)
                return;

            GameObject vfx;

            if (_pool != null)
            {
                vfx = _pool.Get(_explosionVfxPrefab, transform.position, Quaternion.identity, null);
            }
            else
            {
                vfx = Instantiate(_explosionVfxPrefab, transform.position, Quaternion.identity);

                float life = GetVfxLifetime(vfx);
                if (life <= 0.01f) life = _explosionFallbackLife;
                Destroy(vfx, life);
            }

            // ✅ масштаб під радіус (якщо radius заданий)
            if (vfx != null && radius > 0.001f && _explosionVfxBaseRadius > 0.001f)
            {
                // scale=1 відповідає baseRadius, тому scale = radius/baseRadius
                float k = radius / _explosionVfxBaseRadius;
                vfx.transform.localScale = Vector3.one * k;
            }
        }

        private static float GetVfxLifetime(GameObject vfxRoot)
        {
            if (vfxRoot == null) return 0f;

            float maxLife = 0f;

            var systems = vfxRoot.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < systems.Length; i++)
            {
                var ps = systems[i];
                if (ps == null) continue;

                var main = ps.main;

                float startDelay = GetMax(main.startDelay);
                float startLifetime = GetMax(main.startLifetime);

                float life = startDelay + main.duration + startLifetime;
                if (life > maxLife)
                    maxLife = life;
            }

            if (maxLife > 0f)
                maxLife += 0.25f;

            return maxLife;
        }

        private static float GetMax(ParticleSystem.MinMaxCurve curve)
        {
            return curve.mode switch
            {
                ParticleSystemCurveMode.Constant     => curve.constant,
                ParticleSystemCurveMode.TwoConstants => curve.constantMax,
                ParticleSystemCurveMode.Curve        => curve.constantMax,
                ParticleSystemCurveMode.TwoCurves    => curve.constantMax,
                _                                    => curve.constantMax
            };
        }
    }
}