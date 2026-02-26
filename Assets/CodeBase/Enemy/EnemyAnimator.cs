using System.Collections.Generic;
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

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int DieHash      = Animator.StringToHash("Die");
        private static readonly int ExplodeHash  = Animator.StringToHash("Explode");
        private static readonly int AttackHash   = Animator.StringToHash("Attack");

        private bool _finished;
        private HashSet<int> _paramHashes;

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

        public void PlayExplode()
        {
            if (_finished) return;
            _finished = true;

            SetBoolSafe(IsMovingHash, false);

            if (_agent != null && _agent.isActiveAndEnabled)
            {
                _agent.isStopped = true;
                _agent.enabled = false;
            }

            SetTriggerSafe(ExplodeHash);

            SpawnExplosionVfx();
        }

        public void ResetForReuse()
        {
            _finished = false;

            if (_animator != null)
            {
                _animator.Rebind();
                _animator.Update(0f);
            }
            if (_agent != null)
            {
                if (!_agent.enabled)
                    _agent.enabled = true;
                _animator.applyRootMotion = false;
                _agent.isStopped = false;
            }

            SetBoolSafe(IsMovingHash, false);
        }

        private void SpawnExplosionVfx()
        {
            if (_explosionVfxPrefab == null)
                return;

            GameObject vfx = Instantiate(_explosionVfxPrefab, transform.position, Quaternion.identity);

            float life = GetVfxLifetime(vfx);
            if (life <= 0.01f)
                life = _explosionFallbackLife;

            Destroy(vfx, life);
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
            switch (curve.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    return curve.constant;

                case ParticleSystemCurveMode.TwoConstants:
                    return curve.constantMax;

                case ParticleSystemCurveMode.Curve:
                case ParticleSystemCurveMode.TwoCurves:
                    return curve.constantMax;

                default:
                    return curve.constantMax;
            }
        }
    }
}