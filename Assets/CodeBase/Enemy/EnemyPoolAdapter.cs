using CodeBase.GameLogic.Pool;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyDeath))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyPoolAdapter : MonoBehaviour, IPoolable
    {
        private EnemyDeath _death;
        private EnemyHealth _health;

        private EnemyAnimator _anim;
        private NavMeshAgent _agent;

        private int _aliveLayer;

        private void Awake()
        {
            _death = GetComponent<EnemyDeath>();
            _health = GetComponent<EnemyHealth>();

            _anim = GetComponent<EnemyAnimator>();
            _agent = GetComponent<NavMeshAgent>();

            _aliveLayer = gameObject.layer; // запам’ятали “живий” слой
        }

        private void OnEnable()
        {
            _death.DeathEvent += OnDied;
        }

        private void OnDisable()
        {
            _death.DeathEvent -= OnDied;
        }

        private void OnDied()
        {

        }

        public void OnSpawned()
        {
            // 1) скинути death flag
            _death.ResetDeathState();

            // 2) revive health-state
            _health.Revive();

            // 3) повернути layer (бо в Die() ти ставиш Dead)
            foreach (var t in GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = _aliveLayer;

            // 4) повернути колайдери
            foreach (var c in GetComponentsInChildren<Collider>(true))
                c.enabled = true;

            // 5) повернути NavMeshAgent (бо вимикався в Die/Animator)
            if (_agent != null)
            {
                if (!_agent.enabled)
                    _agent.enabled = true;

                if (_agent.isOnNavMesh)
                {
                    _agent.ResetPath();
                    _agent.velocity = Vector3.zero;
                }

                _agent.isStopped = false;
            }
            // 6) скинути аніматор під reuse (finished + rebind)
            _anim?.ResetForReuse();
        }

        public void OnDespawned()
        {

        }
    }
}