using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    public class EnemyAnimator : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private Animator _animator;
        [SerializeField] private NavMeshAgent agent;

        [Header("Explosion (optional)")]
        [SerializeField] private GameObject _explosionVfxPrefab;
        [SerializeField] private float _destroyDelayAfterDie = 1.2f;
        [SerializeField] private float _destroyDelayAfterExplode = 0.15f;
        
        [SerializeField] private float _explosionFallbackLife = 2f;

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int DieHash = Animator.StringToHash("Die");
        private static readonly int ExplodeHash = Animator.StringToHash("Explode");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        private bool _finished;

        private void Awake()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            if (agent == null)
                agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            if (_finished)
                return;

            if (!IsAgentValid())
            {
                if (_animator != null)
                    _animator.SetBool(IsMovingHash, false);
                return;
            }

            bool moving =
                agent.remainingDistance > agent.stoppingDistance + 0.05f &&
                agent.velocity.sqrMagnitude > 0.01f;

            if (_animator != null)
                _animator.SetBool(IsMovingHash, moving);
        }

        private bool IsAgentValid()
        {
            return agent != null
                   && agent.isActiveAndEnabled
                   && agent.gameObject.activeInHierarchy
                   && agent.isOnNavMesh;
        }

        public void PlayAttack()
        {
            if (_finished) return;
            if (_animator == null) return;

            _animator.SetTrigger(AttackHash);
        }

        public void PlayDeath()
        {
            if (_finished) return;
            _finished = true;

            if (_animator != null)
                _animator.SetBool(IsMovingHash, false);

            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            if (_animator != null)
                _animator.SetTrigger(DieHash);

            Destroy(transform.root.gameObject, _destroyDelayAfterDie);
        }

        public void PlayExplode()
        {
            if (_finished) return;
            _finished = true;

            if (_animator != null)
                _animator.SetBool(IsMovingHash, false);

            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            if (_animator != null)
                _animator.SetTrigger(ExplodeHash);

            SpawnExplosionVfx();

            Destroy(transform.root.gameObject, _destroyDelayAfterExplode);
        }

        private void SpawnExplosionVfx()
        {
            if (_explosionVfxPrefab == null)
                return;

            GameObject vfx = Instantiate(_explosionVfxPrefab, transform.position, Quaternion.identity);

            // порахувати реальний лайфтайм по всіх particle системах
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

                // duration + delay + lifetime = приблизний час “життя”
                float life = startDelay + main.duration + startLifetime;
                if (life > maxLife)
                    maxLife = life;
            }

            // safety буфер щоб не обрізало останні частинки
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
                    // приблизно, беремо constantMax як верхню оцінку
                    return curve.constantMax;

                default:
                    return curve.constantMax;
            }
        }
    }
}
