using System;
using UnityEngine;
using UnityEngine.AI;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyDeath : MonoBehaviour
    {
        public enum DeathType { Normal, Explode }

        [SerializeField] private DeathType _deathType = DeathType.Normal;

        private EnemyHealth _health;
        private NavMeshAgent _agent;

        public event Action DeathEvent;

        private bool _dead;

        private void Awake()
        {
            _health = GetComponent<EnemyHealth>();
            _agent = GetComponent<NavMeshAgent>();
        }

        private void OnEnable()  => _health.HealthChanged += OnHealthChanged;
        private void OnDisable() => _health.HealthChanged -= OnHealthChanged;

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

            // ✅ 1) move to Dead layer
            int deadLayer = LayerMask.NameToLayer("Dead");
            foreach (var t in GetComponentsInChildren<Transform>(true))
                t.gameObject.layer = deadLayer;

            // ✅ 2) disable all colliders
            foreach (var c in GetComponentsInChildren<Collider>(true))
                c.enabled = false;

            DeathEvent?.Invoke();

            var anim = GetComponent<EnemyAnimator>();
            if (anim == null)
            {
                Destroy(gameObject);
                return;
            }

            if (_deathType == DeathType.Explode)
                anim.PlayExplode();
            else
                anim.PlayDeath();
        }

    }
}