using System;
using UnityEngine;

namespace CodeBase.Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyDeath : MonoBehaviour
    {
        private EnemyHealth _enemyHealth;
        public event Action DeathEvent;

        private void Awake()
        {
            _enemyHealth = GetComponent<EnemyHealth>();
        }

        private void OnEnable()
        {
            _enemyHealth.HealthChanged += HealthChanged;
        }

        private void OnDisable()
        {
            _enemyHealth.HealthChanged -= HealthChanged;
        }

        private void HealthChanged()
        {
            if (_enemyHealth.currentHealth <= 0) // краще через проперті
                Die();
        }

        private void Die()
        {
            DeathEvent?.Invoke();
            Destroy(gameObject);
        }
    }
}