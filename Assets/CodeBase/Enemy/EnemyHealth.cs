using System;
using CodeBase.Logic;
using UnityEngine;

namespace CodeBase.Enemy
{
    public class EnemyHealth : MonoBehaviour, IHealth
    {
        [SerializeField] private float _maxHealth;
        [SerializeField] private float _currentHealth;
        private bool _isDead;
        public event Action HealthChanged;


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
            if (_isDead)
                return;

            _currentHealth -= damage;
            HealthChanged?.Invoke();

            if (_currentHealth <= 0)
                _isDead = true;
        }
    }
}