using System;
using CodeBase.GameLogic.Pool;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyDeath : MonoBehaviour
    {
        public enum DeathType { Normal, Explode }
        [SerializeField] private DeathType _deathType = DeathType.Normal;

        [Header("Delays")]
        [SerializeField] private float _releaseDelayAfterDie = 1.2f;
        [SerializeField] private float _releaseDelayAfterExplode = 0.15f;

        private EnemyHealth _health;
        private NavMeshAgent _agent;
        private EnemyAnimator _anim;
        private PooledObject _pooled;

        public event Action DeathEvent;

        private bool _dead;
        private int _aliveLayer;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _agent  = GetComponent<NavMeshAgent>();
            _anim   = GetComponent<EnemyAnimator>();
            _pooled = GetComponent<PooledObject>();

            _aliveLayer = gameObject.layer;
        }

        private void OnEnable()
        {
            _health.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            _health.HealthChanged -= OnHealthChanged;

            // ✅ щоб не було “виліз після реліза/пула” через Invoke з минулого життя
            CancelInvoke(nameof(ReleaseOrDestroy));
        }

        // ✅ викликай при OnSpawned() з пула
        public void ResetDeathState()
        {
            _dead = false;

            CancelInvoke(nameof(ReleaseOrDestroy));

            _health.Revive();

            foreach (var t in GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = _aliveLayer;

            foreach (var c in GetComponentsInChildren<Collider>(true))
                c.enabled = true;

            if (_agent != null)
            {
                if (!_agent.enabled)
                    _agent.enabled = true;

                _agent.isStopped = false;
            }

            _anim?.ResetForReuse();
        }

        private void OnHealthChanged()
        {
            if (_dead) return;
            if (_health.currentHealth > 0) return;

            Die();
        }

        private void Die()
        {
            _dead = true;

            GetComponent<EnemyKnockback>()?.Cancel();

            if (_agent != null)
                _agent.enabled = false;

            int deadLayer = LayerMask.NameToLayer("Dead");
            foreach (var t in GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = deadLayer;

            foreach (var c in GetComponentsInChildren<Collider>(true))
                c.enabled = false;

            DeathEvent?.Invoke();

            if (_anim == null)
            {
                ReleaseOrDestroy();
                return;
            }

            if (_deathType == DeathType.Explode)
            {
                _anim.PlayExplode();
                Invoke(nameof(ReleaseOrDestroy), _releaseDelayAfterExplode);
            }
            else
            {
                _anim.PlayDeath();
                Invoke(nameof(ReleaseOrDestroy), _releaseDelayAfterDie);
            }
        }

        private void ReleaseOrDestroy()
        {
            if (!_dead) return; // safety

            if (_pooled == null)
                _pooled = GetComponent<PooledObject>();

            if (_pooled != null)
                _pooled.Release();
            else
                Destroy(gameObject);
        }
    }
}