using System;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyHealth : MonoBehaviour, IHealth
    {
        private float _maxHealth;
        private float _currentHealth;
        private bool _isDead;

        public event Action HealthChanged;
        public bool IsDead => _isDead;

        public float maxHealth
        {
            get => _maxHealth;
            set => _maxHealth = value;
        }

        public float currentHealth
        {
            get => _currentHealth;
            set => _currentHealth = value;
        }

        public void TakeDamage(float damage)
        {
            Debug.Log($"[HP] {name} before={_currentHealth} dmg={damage}");

            if (_isDead) return;
            if (damage <= 0f) return;

            _currentHealth -= damage;

            if (_currentHealth <= 0f)
            {
                _currentHealth = 0f;
                _isDead = true;
            }
            Debug.Log($"[HP] {name} after={_currentHealth}");

            HealthChanged?.Invoke();
        }

        public void Heal(float amount)
        {
            if (_isDead) return;
            if (amount <= 0f) return;

            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            HealthChanged?.Invoke();
        }
    }
}